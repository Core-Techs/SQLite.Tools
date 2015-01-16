using System;
using System.Reflection;

namespace SQLite.Tools
{
    public class ColumnPropertyPair
    {
        public ColumnPropertyPair(SQLiteColumn column, PropertyInfo property)
        {
            if (column == null) throw new ArgumentNullException("column");
            if (property == null) throw new ArgumentNullException("property");
            Column = column;
            Property = property;
            ParameterName = "@" + Property.Name;
        }

        public SQLiteColumn Column { get; private set; }
        public PropertyInfo Property { get; private set; }
        public string ParameterName { get; private set; }
    }
}