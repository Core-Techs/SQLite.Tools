namespace SQLite.Tools
{
    public class ResolvedSql<T>
    {
        public ResolvedSql(string sql, T parameters)
        {
            SQL = sql;
            Parameters = parameters;
        }

        public string SQL { get; private set; }
        public T Parameters { get; private set; }
    }
}