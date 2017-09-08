using Elastic.Example.Services.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Services
{
    public class ReindexResponse
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }

        public ReindexResponse(bool success = true)
        {
            Success = success;
            TotalProcessed = 0;
        }

        public ReindexResponse MergeWith(ReindexResponse other)
        {
            Success &= other.Success;
            TotalProcessed += other.TotalProcessed;

            return this;
        }
    }

    public class CommonSearchRequest
    {
        public CommonSearchRequest()
        {
            this.PageSize = 20;
        }

        public int Skip { get; set; }
        public int PageSize { get; set; }

        public string Query { get; set; }

        public List<Guid> ActorIds { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class SearchServiceResults
    {
        public List<SearchItemDocumentBase> Items { get; set; }

        public int TotalResults { get; set; }
        public string OriginalQuery { get; set; }

        // Usefull while debugging
        public string DebugInformation { get; set; }

        public SearchServiceResults()
        {
            Items = new List<SearchItemDocumentBase>();
        }
    }
}
