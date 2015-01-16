using System.Linq;

namespace SQLite.Tools.Migrations
{
    public static class Harvester
    {
        /// <summary>
        /// Returns migrations found as embedded resources in this part of the assembly.
        /// </summary>
        public static Migration[] GetMigrations()
        {
            var type = typeof (Harvester);

            var migrations = from name in type.GetEmbeddedResourceNames()
                let n = name.TrimStart((type.Namespace + ".").ToCharArray())
                let sql = type.Assembly.GetEmbeddedResource(name).ReadText()
                select new Migration
                {
                    Name = n,
                    Version = Migration.ParseVersionFromName(n),
                    SQL = sql
                };

            return migrations.ToArray();
        }
    }
}
