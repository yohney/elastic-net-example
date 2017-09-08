using Elastic.Example.Services;
using Elastic.Example.Services.Indexing;
using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Indexing.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var node = new Uri(ConfigurationManager.AppSettings["Search-Uri"]);
            var settings = new ConnectionSettings(node);
            settings.ThrowExceptions(alwaysThrow: true);
            settings.DisableDirectStreaming();

            var client = new ElasticClient(settings);

            System.Console.WriteLine("Starting indexing...");

            var service = new SearchService(client, new ConsoleContextFactory());
            service.CreateIndex(IndexDefinition.Actor, autoReindex: true);
            System.Console.WriteLine("Actor [done]");
            service.CreateIndex(IndexDefinition.Movie, autoReindex: true);
            System.Console.WriteLine("Movie [done]");
            service.FullRefresh();

            System.Console.WriteLine("Completed!");
        }
    }
}
