using SquirrelDemoApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDemoApp.DAL
{
    public sealed class Repository
    {
        private readonly string _connectionstring;

        public Repository(string connectionstring)
        {
            _connectionstring = connectionstring;
        }

        public string GetDatabaseServerName()
        {
            using (var ctx = new TestDbContext(_connectionstring))
            {
                var sql = "SELECT @@SERVERNAME AS Field1";

                var serverName = 
                    ctx.Database.SqlQuery<SqlQueryResult<string>>(sql)
                    .Single().Field1;

                return serverName;
            }
        }
    }
}
