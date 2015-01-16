using System;
using System.Diagnostics;
using System.Linq;

namespace SQLite.Tools.Migrations
{
    public static class Harvester
    {
        /// <summary>
        /// Returns migrations found as embedded resources in this part of the assembly.
        /// </summary>
        public static Migration[] GetMigrations(Type type = null, Func<string, bool> resourcePredicate = null)
        {
            if (type == null)
            {
                // this only works when the caller is in the assembly containing
                // the resources
                var frame = new StackTrace().GetFrame(1);
                type = frame.GetMethod().DeclaringType;
            }

            var migrations = from name in type.GetEmbeddedResourceNames()
                             where (resourcePredicate == null && name.EndsWith("sql", StringComparison.OrdinalIgnoreCase)) || resourcePredicate(name)
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
