using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTechs.Common;
using CoreTechs.Common.Database;
using CoreTechs.Common.Reflection;

namespace SQLite.Tools
{
    public static class SQLiteExtensions
    {
        static SQLiteExtensions()
        {
            // this registers type converters that exist in the coretechs
            // common library. these conversions are especially useful
            // for creating object instances from datasets
            // but because it can change behavior app-wide
            // the custom converters are opt-in and not registered automatically
            ConversionExtensions.RegisterAllCustomTypeConverters();
        }

        public static SQLiteConnection CreateConnection(this SQLiteConnectionStringBuilder csb)
        {
            if (csb == null) throw new ArgumentNullException("csb");
            return new SQLiteConnection(csb.ConnectionString);
        }

        public static SQLiteConnectionStringBuilder GetConnectionInfo(this SQLiteConnection conn)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            return new SQLiteConnectionStringBuilder(conn.ConnectionString);
        }

        public static SQLiteParameter AsSQLiteParam(this object obj, string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            if (obj is DateTime)
                obj = ((DateTime)obj).ToString("o");

            if (obj is DateTimeOffset)
                obj = ((DateTimeOffset)obj).ToString("o");

            return new SQLiteParameter(name, obj);
        }

        public static Task UpdateAsync(this SQLiteConnection conn, ISQLiteWriteTo obj)
        {
            return UpdateAsync(conn, obj, obj.WriteDestination);
        }

        public static Task UpdateAsync(this SQLiteConnection conn, object obj, string table)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var data = (from pair in GetColumnPropertyPairs(conn, table, obj.GetType())
                        where pair.Property.CanRead
                        let value = pair.Property.GetValue(obj)
                        let param = value.AsSQLiteParam(pair.ParameterName)
                        select new
                        {
                            pair,
                            param
                        }).ToArray();

            var values = data.Where(x => !x.pair.Column.PK)
                .Select(x => string.Format("\"{0}\" = {1}", x.pair.Column.Name, x.param.ParameterName))
                .Join(", ");

            var predicate = data.Where(x => x.pair.Column.PK)
                .Select(x => string.Format("\"{0}\" = {1}", x.pair.Column.Name, x.param.ParameterName))
                .Join(" AND ");

            var sql = string.Format("UPDATE \"{0}\" SET {1} ", table, values);

            if (!predicate.IsNullOrWhiteSpace())
                sql += "WHERE " + predicate;

