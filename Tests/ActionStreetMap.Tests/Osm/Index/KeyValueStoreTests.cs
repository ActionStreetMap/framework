using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionStreetMap.Osm.Index.Storage;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Osm.Index
{
    [TestFixture]
    public class KeyValueStoreTests
    {
        private KeyValueStore _store;

        [SetUp]
        public void Setup()
        {
            var buffer = new byte[256];
            var stream = new MemoryStream(buffer);
            stream.WriteByte(4);
            stream.WriteByte(7);

            var index = new KeyValueIndex(100, 3);
            _store = new KeyValueStore(index, stream);
        }

        [Test]
        public void CanInsertAndGetByOffset()
        {
            // ARRANGE
            var pair1 = new KeyValuePair<string, string>("addr", "eic");
            var pair2 = new KeyValuePair<string, string>("addr", "inv");

            // ACT
            _store.Insert(pair1);
            var offset = _store.Insert(pair2);
            var result = _store.Get(offset);

            // ASSERT
            Assert.AreEqual(pair2, result);
        }

        [Test]
        public void CanInsertAndSearch()
        {
            // ARRANGE
            var pair1 = new KeyValuePair<string, string>("addr", "eic");
            var pair2 = new KeyValuePair<string, string>("addr", "inv");

            // ACT
            _store.Insert(pair1);
            _store.Insert(pair2);
            var result = _store.Search(pair2);

            // ASSERT
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(pair2, result.First());
        }

        [Test]
        public void CanInsertTheSame()
        {
            // ARRANGE
            var pair1 = new KeyValuePair<string, string>("addr", "eic");
            var pair2 = new KeyValuePair<string, string>("addr", "eic");

            // ACT
            var offset1 = _store.Insert(pair1);
            var offset2 = _store.Insert(pair2);

            // ASSERT
            Assert.AreEqual(offset1, offset2);
        }

        [Test]
        public void CanInsertMultiplyWithCollision()
        {
            // ARRANGE
            _store.Insert(new KeyValuePair<string, string>("addr", "eic"));
            _store.Insert(new KeyValuePair<string, string>("addr", "inv"));
            _store.Insert(new KeyValuePair<string, string>("addr", "eic")); //  the same
            _store.Insert(new KeyValuePair<string, string>("addr", "eic1")); // collision
            _store.Insert(new KeyValuePair<string, string>("addr", "inv1")); // collision
            _store.Insert(new KeyValuePair<string, string>("addr", "eic2")); // collision

            // ACT
            var result1 = _store.Search(new KeyValuePair<string, string>("addr", "eic")).ToList();
            var result2 = _store.Search(new KeyValuePair<string, string>("addr", "inv")).ToList();

            // ASSERT
            Assert.AreEqual(3, result1.Count());
            Assert.AreEqual("eic", result1[0].Value);
            Assert.AreEqual("eic1", result1[1].Value);
            Assert.AreEqual("eic2", result1[2].Value);

            Assert.AreEqual(2, result2.Count());
            Assert.AreEqual("inv", result2[0].Value);
            Assert.AreEqual("inv1", result2[1].Value);
        }
    }
}
