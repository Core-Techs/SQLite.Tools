using System;
using SQLite.Tools;

namespace SQLite.Demo.CRUD
{
    public class User : ISQLiteEntity
    {
        public const string TableName = "Users";
        public const string ViewName = "UsersView";

        public string ReadSource { get { return ViewName; } }
        public string WriteDestination { get { return TableName; } }

        public int Id { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Computed by database view.
        /// </summary>
        public int Age { get; private set; }
    }
}