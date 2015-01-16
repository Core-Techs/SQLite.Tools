using System.Data.SQLite;

namespace SQLite.Demo.GetStarted
{
    public class DemoTestBase
    {
        protected SQLiteConnection CreateConnection()
        {
            var connStrings = new ConnectionStrings();
            return new SQLiteConnection(connStrings.Demo.ConnectionString);
        }
    }
}