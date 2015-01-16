using System;
using SQLite.Tools;

namespace SQLite.Demo.CRUD
{
    public class User : ISQLiteReadFrom, ISQLiteWriteTo
    {
        public const string TableName = "Users";
        public const string ViewName = "UsersView";

        string ISQLiteReadFrom.Table
        {
            get { return ViewName; }
        }

        string ISQLiteWriteTo.Table
        {
            get { return TableName; }
        }

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