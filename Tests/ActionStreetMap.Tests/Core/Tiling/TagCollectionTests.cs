using ActionStreetMap.Core.Tiling.Models;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Tiling
{
    [TestFixture]
    public class TagCollectionTests
    {
        [Test]
        public void CanAddAndCheckContainsTags()
        {
            // ARRANGE
            var tags = new TagCollection(3);
            
            // ACT
            tags.Add("key1", "value1");
            tags.Add("key2", "value2");
            tags.Add("key0", "value0");

            tags = tags.AsReadOnly();

            // ASSERT
            Assert.IsTrue(tags.IndexOf("key0") >= 0);
            Assert.AreEqual("value0", tags[tags.IndexOf("key0")].Value);
            Assert.IsTrue(tags.IndexOf("key1") >= 0);
            Assert.AreEqual("value1", tags[tags.IndexOf("key1")].Value);
            Assert.IsTrue(tags.IndexOf("key2") >= 0);
            Assert.AreEqual("value2", tags[tags.IndexOf("key2")].Value);
            Assert.IsFalse(tags.IndexOf("key3") >= 0);
        }

        [Test]
        public void CanMergeTags()
        {
            // ARRANGE
            var tags1 = new TagCollection(3);
            tags1.Add("key1", "value1");
            tags1.Add("key2", "value2");
            tags1.Add("key0", "value0");

            var tags2 = new TagCollection(3);
            tags2.Add("key3", "value3");
            tags2.Add("key2", "value2");
            tags2.Add("key4", "value4");

            tags1 = tags1.AsReadOnly();
            tags2 = tags2.AsReadOnly();

            // ACT
            tags1.Merge(tags2);

            // ASSERT
            Assert.AreEqual(5, tags1.Count);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual("key"+ i, tags1.KeyAt(i));
                Assert.AreEqual("value" + i, tags1.ValueAt(i));
            }
        }
    }
}
