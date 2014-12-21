using System;
using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Osm.Index.Data;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Osm.Index
{
    [TestFixture]
    public class KeyValueIndexTests
    {
        private const int Capacity = 100;
        private const int PrefixLength = 3;

        [Test]
        public void CanCreateAddGetOne()
        {
            // ARRANGE
            var index = new KeyValueIndex(Capacity, PrefixLength);
            var pair = new KeyValuePair<string, string>("addr", "eic");
            uint offset = 2;

            // ACT
            index.Add(pair, offset);
            var result = index.GetOffset(pair);

            // ASSERT
            Assert.AreEqual(offset, result);
        }

        [Test]
        public void CanCreateAddGetMultiply()
        {
            // ARRANGE
            var index = new KeyValueIndex(Capacity, PrefixLength);

            // ACT & ASSERT
            for (int i = 0; i < 3; i++)
                index.Add(new KeyValuePair<string, string>(i.ToString(), (i + 1).ToString()), (uint) i * 10);

            for (int i = 0; i < 3; i++)
            {
                var offset = index.GetOffset(new KeyValuePair<string, string>(i.ToString(), (i + 1).ToString()));
                Assert.AreEqual((uint)i * 10, offset);
            }
        }

        [Test]
        public void CanSaveAndLoad()
        {
            // ARRANGE
            var index = new KeyValueIndex(Capacity, PrefixLength);
            var buffer = new byte[Capacity * 4 + 4 + 4];
            var stream = new MemoryStream(buffer);
            // fill with some test data
            for (int i = 0; i < 3; i++)
                index.Add(new KeyValuePair<string, string>(i.ToString(), (i + 1).ToString()), (uint)i * 10);

            // ACT
            KeyValueIndex.Save(index, stream);
            var result = KeyValueIndex.Load(new MemoryStream(buffer));

            // ARRANGE
            for (int i = 0; i < 3; i++)
            {
                var offset = result.GetOffset(new KeyValuePair<string, string>(i.ToString(), (i + 1).ToString()));
                Assert.AreEqual((uint)i * 10, offset);
            }
        }
    }
}
