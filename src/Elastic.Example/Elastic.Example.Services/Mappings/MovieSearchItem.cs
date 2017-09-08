using Elastic.Example.Data.Entity;
using Elastic.Example.Services.Mappings.NestedTypes;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Services.Mappings
{
    [ElasticsearchType(IdProperty = nameof(SearchItemDocumentBase.Id), Name = MovieSearchItem.EsTypeName)]
    public class MovieSearchItem : SearchItemDocumentBase
    {
        public const string EsTypeName = "moviesearchitem";

        [Date(Name = nameof(Date), Index = true)]
        public DateTime? Date { get; set; }

        [Nested(Name = nameof(Cast), IncludeInRoot = true)]
        public List<ActorNestedType> Cast { get; set; }

        internal static MovieSearchItem Map(Movie movie)
        {
            var result = new MovieSearchItem()
            {
                Id = movie.Id.ToString(),
                Date = movie.AirDate,
                Keywords = movie.Title + " " + string.Join(" ", movie.Cast.Take(3).Select(p => p.FullName)),
                Rating = movie.Rating * 0.1,
                Cast = movie.Cast.Select(ActorNestedType.Map).ToList(),
                Title = movie.Title,
                Summary = movie.Summary
            };

            return result;
        }
    }
}
