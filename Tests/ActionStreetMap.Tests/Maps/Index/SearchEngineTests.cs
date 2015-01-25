using System;
using System.Linq;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Maps.Index;
using ActionStreetMap.Maps.Index.Search;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Maps.Index
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
            componentRoot.RunGame(TestHelper.BerlinTestFilePoint);
            var searchEngine = _container.Resolve<ISearchEngine>();

            // ACT
            var elements = searchEngine.SearchByTag("amenity", "bar").ToArray();
        
            // ASSERT
            Assert.IsNotNull(elements);
            Assert.Greater(elements.Count(), 0);
        }
    }
}
