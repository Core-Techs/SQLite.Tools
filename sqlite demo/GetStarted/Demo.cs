using System;
using System.Data.SQLite;
using CoreTechs.Common.Database;
using NUnit.Framework;

namespace SQLite.Demo.GetStarted
{
    [TestFixture]
    public class Demo : DemoTestBase
    {
        [Test]
        public void BuildAConnectionString()
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = @"C:\Users\roverby\Desktop\demo.sqlite"
            };

            // we have a connection string
            // I've pasted the output into app.config

            Console.WriteLine(builder.ConnectionString);
        }

        [Test]
        public void OpenAConnection()
        {
            using (var conn = CreateConnection())
            using(conn.Connect())
            {
                // we can connect!
            }
        }
    }
}
