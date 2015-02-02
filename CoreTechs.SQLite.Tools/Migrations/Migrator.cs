using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CoreTechs.Common;
using CoreTechs.Common.Database;

namespace CoreTechs.SQLite.Tools.Migrations
{
    /// <summary>
    /// Executues migrations on a database.
    /// </summary>
    public class Migrator
    {
        private readonly SQLiteConnection _db;
        private readonly Migration[] _migrations;

        public Migrator(SQLiteConnection connection, IEnumerable<Migration> migrations)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (migrations == null) throw new ArgumentNullException("migrations");

            _db = connection;
            _migrations = migrations.ToArray();
        }

        public void MigrateToVersion(decimal targetVersion = decimal.MaxValue)
        {
            EnsureMigrationsTableExists();
            var history = GetMigrationHistory();
            var currentVersion = history.Where(x => x.Error.IsNullOrEmpty())
                    .Select(x => x.Version)
                    .OrderByDescending(x => x)
                    .FirstOrDefault();

            // no work to do?
            if (currentVersion == targetVersion)
                return;

            var backup = BackupDatabase();

            var up = currentVersion < targetVersion;

            var migrations = up

                // going up
                ? _migrations.Where(m => m.Version > currentVersion && m.Version <= targetVersion)
                             .OrderBy(x => x.Version).ToArray()

                // going down
                : _migrations.Where(m => m.Version <= currentVersion && m.Version > targetVersion)
                             .OrderByDescending(x => x.Version).ToArray();

            foreach (var migration in migrations)
            {
                try
                {
                    var schemaVersion = _db.ScalarSql<long>("PRAGMA schema_version;");

                    // sql script as defined by developer
                    var rawSql = migration.SQL;

                    // sql with placeholders replaced, etc.
                    var finalSql = ProcessSql(rawSql, up, schemaVersion);

                    _db.ExecuteSql(finalSql);

                    // make sure database isn't screwed up
                    CheckIntegrity();
                    CheckForeignKeys();
                }
                catch (Exception ex)
                {
                    migration.Error = ex.ToString();
                    RecordMigration(_db, migration, up);
                    throw;
                }

                RecordMigration(_db, migration, up);
            }

            if (backup != null)
                backup.Delete();
        }

        private void CheckForeignKeys()
        {
            var results = _db.QuerySql("PRAGMA foreign_key_check;").Tables[0];

            if (results.Rows.Count == 0)
                return;

            var ex = new SQLiteException("foreign key check failed");
            var failures = new StringWriter();
            results.WriteXml(failures);
            ex.Data["sqlite_fk_failures"] = failures.ToString();
            throw ex;
        }

        private void CheckIntegrity()
        {
            var results =
                _db.QuerySql("PRAGMA integrity_check;").AsEnumerable().Select(x => x.Field<string>(0)).ToArray();

            if (results.Length == 1 && results.First().Equals("ok", StringComparison.OrdinalIgnoreCase))
                return;

            var ex = new SQLiteException("integrity check failed");
            ex.Data["sqlite_integrity_failures"] = results.Join(Environment.NewLine);
            throw ex;
        }

        /// <summary>
        /// Modifies the sql.
        /// </summary>
        private static string ProcessSql(string sql, bool up, long schemaVersion)
        {
            var keep = up ? UpRegex : DownRegex;
            var noKeep = up ? DownRegex : UpRegex;

            // remove sql that shouldn't be kept
            sql = noKeep.Replace(sql, string.Empty);

            var matches = keep.Matches(sql);
            foreach (var match in matches.Cast<Match>().OrderByDescending(x => x.Index))
            {
                // iterating matches in reverse (from end of string to beginning)
                // because we're mutating the string and match index/length properties
                // become stale if we remove tags from left-most to right-most

                var inner = match.Groups["sql"].Value;
                sql = sql.Remove(match.Index, match.Length).Insert(match.Index, inner);

                // removed keeper <TAG> and </TAG>
            }

            var incrementedSchemaVersion = schemaVersion + 1;
            sql = sql.Replace("{{incremented_schema_version}}", incrementedSchemaVersion.ToString());
            return sql;
        }

        private Migration[] GetMigrationHistory()
        {
            var history = _db.SelectAsync<Migration>().Result.ToArray();
            return history;
        }

        private void RecordMigration(SQLiteConnection conn, Migration migration, bool up = true)
        {
            // delete from migration table where version matches
            conn.DeleteAsync(new { migration.Version }, Migration.TableName).Wait();
            migration.Ran = DateTime.Now;

            var error = !migration.Error.IsNullOrEmpty();

            if (error || up)
                conn.InsertAsync(migration);
        }

        private FileInfo BackupDatabase()
        {
            if (_db.IsInMemory())
                return null;

            var dbInfo = _db.GetConnectionInfo();

            var source = new FileInfo(dbInfo.DataSource);
            if (!source.Exists) return null;

            var destination = GetBackupFileInfo(source);
            source.CopyTo(destination.FullName, true);
            return destination;
        }

        private static FileInfo GetBackupFileInfo(FileInfo source)
        {
            return source.Directory.GetFile(String.Format("{0}.{1}.migbak", source.Name, DateTime.Now.Ticks));
        }

        private void EnsureMigrationsTableExists()
        {
            _db.ExecuteSql("CREATE TABLE IF NOT EXISTS Migrations (" +
                           "Version NUMERIC PRIMARY KEY NOT NULL, " +
                           "Name TEXT NOT NULL, " +
                           "Ran TEXT NOT NULL, " +
                           "SQL TEXT, " +
                           "Error TEXT)");
        }

        private static readonly Regex UpRegex = new Regex(@"<UP>(?<sql>.*?)<\/UP>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex DownRegex = new Regex(@"<DOWN>(?<sql>.*?)<\/DOWN>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private string ExtractDownSql(string sql)
        {
            if (sql == null) throw new ArgumentNullException("sql");

            var match = DownRegex.Match(sql);

            if (!match.Success)
                return null;

            var downSql = match.Groups["sql"].Value;
            return downSql;
        }

        private string ExtractUpSql(string sql)
        {
            return DownRegex.Replace(sql, String.Empty);
        }
    }
}
