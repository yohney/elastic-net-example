using Elastic.Example.Data;
using Elastic.Example.Data.Entity;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiMovie = System.Net.TMDb.Movie;
using ApiPerson = System.Net.TMDb.Person;

namespace Elastic.Example.TMDB.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new MoviesDbContext())
            {
                //db.Database.ExecuteSqlCommand("DELETE FROM Movies;");
                //db.Database.ExecuteSqlCommand("DELETE FROM Actors;");
            }

            using (var client = new System.Net.TMDb.ServiceClient(ConfigurationManager.AppSettings["TMDB-Api"]))
            {
                for (int i = 1, count = 1000; i <= count; i++)
                {
                    using (var db = new MoviesDbContext())
                    {
                        var movies = client.Movies.GetTopRatedAsync(null, i, CancellationToken.None).Result;
                        count = movies.PageCount; // keep track of the actual page count

                        foreach (ApiMovie m in movies.Results)
                        {
                            Thread.Sleep(250);

                            var movieApi = client.Movies.GetAsync(m.Id, null, true, CancellationToken.None).Result;

                            var movieDb = db.Movies.FirstOrDefault(p => p.ApiId == movieApi.Id);

                            if(movieDb == null)
                            {
                                movieDb = new Movie();
                                movieDb.ApiId = movieApi.Id;
                                movieDb.Cast = new List<Actor>();
                                movieDb.Title = movieApi.Title;
                                movieDb.Summary = movieApi.Overview;
                                movieDb.AirDate = movieApi.ReleaseDate;
                                movieDb.Rating = (double)movieApi.VoteAverage;

                                foreach (var x in movieApi.Credits.Cast.Take(10))
                                {
                                    Thread.Sleep(250);

                                    movieDb.Cast.Add(GetOrCreateActor(db, client, x.Id).Result);
                                }

                                db.Movies.Add(movieDb);
                                db.SaveChanges();

                                System.Console.WriteLine("[Added] " + movieDb.Title);
                            }
                            else
                            {
                                System.Console.WriteLine("[Exists] " + movieDb.Title);
                            }
                        }
                    }
                        
                }
            }
        }

        private static async Task<Actor> GetOrCreateActor(MoviesDbContext db, System.Net.TMDb.ServiceClient client, int id)
        {
            var person = await client.People.GetAsync(id, true, CancellationToken.None);

            var actor = db.Actors.FirstOrDefault(p => p.ApiId == id);

            if(actor == null)
            {
                actor = new Actor();
                actor.ApiId = person.Id;
                actor.FullName = person.Name;
                actor.Bio = person.Biography;
                db.Actors.Add(actor);
            }

            return actor;
        }
    }
}
