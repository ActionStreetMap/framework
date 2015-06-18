using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Entities;
using NUnit.Framework;
using ActionStreetMap.Explorer.Infrastructure;

namespace ActionStreetMap.Tests.Maps.Index
{
    [TestFixture]
    public class ElementSourceTests
    {
        private ElementSource _source;
        [SetUp]
        public void Setup()
        {
            var directory = "index";
            _source = new ElementSource(directory, Utils.GetFileSystemServiceMock(directory).Object,
                TestHelper.GetObjectPool());
        }

        [Test]
        public void CanGetAll()
        {
            // ACT
            var results = _source.Get(new BoundingBox(new GeoCoordinate(52.0, 13.0), new GeoCoordinate(52.2, 13.2)), MapConsts.MaxZoomLevel)
                .ToArray().Wait().ToList();

            // ASSERT
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
            Assert.IsNotNull(results[0]);
            Assert.IsNotNull(results[1]);
            Assert.AreNotSame(results[0], results[1]);
            Assert.Contains(1, results.Select(e => e.Id).ToArray());
            Assert.Contains(2, results.Select(e => e.Id).ToArray());
        }

        [Test]
        public void CanGetOne()
        {
            // ACT
            var results = _source.Get(new BoundingBox(new GeoCoordinate(52.15, 13.15), new GeoCoordinate(52.2, 13.2)), MapConsts.MaxZoomLevel)
                .ToArray().Wait().ToList();

            // ASSERT
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            var way = results[0] as Way;
            Assert.IsNotNull(way);
            Assert.AreEqual(2, way.Id);
            Assert.AreEqual(2, way.Tags.Count);
            Assert.AreEqual("value1", way.Tags["key1"]);
            Assert.AreEqual("value2", way.Tags["key2"]);
        }
    }
}
