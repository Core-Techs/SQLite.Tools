using System;
using CoreTechs.Common.Database;
using CoreTechs.SQLite.Tools;
using NUnit.Framework;
using SQLite.Demo.GetStarted;

namespace SQLite.Demo.CRUD
{
    [TestFixture]
    public class Demo : DemoTestBase
    {
        [Test, Explicit]
        public async void InsertUser()
        {
            using (var conn = CreateConnection())
            {
                var user = new User
                {
                    UserName = "roverby",
                    BirthDate = new DateTime(1984, 5, 10)
                };

                await conn.InsertAsync(user);

                Console.WriteLine(user.Id);
                //Assert.AreNotEqual(0, user.Id);
            }
        }

        [Test, Explicit]
        public async void UpdateUser()
        {
            using (var conn = CreateConnection())
            {
                var userId = conn.ScalarSql<int>("SELECT Id FROM Users LIMIT 1");

                // we have a user id

                var user = new User
                {
                    Id = userId,
                    Email = "roverby@core-techs.net"
                };

                // we have the updates we want to perform
                // we used the user class for type safety

                await conn.UpdateAsync(new
                {
                    user.Id,
                    user.Email
                }, User.TableName);

                // we have updated the row partially
                // that's why we used an anonymous object
                // and not the User class object
                // we could have passed in the User object
                // directly (and omitted the table name)
                // but that would have updated every column
            }
        }

        [Test, Explicit]
        public async void DeleteUser()
        {
            using (var conn = CreateConnection())
            {
                var userId = conn.ScalarSql<int>("SELECT Id FROM Users LIMIT 1");

                // we have a user id

                var user = new User {Id = userId};

                // we have a user object that has the needed information
                // to uniquely identifiy which row to delete

                await conn.DeleteAsync(user);

                // we have deleted the user from the database
            }
        }

        [Test, Explicit]
        public async void SelectUsers()
        {
            using (var conn = CreateConnection())
            {
                var query = new
                {
                    UserName = "roverby"
                };

                // we have an object that represents our query:
                // give me rows with an email that matches "roverby"

                var users = await conn.SelectAsync<User>(query);

                // we have our database results for our query
                // this method of querying the database is limited
                // to only querying by equality
                // for inequality or other comparisons
                // you'll need to write SQL

                foreach (var user in users)
                {
                    Console.WriteLine(new {user.Id, user.UserName, user.Age});
                }
            }
        }

        [Test, Explicit]
        public async void ComplexQuery()
        {
            using (var conn = CreateConnection())
            {
                // use coretechs.common extensions methods
                // on IDbConnection to execute arbitrary sql

                // use UniqueNamespace.SqlBuilder or anything else
                // to help construct said arbitrary sql

                throw new NotImplementedException();

            }
        }
    }


    
}