            var parameters = data.Select(x => x.param).ToArray();
            return conn.ExecuteSqlAsync(sql, parameters);
        }

        public static Task DeleteAsync(this SQLiteConnection conn, ISQLiteWriteTo obj)
        {
            return DeleteAsync(conn, obj, obj.WriteDestination);
        }

        public static Task DeleteAsync(this SQLiteConnection conn, object obj, string table)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var columns = GetColumns(conn, table);
            var pkSize = columns.Count(x => x.PK);

            var data = (from pair in GetColumnPropertyPairs(conn, table, obj.GetType())
                        where pair.Property.CanRead
                        let value = pair.Property.GetValue(obj)
                        let param = value.AsSQLiteParam(pair.ParameterName)
                        select new
                        {
                            pair,
                            param
                        }).ToArray();

            var sql = string.Format("DELETE FROM \"{0}\" ", table);
            var pkSpecified = pkSize > 0 && data.Count(x => x.pair.Column.PK) == pkSize;
            var predicate = data

                // if the pk is specified use only it for criteria
                // otherwise use every value in obj
                .Where(x => x.pair.Column.PK || !pkSpecified)

                .Select(x => string.Format("\"{0}\" = {1}", x.pair.Column.Name, x.param.ParameterName))
                .Join(" AND ");

            if (!predicate.IsNullOrWhiteSpace())
                sql += string.Format(" WHERE {0}", predicate);

            var parameters = data.Select(x => x.param).ToArray();
            return conn.ExecuteSqlAsync(sql, parameters);
        }

        public static Task InsertAsync(this SQLiteConnection conn, ISQLiteWriteTo obj)
        {
            return InsertAsync(conn, obj, obj.WriteDestination);
        }

        public static async Task InsertAsync(this SQLiteConnection conn, object obj, string table)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var data = (from pair in GetColumnPropertyPairs(conn, table, obj.GetType())
                        where pair.Property.CanRead
                        let value = pair.Property.GetValue(obj)

                        // dont insert rowid value if value is null or zero
                        let defaultRowId = pair.Column.RowId && (ReferenceEquals(value, null) || value.Equals(0))
                        let param = value.AsSQLiteParam(pair.ParameterName)
                        select new
                        {
                            defaultRowId,
                            pair,
                            param
                        }).ToArray();

            if (!data.Any())
                throw new DataException("There are no matching column name for the properties of type " + obj.GetType());


            var paramData = data.WhereNot(x => x.defaultRowId).ToArray();
            var columns = paramData.Select(x => string.Format("\"{0}\"", x.pair.Column.Name)).Join(", ");
            var values = paramData.Select(x => x.param.ParameterName).Join(", ");

            var sql = string.Format("INSERT INTO \"{0}\" ({1}) VALUES ({2})", table, columns, values);

            var parameters = data.Select(x => x.param).ToArray();

            using (await conn.ConnectAsync())
            {
                await conn.ExecuteSqlAsync(sql, parameters);

                data.Where(x => x.defaultRowId)
                    .ForEach(
                        x => x.pair.Property.SetValue(obj, conn.LastInsertRowId.ConvertTo(x.pair.Property.PropertyType)));
            }
        }

        public static Task<IEnumerable<T>> SelectAsync<T>(this SQLiteConnection conn, object obj)
            where T : class, ISQLiteReadFrom, new()
        {
            return SelectAsync<T>(conn, obj, new T().ReadSource);
        }

        public static Task<IEnumerable<T>> SelectAsync<T>(this SQLiteConnection conn)
             where T : class, ISQLiteReadFrom, new()
        {
            return SelectAsync<T>(conn, new { });
        }

        public static Task<IEnumerable<T>> SelectAsync<T>(this SQLiteConnection conn, string table)
            where T : class, new()
        {
            return SelectAsync<T>(conn, new { }, table);
        }

        public static async Task<IEnumerable<T>> SelectAsync<T>(this SQLiteConnection conn, object obj, string table) where T : class, new()
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var data = (from pair in GetColumnPropertyPairs(conn, table, obj.GetType())
                        where pair.Property.CanRead
                        let value = pair.Property.GetValue(obj)
                        let param = value.AsSQLiteParam(pair.ParameterName)
                        select new
                        {
                            pair,
                            param
                        }).ToArray();

            var predicate = data
                .Select(x => string.Format("\"{0}\" = {1}", x.pair.Column.Name, x.param.ParameterName))
                .Join(" AND ");

            var sql = new StringBuilder()
                .AppendFormat("SELECT * FROM \"{0}\" ", table);

            if (!predicate.IsNullOrWhiteSpace())
                sql.AppendFormat(" WHERE {0}", predicate);

            var parameters = data.Select(x => x.param).ToArray();
            var results = await conn.QuerySqlAsync(sql.ToString(), parameters);

            return results.AsEnumerable<T>();
        }


        public static SQLiteColumn[] GetColumns(this SQLiteConnection conn, string table)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (table == null) throw new ArgumentNullException("table");

            var keyData = new { Table = table.ToLowerInvariant(), conn.ConnectionString };
            return Memoizer.Instance.Get(keyData, () =>
            {
                var cols = conn.QuerySql(string.Format("PRAGMA table_info({0})", table))
                    .AsEnumerable()
                    .Select(row => new SQLiteColumn(row))
                    .OrderByDescending(x => x.PK)
                    .ToArray();

                return cols;
            });
        }

        public static ColumnPropertyPair[] GetColumnPropertyPairs(this SQLiteConnection conn, string table, Type type)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (table == null) throw new ArgumentNullException("table");
            if (type == null) throw new ArgumentNullException("type");

            var keyData = new { Table = table.ToLowerInvariant(), conn.ConnectionString, type };
            return Memoizer.Instance.Get(keyData, () =>
            {
                var cols = GetColumns(conn, table);

                var pairs = (from p in type.GetPropertiesAsDeclared()
                             where p != null
                             let c = cols.FirstOrDefault(c => c.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
                             where c != null
                             select new ColumnPropertyPair(c, p)).ToArray();

                return pairs;
            });
        }

        /// <summary>
        /// Copies the database from source to destination, changing password if needed.
        /// </summary>
        /// <remarks>
        /// Not intended for use when the source or destination datasource is not a file system path
        /// (in memory databases).
        /// </remarks>
        public static void CopyDatabase(this SQLiteConnectionStringBuilder source, SQLiteConnectionStringBuilder destination)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");

            var temp = new SQLiteConnectionStringBuilder(source.ConnectionString);
            temp.DataSource = temp.DataSource + ".temp";

            using (var conn1 = source.CreateConnection())
            using (var conn2 = temp.CreateConnection())
            using (conn1.Connect())
            using (conn2.Connect())
            {
                conn1.BackupDatabase(conn2, "main", "main", -1, null, -1);

                if (source.Password != destination.Password)
                    conn2.ChangePassword(destination.Password);
            }

            // move temp to destination
            File.Copy(temp.DataSource, destination.DataSource, true);
            File.Delete(temp.DataSource);
        }

        public static bool IsInMemory(this SQLiteConnectionStringBuilder info)
        {
            return info.ConnectionString.Contains(":memory:", StringComparison.OrdinalIgnoreCase)
                   || info.ConnectionString.Contains("mode=memory", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsInMemory(this SQLiteConnection connection)
        {
            return connection.GetConnectionInfo().IsInMemory();
        }
    }
}

