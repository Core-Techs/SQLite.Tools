using System;
using System.Data;
using CoreTechs.Common.Database;

namespace CoreTechs.SQLite.Tools
{
    public class SQLiteColumn
    {
        public SQLiteColumn(DataRow row)
        {
            var ti = row.AsDynamic();
            Name = (string)ti.name;
            Type = (string)ti.type;
            NotNull = ti.notnull == 1;
            PK = ti.pk == 1;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public bool NotNull { get; set; }
        public bool PK { get; set; }

        public bool RowId
        {
            get { return PK && Type.Trim().Equals("INTEGER", StringComparison.OrdinalIgnoreCase); }
        }
    }
}