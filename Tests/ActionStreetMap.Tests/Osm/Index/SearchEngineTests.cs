using System;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Osm.Index;
using ActionStreetMap.Osm.Index.Search;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Osm.Index
{
    [TestFixture]
    public class SearchEngineTests
    {
        private Container _container;

        [SetUp]
        public void SetUp()
        {
            _container = new Container();
        }

        [TearDown]
        public void TearDown()
        {
            // free resources: this class opens various file streams
            _container.Resolve<IElementSourceProvider>().Dispose();
        }

        [Test]
        public void CanSearchTags()
        {
            // ARRANGE
            var componentRoot = TestHelper.GetGameRunner(_container);
            componentRoot.RunGame(new GeoCoordinate(55.75372, 37.61990));
            var searchEngine = _container.Resolve<SearchEngine>();

            // ACT
            var elements = searchEngine.SearchByTag("amenity", "bar").ToArray();
        
            // ASSERT
            Assert.IsNotNull(elements);
            Assert.Greater(elements.Count(), 0);
        }
    }
}
