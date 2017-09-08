using Elastic.Example.Data;
using Nest;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Example.Services.Mappings;

namespace Elastic.Example.Services.Indexing
{
    public class MovieIndexDefinition : IndexDefinition
    {
        public override string Name => "movie";
        public override string EsType => MovieSearchItem.EsTypeName;

        public MovieIndexDefinition()
        {

        }

        public override ICreateIndexResponse Create(IElasticClient client)
        {
            return client.CreateIndex(Name, i => i
                       .Settings(CommonIndexDescriptor)
                       .Mappings(m => m
                           .Map<MovieSearchItem>(EsType, map => map.AutoMap())
                       )
                   );
        }

        internal override int PerformIndexing(IElasticClient client, MoviesDbContext db, List<Guid> ids)
        {
            var movies = db.Movies
                .AsNoTracking()
                .Include(p => p.Cast)
                .Where(p => ids.Contains(p.Id))
                .AsEnumerable()
                .Select(MovieSearchItem.Map)
                .ToList();

            return PerformDocumentIndexing(client, movies);
        }

        internal override int PerformIndexing(IElasticClient client, MoviesDbContext db, int batchSize, int batchSkip = 0)
        {
            var movieIds = db.Movies
                .OrderBy(p => p.Id)
                .Skip(batchSkip)
                .Take(batchSize)
                .Select(p => p.Id)
                .ToList();

            return PerformIndexing(client, db, movieIds);
        }
    }
}
