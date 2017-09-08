using Elastic.Example.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Indexing.Console
{
    public class ConsoleContextFactory : IDataContextFactory
    {
        public MoviesDbContext Create()
        {
            return new MoviesDbContext();
        }
    }
}
