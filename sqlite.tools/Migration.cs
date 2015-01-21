using System;
using System.Linq;
using CoreTechs.Common;

namespace SQLite.Tools
{
    public class Migration : ISQLiteEntity
    {
        public const string TableName = "Migrations";

        public string ReadSource
        {
            get { return TableName; }
        }

        public string WriteDestination
        {
            get { return TableName; }
        }

        public decimal Version { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Ran { get; set; }
        public string SQL { get; set; }
        public string Error { get; set; }

        public static decimal ParseVersionFromName(string name)
        {
            var dot = false;
            var parts = name
                .TakeWhile(c =>
                {
                    if (c != '.')
                        return Characters.Keyboard.Digits.Contains(c);

                    if (dot)
                        return false;

                    return dot = true;
                })
                .StringConcat();

            var v = decimal.Parse(parts);
            return v;
        }
    }
}