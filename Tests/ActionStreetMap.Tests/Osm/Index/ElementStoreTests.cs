using System;
using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Osm.Entities;
using ActionStreetMap.Osm.Index.Data;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Osm.Index
{
    [TestFixture]
    public class ElementStoreTests
    {
        private ElementStore _store;

        [SetUp]
        public void Setup()
        {
            var keyValueStream = new MemoryStream(new byte[256]);
            keyValueStream.WriteByte(4);
            keyValueStream.WriteByte(7);

            var index = new KeyValueIndex(100, 3);
            var keyValueStore = new KeyValueStore(index, keyValueStream);

            var elementStoreStream = new MemoryStream(new byte[10000]);
            _store = new ElementStore(keyValueStore, elementStoreStream);
        }

        [Test]
        public void CanInsertAndGetNode()
        {
            // ARRANGE
            var node = new Node()
            {
                Id = 1,
                Coordinate = new GeoCoordinate(52, 13),
                Tags = new Dictionary<string, string>() {{"key1", "value1"}, {"key2", "value2"}}
            };

            // ACT
            var offset = _store.Insert(node);
            var result = _store.Get(offset) as Node;

            // ASSERT
            AssertNodes(node, result);
        }

        [Test]
        public void CanInsertAndGetWay()
        {
            // ARRANGE
            var way = new Way()
            {
                Id = 1,
                Coordinates = new List<GeoCoordinate>() { new GeoCoordinate(52, 13), new GeoCoordinate(52.1f, 13.1f)},
                Tags = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } }
            };

            // ACT
            var offset = _store.Insert(way);
            var result = _store.Get(offset) as Way;

            // ASSERT
            AssertWays(way, result);
        }

        #region Helpers

        private static void AssertNodes(Node expected, Node actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            AssertCoordinate(expected.Coordinate, actual.Coordinate);
            Assert.AreEqual(expected.Tags, actual.Tags);
        }

        private static void AssertWays(Way expected, Way actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Coordinates.Count, actual.Coordinates.Count);
            for(int i= 0; i < expected.Coordinates.Count; i++)
                AssertCoordinate(expected.Coordinates[i], actual.Coordinates[i]);
            Assert.AreEqual(expected.Tags, actual.Tags);
        }

        private static void AssertCoordinate(GeoCoordinate expected, GeoCoordinate actual)
        {
            Assert.IsTrue(expected.Latitude - actual.Latitude <= 0.0000001);
            Assert.IsTrue(expected.Longitude - actual.Longitude <= 0.0000001);
        }

        #endregion
    }
}
