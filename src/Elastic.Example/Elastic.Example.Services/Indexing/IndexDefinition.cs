using Elastic.Example.Data;
using Elastic.Example.Services.Mappings;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Services.Indexing
{
    public abstract class IndexDefinition
    {
        public static MovieIndexDefinition Movie => new MovieIndexDefinition();
        public static ActorIndexDefinition Actor => new ActorIndexDefinition();

        public static IEnumerable<IndexDefinition> All
        {
            get
            {
                yield return Movie;
                yield return Actor;
            }
        }

        public abstract string Name { get; }
        public abstract string EsType { get; }
        public abstract ICreateIndexResponse Create(IElasticClient client);
        internal abstract int PerformIndexing(IElasticClient client, MoviesDbContext db, List<Guid> ids);
        internal abstract int PerformIndexing(IElasticClient client, MoviesDbContext db, int batchSize, int batchSkip = 0);

        public static IndexDefinition FromName(string name)
        {
            return All.Single(p => p.Name == name);
        }

        public virtual int PerformDelete(IElasticClient client, params Guid[] guidsToDelete)
        {
            if (guidsToDelete.Any())
            {
                var idsToDelete = guidsToDelete.Select(s => new Id(s.ToString())).ToList();

                var deleteResponse = client.DeleteByQuery(new DeleteByQueryRequest(Name)
                {
                    Query = new IdsQuery()
                    {
                        Values = idsToDelete
                    }
                });

                return (int)deleteResponse.Deleted;
            }

            return 0;
        }

        internal int PerformIndexing(IElasticClient client, MoviesDbContext db, Guid id)
        {
            return PerformIndexing(client, db, new List<Guid>() { id });
        }

        protected virtual int PerformDocumentIndexing<TDocument>(IElasticClient client, List<TDocument> documents)
            where TDocument : SearchItemDocumentBase
        {
            if (documents.Any())
            {
                var bulkIndexResponse = client.Bulk(b => b
                       .IndexMany(documents, (op, item) => op
                           .Index(this.Name)
                       )
                    );

                if (bulkIndexResponse.Errors)
                {
                    // Handle error...
                }

                return bulkIndexResponse.Items.Count;
            }

            return 0;
        }

        protected virtual List<Guid> GetUserContentActionsIds(IEnumerable<SearchItemDocumentBase> documents)
        {
            return documents.Select(p => new Guid(p.Id)).ToList();
        }

        protected static IPromise<IIndexSettings> CommonIndexDescriptor(IndexSettingsDescriptor descriptor)
        {
            return descriptor
                .NumberOfReplicas(0)
                .NumberOfShards(1)
                .Analysis(InitCommonAnalyzers);
        }

        protected static IAnalysis InitCommonAnalyzers(AnalysisDescriptor analysis)
        {
            return analysis.Analyzers(a => a
                .Custom("html_stripper", cc => cc
                    .Filters("eng_stopwords", "trim", "lowercase")
                    .CharFilters("html_strip")
                    .Tokenizer("autocomplete")
                )
                .Custom("keywords_wo_stopwords", cc => cc
                    .Filters("eng_stopwords", "trim", "lowercase")
                    .CharFilters("html_strip")
                    .Tokenizer("key_tokenizer")
                )
                .Custom("autocomplete", cc => cc
                    .Filters("eng_stopwords", "trim", "lowercase")
                    .Tokenizer("autocomplete")
                )
            )
            .Tokenizers(tdesc => tdesc
                .Keyword("key_tokenizer", t => t)
                .EdgeNGram("autocomplete", e => e
                    .MinGram(3)
                    .MaxGram(15)
                    .TokenChars(TokenChar.Letter, TokenChar.Digit)
                )
            )
            .TokenFilters(f => f
                .Stop("eng_stopwords", lang => lang
                    .StopWords("_english_")
                )
            );
        }
    }
}
