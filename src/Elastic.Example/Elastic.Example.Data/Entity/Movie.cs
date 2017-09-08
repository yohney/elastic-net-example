using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Data.Entity
{
    public class Movie
    {
        public Movie()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public int ApiId { get; set; }

        public string Title { get; set; }
        public string Summary { get; set; }

        public double Rating { get; set; }

        public DateTime? AirDate { get; set; }

        public virtual ICollection<Actor> Cast { get; set; }
    }
}
