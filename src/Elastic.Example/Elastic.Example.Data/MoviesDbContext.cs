using Elastic.Example.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Data
{
    public class MoviesDbContext : DbContext
    {
        public MoviesDbContext(DbConnection dbConnection)
            : base(dbConnection, contextOwnsConnection: false)
        {

        }

        public MoviesDbContext() 
            : base("MoviesDbContext")
        {

        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
    }
}
