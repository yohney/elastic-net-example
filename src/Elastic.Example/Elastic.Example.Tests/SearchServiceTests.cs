using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using Elastic.Example.Tests.Infrastructure;
using Elastic.Example.Services;
using System.Configuration;
using Nest;
using System.Linq;
using Elastic.Example.Services.Indexing;
using Elastic.Example.Services.Mappings;

namespace Elastic.Example.Tests
{
    [TestClass]
    public class SearchServiceTests
    {
        private DbConnection _connection = Effort.DbConnectionFactory.CreateTransient();
        private TestContextFactory _contextFactory;
        private SearchService _service;

        [TestInitialize]
        public void TestInit()
        {
            this._contextFactory = new TestContextFactory(_connection);

            var node = new Uri(ConfigurationManager.AppSettings["Search-Uri"]);
            var settings = new ConnectionSettings(node);
            settings.ThrowExceptions(alwaysThrow: true);
            settings.PrettyJson(true);
            settings.DisableDirectStreaming();

            var client = new ElasticClient(settings);

            this._service = new SearchService(client, _contextFactory);
        }

        [TestMethod]
        public void Indexing_MoviesAndActors_ShouldSucceed()
        {
            AddActor("Christopher Walken");
            AddActor("Denzel Washington");
            AddActor("Ethan Hawke");

            AddMovie(
                name: "Man on Fire",
                summary: "In Mexico City, a former assassin swears vengeance on those who committed an unspeakable act against the family he was hired to protect.",
                actors: "Denzel Washington,Christopher Walken",
                airDate: new DateTime(2004, 4, 23),
                rating: 7.72);

            AddMovie(
                name: "Training Day",
                summary: "On his first day on the job as a Los Angeles narcotics officer, a rookie cop goes beyond a full work day in training within the narcotics division of the LAPD with a rogue detective who isn't what he appears to be.",
                actors: "Denzel Washington,Ethan Hawke",
                airDate: new DateTime(2001, 10, 5),
                rating: 7.67);

            _service.CreateIndex(IndexDefinition.Actor, autoReindex: true);
            _service.CreateIndex(IndexDefinition.Movie, autoReindex: true);
            _service.FullRefresh();

            var result = this.Search("Fire");
            Assert.AreEqual(2, result.Items.Count, result.DebugInformation);
            Assert.AreEqual(2, result.Items.OfType<MovieSearchItem>().Count(), result.DebugInformation);

            result = this.Search("Denzel");
            Assert.AreEqual(3, result.Items.Count, result.DebugInformation);
            Assert.AreEqual(1, result.Items.OfType<ActorSearchItem>().Count(), result.DebugInformation);
        }

        private SearchServiceResults Search(string query)
        {
            return _service.Search(new CommonSearchRequest()
            {
                Query = query
            });
        }

        private void AddMovie(string name, string summary = null, string actors = null, double rating = 5, DateTime? airDate = null)
        {
            using (var db = _contextFactory.Create())
            {
                var actorsDb = actors.Split(',').Select(a => db.Actors.Single(s => s.FullName == a));

                db.Movies.Add(new Data.Entity.Movie()
                {
                    Cast = actorsDb.ToList(),
                    AirDate = airDate,
                    Rating = rating,
                    Summary = summary,
                    Title = name
                });

                db.SaveChanges();
            }
        }

        private void AddActor(string name, string bio = null)
        {
            using (var db = _contextFactory.Create())
            {
                db.Actors.Add(new Data.Entity.Actor()
                {
                    FullName = name,
                    Bio = bio
                });

                db.SaveChanges();
            }
        }
    }
}
