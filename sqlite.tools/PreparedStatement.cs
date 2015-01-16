using System;
using System.Data.SQLite;
using System.Linq;

namespace SQLite.Tools
{
    public class PreparedStatement
    {
        public string Sql { get; set; }
        public SQLiteParameter[] Parameters { get; set; }

        public PreparedStatement(string sql, params SQLiteParameter[] parameters)
        {
            Sql = sql;
            Parameters = parameters;
        }

        public PreparedStatement Merge(PreparedStatement other)
        {
            if (other == null)
                return this;

            var sql = Sql + ";" + Environment.NewLine + other.Sql;
            var paramGroups = Enumerable.Concat(Parameters, other.Parameters).GroupBy(x => new { x.ParameterName }).ToArray();

            if (paramGroups.Any(g => g.Select(p => p.Value).Distinct().Count() != 1))
                throw new InvalidOperationException("Irreconcilable Parameters");

            var parameters = paramGroups.Select(g => g.First()).ToArray();

            return new PreparedStatement(sql, parameters);
        }

        public PreparedStatement Merge(string sql, params SQLiteParameter[] parameters)
        {
            if (sql == null) throw new ArgumentNullException("sql");

            return Merge(new PreparedStatement(sql, parameters));
        }

        public static implicit operator PreparedStatement(string sql)
        {
            return sql == null ? null : new PreparedStatement(sql);
        }
    }
}