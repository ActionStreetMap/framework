using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Data.Storage;
using ActionStreetMap.Maps.Entities;
using NUnit.Framework;
using ActionStreetMap.Explorer.Infrastructure;

namespace ActionStreetMap.Tests.Maps.Index
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

            var kvUsage = new KeyValueUsage(new MemoryStream(1000));
            var index = new KeyValueIndex(100, 3);
            var keyValueStore = new KeyValueStore(index, kvUsage, keyValueStream);

            var elementStoreStream = new MemoryStream(new byte[10000]);
            _store = new ElementStore(keyValueStore, elementStoreStream, new ObjectPool());
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

        [Test]
        public void CanInsertAndGetRelation()
        {
            // ARRANGE
            var way1 = new Way()
            {
                Id = 1,
                Coordinates = new List<GeoCoordinate>() {new GeoCoordinate(52, 13), new GeoCoordinate(52.1f, 13.1f)},
                Tags = new Dictionary<string, string>() {{"key11", "value11"}, {"key12", "value12"}}
            };
            var offset1 = _store.Insert(way1);
            var way2 = new Way()
            {
                Id = 2,
                Coordinates = new List<GeoCoordinate>() {new GeoCoordinate(53, 14), new GeoCoordinate(53.1f, 14.1f)},
                Tags = new Dictionary<string, string>() {{"key21", "value21"}, {"key22", "value22"}}
            };
            var offset2 = _store.Insert(way2);

            var relation = new Relation()
            {
                Id = 3,
                Members = new List<RelationMember>()
                {
                    new RelationMember()
                    {
                        Role = "inner",
                        Offset = offset1
                    },
                    new RelationMember()
                    {
                        Role = "outer",
                        Offset = offset2
                    }
                },
                Tags = new Dictionary<string, string>() { { "type", "multipolygon"} }
            };

            // ACT
            var offset = _store.Insert(relation);
            var result = _store.Get(offset) as Relation;

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(relation.Id, result.Id);
            Assert.AreEqual(relation.Tags, result.Tags);
            Assert.AreEqual(2, result.Members.Count);
            AssertWays(way1, result.Members[0].Member as Way);
            AssertWays(way2, result.Members[1].Member as Way);
        }

        [Test]
        public void CanProcessSeveral()
        {
            CanInsertAndGetWay();
            CanInsertAndGetRelation();
            CanInsertAndGetNode();
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
