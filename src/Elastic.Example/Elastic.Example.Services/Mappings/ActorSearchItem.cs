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
    [ElasticsearchType(IdProperty = nameof(SearchItemDocumentBase.Id), Name = ActorSearchItem.EsTypeName)]
    public class ActorSearchItem : SearchItemDocumentBase
    {
        public const string EsTypeName = "actorsearchitem";

        internal static ActorSearchItem Map(Actor actor)
        {
            var result = new ActorSearchItem()
            {
                Id = actor.Id.ToString(),
                Keywords = actor.FullName,
                Rating = actor.Movies.Average(p => p.Rating) * 0.1,
                Summary = actor.Bio,
                Title = actor.FullName
            };

            return result;
        }
    }
}
