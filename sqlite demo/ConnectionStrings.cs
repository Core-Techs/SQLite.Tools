using System.Configuration;

namespace SQLite.Demo
{
    public class ConnectionStrings : CoreTechs.Common.ConnectionStrings
    {
        public ConnectionStringSettings Demo
        {
            get { return GetConnectionString(); }
        }
    }
}