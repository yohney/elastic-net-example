using Elastic.Example.Data.Entity;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Services.Mappings.NestedTypes
{
    public class ActorNestedType
    {
        public string Id { get; set; }

        [Text(Analyzer = "autocomplete", Name = nameof(Name))]
        public string Name { get; set; }

        public ActorNestedType()
        {
        }

        internal static ActorNestedType Map(Actor actor)
        {
            if (actor == null)
            {
                return null;
            }

            var result = new ActorNestedType()
            {
                Name = actor.FullName,
                Id = actor.Id.ToString()
            };

            return result;
        }
    }
}
