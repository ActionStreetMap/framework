using System;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Search;
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

        [TestCase("amenity", "bar")]
        [TestCase("addr:street", null)]
        public void CanSearchTags(string key, string value)
        {
            // ARRANGE
            var componentRoot = TestHelper.GetGameRunner(_container);
            var messageBus = _container.Resolve<IMessageBus>();
            componentRoot.RunGame(TestHelper.BerlinTestFilePoint);

            // NOTE wait for tile loading ends before ask search engine
            messageBus.AsObservable<TileLoadFinishMessage>().Take(1).Wait();

            var searchEngine = _container.Resolve<ISearchEngine>();

            // ACT
            var bars = (!String.IsNullOrEmpty(value)
                ? searchEngine.SearchByTag(key, value)
                : searchEngine.SearchByText(key, BoundingBox.CreateBoundingBox(TestHelper.BerlinTestFilePoint, 100, 100), CoreConsts.MaxZoomLevel))
                .ToArray().Wait();
        
            // ASSERT
            Assert.IsNotNull(bars);
            Assert.Greater(bars.Count(), 0);
        }
    }
}
