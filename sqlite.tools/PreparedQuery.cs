using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace SQLite.Tools
{
    /// <summary>
    /// Encapsulates a sql statement and logic used to transform results into an object graph.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PreparedQuery<T>
    {
        private readonly Func<DataSet, T> _transformation;
        public PreparedStatement Statement { get; private set; }

        public PreparedQuery(PreparedStatement statement, Func<DataSet, T> transformation)
        {
            if (statement == null) throw new ArgumentNullException("statement");
            if (transformation == null) throw new ArgumentNullException("transformation");

            Statement = statement;
            _transformation = transformation;
        }

        public async Task<T> ExecuteAsync(SQLiteConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var results = await connection.QueryAsync(Statement);
            return _transformation(results);
        }
    }
}