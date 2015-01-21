using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreTechs.Common;
using CoreTechs.Common.Database;
using UniqueNamespace;

namespace SQLite.Tools
{
    public static class Extensions
    {
        internal static string ReadText(this IEnumerable<byte> bytes, Encoding encoding = null)
        {
            using (var stream = bytes.ToMemoryStream())
            using (var reader = encoding == null 
                ? new StreamReader(stream) 
                : new StreamReader(stream, encoding))
                return reader.ReadToEnd();
        }

        internal static byte[] GetEmbeddedResource(this Assembly assembly, string name)
        {
            using (var stream = assembly.GetManifestResourceStream(name))
                return stream.EnumerateBytes().ToArray();
        }

        internal static string[] GetEmbeddedResourceNames(this Type type)
        {
            var ns = type.Namespace + ".";
            var names = type.Assembly.GetManifestResourceNames().Where(x => x.StartsWith(ns));
            return names.ToArray();
        }

        public static ResolvedSql<TParamsOut> Resolve<TParamsIn, TParamsOut>(this SqlBuilderBase<TParamsIn, TParamsOut>.Template template)
            where TParamsIn : class
            where TParamsOut : class
        {
            if (template == null) throw new ArgumentNullException("template");

            var resolved = new ResolvedSql<TParamsOut>(template.RawSql, template.Parameters);
            return resolved;
        }

        public static T Scalar<T>(this DataTable dataTable)
        {
            if (dataTable == null) throw new ArgumentNullException("dataTable");
            var value = dataTable.AsEnumerable().Select(x => x[0].ConvertTo<T>()).FirstOrDefault();
            return value;
        }

        public static DataSet Query(this IDbConnection conn, PreparedStatement statement)
        {
            return conn.QuerySql(statement.Sql, statement.Parameters.Cast<DbParameter>().ToArray());
        }

        public static Task<DataSet> QueryAsync(this IDbConnection conn, PreparedStatement statement)
        {
            return conn.QuerySqlAsync(statement.Sql, statement.Parameters.Cast<DbParameter>().ToArray());
        }
       
        public static void Execute(this IDbConnection conn, PreparedStatement statement)
        {
            conn.ExecuteSql(statement.Sql, statement.Parameters.Cast<DbParameter>().ToArray());
        }

        public static Task ExecuteAsync(this IDbConnection conn, PreparedStatement statement)
        {
            return conn.ExecuteSqlAsync(statement.Sql, statement.Parameters.Cast<DbParameter>().ToArray());
        }
    }
}