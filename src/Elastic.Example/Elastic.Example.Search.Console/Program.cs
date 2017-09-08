using Elastic.Example.Services;
using Elastic.Example.Services.Mappings;
using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Example.Search.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var node = new Uri(ConfigurationManager.AppSettings["Search-Uri"]);
            var settings = new ConnectionSettings(node);
            settings.ThrowExceptions(alwaysThrow: true);
            settings.DisableDirectStreaming();
            settings.PrettyJson();

            var client = new ElasticClient(settings);

            var service = new SearchService(client, null);

            while (true)
            {
                System.Console.WriteLine();
                System.Console.WriteLine();
                System.Console.Write("Add query or 'exit':");
                var q = System.Console.ReadLine();

                if(q == "exit")
                {
                    break;
                }

                System.Console.Clear();

                var anchor = DateTime.Now;

                var results = service.Search(new CommonSearchRequest()
                {
                    Query = q,
                    PageSize = 10
                });

                File.WriteAllText("log.txt", results.DebugInformation);

                System.Console.WriteLine();
                System.Console.WriteLine($"Results for '{q}' (total {results.TotalResults}) / {(int)((DateTime.Now - anchor).TotalMilliseconds)}ms:");

                foreach(var item in results.Items)
                {
                    Print(item);
                }
            }
        }

        static void Print(MovieSearchItem movie)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Movie:    {movie.Title} / {movie.Rating * 10}");
            System.Console.WriteLine("Cast: " + string.Join(", ", movie.Cast.Select(p => p.Name)));
            System.Console.WriteLine(movie.Summary);
        }

        static void Print(ActorSearchItem actor)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"Actor:    " + actor.Title + " / " + actor.Rating * 10);
        }

        static void Print(SearchItemDocumentBase generic)
        {
            if (generic is MovieSearchItem)
                Print((MovieSearchItem)generic);

            if (generic is ActorSearchItem)
                Print((ActorSearchItem)generic);
        }
    }
}
