using Elastic.Example.Data;
using Elastic.Example.Services.Indexing;
using Elastic.Example.Services.Mappings;
using Elastic.Example.Services.Mappings.NestedTypes;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Services
{
    public class SearchService
    {
        private IElasticClient _client;
        private IDataContextFactory _dataContextFactory;

        public SearchService(IElasticClient elasticClient, IDataContextFactory dataContextFactory)
        {
            this._client = elasticClient;
            this._dataContextFactory = dataContextFactory;
        }

        public void FullRefresh()
        {
            _client.Refresh(IndexDefinition.All.Select(p => p.Name).ToArray());
        }

        public void DeleteIndexIfExists(IndexDefinition index)
        {
            if (_client.IndexExists(index.Name).Exists)
                _client.DeleteIndex(index.Name);
        }

        public void CreateIndex(IndexDefinition index, bool autoReindex = true)
        {
            DeleteIndexIfExists(index);

            ICreateIndexResponse createIndexResponse = index.Create(_client);

            if (autoReindex)
            {
                Reindex(index);
            }
        }

        public ReindexResponse Reindex(IndexDefinition index, List<Guid> ids = null)
        {
            ReindexResponse reindexResponse = new ReindexResponse();

            while (true)
            {
                using (var db = this._dataContextFactory.Create())
                {
                    if (ids == null || !ids.Any())
                    {
                        const int batchSize = 100;

                        var processed = index.PerformIndexing(_client, db, batchSize: batchSize, batchSkip: reindexResponse.TotalProcessed);
                        reindexResponse.TotalProcessed += processed;

                        if (processed < batchSize)
                        {
                            break;
                        }
                    }
                    else
                    {
                        reindexResponse.TotalProcessed += index.PerformIndexing(_client, db, ids);
                        break;
                    }
                }
            }

            return reindexResponse;
        }

        private static Type FromStringType(string esType)
        {
            var type = typeof(SearchItemDocumentBase).Assembly.GetTypes()
                .Where(t => typeof(SearchItemDocumentBase).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttributes(inherit: true).OfType<ElasticsearchTypeAttribute>()
                    .Any(a => a.Name == esType))
                .Single();

            return type;
        }

        public SearchServiceResults Search(CommonSearchRequest searchRequest)
        {
            var filteringQuery = CreateCommonFilter(searchRequest);

            const int titleBoost = 15;
            const int keywordBoost = 45;
            const int castBoost = 20;

            var results = _client.Search<SearchItemDocumentBase>(s => s
                .From(searchRequest.Skip)
                .Size(searchRequest.PageSize)
                .ConcreteTypeSelector((a, b) => FromStringType(b.Type))
                .Index(Indices.Index(IndexDefinition.All.Select(p => p.Name).ToArray()))
                .Type(Types.Type(IndexDefinition.All.Select(p => p.EsType).ToArray()))
                .Query(q => q
                    .FunctionScore(fsc => fsc
                        .BoostMode(FunctionBoostMode.Multiply)
                        .ScoreMode(FunctionScoreMode.Sum)
                        .Functions(f => f
                            .FieldValueFactor(b => b
                                .Field(nameof(SearchItemDocumentBase.Rating))
                                .Missing(0.7)
                                .Modifier(FieldValueFactorModifier.None)
                            )
                        )
                        .Query(qx => qx.MultiMatch(m => m
                            .Query(searchRequest.Query.ToLower())
                            .Fields(ff => ff
                                .Field(f => f.Title, boost: titleBoost)
                                .Field(f => f.Summary)
                                .Field(f => f.Keywords, boost: keywordBoost)
                                .Field($"{nameof(MovieSearchItem.Cast)}.{nameof(ActorNestedType.Name)}", boost: castBoost)
                            )
                            .Type(TextQueryType.BestFields)
                        ) && filteringQuery)
                    )
                )
                .Highlight(h => h
                    .Fields(ff => ff
                        .Field(f => f.Title)
                        .Field(f => f.Summary)
                        .NumberOfFragments(2)
                        .FragmentSize(250)
                        .NoMatchSize(200)
                    )
                )
            );

            var searchResult = new SearchServiceResults()
            {
                TotalResults = (int)results.Total,
                DebugInformation = results.DebugInformation,
                OriginalQuery = searchRequest.Query
            };

            foreach (var hit in results.Hits)
            {
                var relatedDocument = results.Documents.FirstOrDefault(p => p.Id == hit.Id);
                relatedDocument.PostProcess(hit.Highlights);
                searchResult.Items.Add(relatedDocument);
            }

            return searchResult;
        }

        private BoolQuery CreateCommonFilter(CommonSearchRequest searchRequest)
        {
            var filteringQuery = new BoolQuery();

            var requiredQueryParts = new List<QueryContainer>();

            if(searchRequest.ActorIds != null && searchRequest.ActorIds.Any())
            {
                requiredQueryParts.Add(GetOptionalFieldQuery(new TermsQuery()
                {
                    Field = $"{nameof(MovieSearchItem.Cast)}.{nameof(ActorNestedType.Id)}",
                    Terms = searchRequest.ActorIds.Cast<object>().ToList()
                }));
            }

            if(searchRequest.DateFrom != null || searchRequest.DateTo != null)
            {
                var dateRange = new DateRangeQuery()
                {
                    Field = nameof(MovieSearchItem.Date)
                };

                if(searchRequest.DateFrom != null)
                {
                    dateRange.GreaterThanOrEqualTo = searchRequest.DateFrom;
                }

                if (searchRequest.DateTo != null)
                {
                    dateRange.LessThanOrEqualTo = searchRequest.DateTo;
                }

                requiredQueryParts.Add(GetOptionalFieldQuery(dateRange));
            }

            filteringQuery.Filter = requiredQueryParts;

            return filteringQuery;
        }

        private BoolQuery GetOptionalFieldQuery(FieldNameQueryBase fieldNameQuery)
        {
            return new BoolQuery()
            {
                Should = new QueryContainer[]
                {
                    new BoolQuery()
                    {
                        MustNot = new QueryContainer[] {
                            new ExistsQuery()
                            {
                                Field = fieldNameQuery.Field
                            }
                        }
                    },
                    fieldNameQuery
                },
                MinimumShouldMatch = 1
            };
        }
    }
}
