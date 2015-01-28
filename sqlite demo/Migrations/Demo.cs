using CoreTechs.SQLite.Tools.Migrations;
using NUnit.Framework;
using SQLite.Demo.GetStarted;

namespace SQLite.Demo.Migrations
{
    [TestFixture]
    public class Demo : DemoTestBase
    {
        [Test, Explicit]
        public void MigrateToVersion1()
        {
            var migrations = Harvester.GetMigrations();

            // we have migration objects from the embedded resources found in this assembly

            using (var connection = CreateConnection())
            {
                var migrator = new Migrator(connection, migrations);
                migrator.MigrateToVersion( 1 );

                // we have updated the database to the latest version
            }
        }

        [Test, Explicit]
        public void MigrateToVersion0_ClearSchema()
        {
            var migrations = Harvester.GetMigrations();

            // we have migration objects from the embedded resources found in this assembly

            using (var connection = CreateConnection())
            {
                var migrator = new Migrator(connection, migrations);
                migrator.MigrateToVersion( 0 );

                // we have updated the database to the latest version
            }
        }

        [Test, Explicit]
        public void RunMigrations()
        {
            var migrations = Harvester.GetMigrations();

            // we have migration objects from the embedded resources found in this assembly

            using (var connection = CreateConnection())
            {
                var migrator = new Migrator(connection, migrations);
                migrator.MigrateToVersion( /*latest*/ );

                // we have updated the database to the latest version
            }
        }
    }
}
