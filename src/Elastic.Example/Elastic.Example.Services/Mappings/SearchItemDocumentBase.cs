using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Services.Mappings
{
    public class SearchItemDocumentBase
    {
        [Keyword(Name = nameof(Id))]
        public string Id { get; set; }

        [Text(Analyzer = "autocomplete", Name = nameof(Title))]
        public string Title { get; set; }

        [Text(Analyzer = "html_stripper", Name = nameof(Summary))]
        public string Summary { get; set; }

        [Number(Name = nameof(Rating))]
        public double Rating { get; set; }

        [Text(Analyzer = "keywords_wo_stopwords", Name = nameof(Keywords))]
        public string Keywords { get; set; }

        public SearchItemDocumentBase()
        {
            Rating = 0.7;
        }

        internal virtual void PostProcess(HighlightFieldDictionary highlights)
        {
            if (highlights?.Any(h => h.Key == nameof(Summary)) == true)
            {
                Summary = string.Join("...", highlights.First(p => p.Key == nameof(Summary)).Value.Highlights);
            }
        }
    }
}
