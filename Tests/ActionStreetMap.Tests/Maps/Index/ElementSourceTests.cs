using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Maps.Data.Storage;
using ActionStreetMap.Maps.Entities;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Maps.Index
{
    [TestFixture]
    public class ElementSourceTests
    {
        private ElementSource _source;
        
        [SetUp]
        public void Setup()
        {
            var boundingBox = new BoundingBox(new GeoCoordinate(52.0, 13.0), new GeoCoordinate(52.2, 13.2));
            var keyValueStream = new MemoryStream(new byte[256]);
            keyValueStream.WriteByte(4);
            keyValueStream.WriteByte(7);

            var kvIndex = new KeyValueIndex(100, 3);
            var kvUsage = new KeyValueUsage(new MemoryStream(1000));
            var kvStore = new KeyValueStore(kvIndex, kvUsage, keyValueStream);

            var elementStoreStream = new MemoryStream(new byte[10000]);
            var elementStore = new ElementStore(kvStore, elementStoreStream, TestHelper.GetObjectPool());
            var tree = new RTree<uint>();

            var node = new Node()
            {
                Id = 1,
                Coordinate = new GeoCoordinate(52.0, 13.0),
                Tags = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } }.ToTags()
            };
            var nodeOffset = elementStore.Insert(node);
            tree.Insert(nodeOffset, new PointEnvelop(node.Coordinate));
            var way = new Way()
            {
                Id = 2,
                Coordinates = new List<GeoCoordinate>() { new GeoCoordinate(52.1, 13.1), new GeoCoordinate(52.2, 13.2) },
                Tags = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } }.ToTags()
            };
            var wayOffset = elementStore.Insert(way);
            tree.Insert(wayOffset, new Envelop(way.Coordinates.First(), way.Coordinates.Last()));

            _source = new ElementSource(boundingBox, kvUsage, kvIndex, kvStore, elementStore, tree);
        }

        [TearDown]
        public void TearDown()
        {
            _source.Dispose();
        }

        [Test]
        public void CanGetAll()
        {
            // ACT
            var results = _source.Get(new BoundingBox(new GeoCoordinate(52.0, 13.0), new GeoCoordinate(52.2, 13.2)), 
                MapConsts.MaxZoomLevel).ToArray().Wait(TimeSpan.FromSeconds(10)).ToList();

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
            var results = _source.Get(new BoundingBox(new GeoCoordinate(52.15, 13.15), new GeoCoordinate(52.2, 13.2)),
                MapConsts.MaxZoomLevel).ToArray().Wait(TimeSpan.FromSeconds(10)).ToList();

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
