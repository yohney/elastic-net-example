using Elastic.Example.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Tests.Infrastructure
{
    public class TestContextFactory : IDataContextFactory
    {
        private DbConnection _connection;

        public TestContextFactory(DbConnection connection)
        {
            this._connection = connection;
        }

        public MoviesDbContext Create()
        {
            return new MoviesDbContext(this._connection);
        }
    }
}
