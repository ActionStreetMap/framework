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
            var tags = new TagCollection(2);
            
            // ACT
            tags.Add("key1", "value1");
            tags.Add("key2", "value2");

            // ASSERT
            Assert.IsTrue(tags.GetIndexOf("key1") > 0);
            Assert.AreEqual("value1", tags[tags.GetIndexOf("key1")]);
            Assert.IsTrue(tags.GetIndexOf("key2") > 0);
            Assert.AreEqual("value2", tags[tags.GetIndexOf("key2")]);
            Assert.IsFalse(tags.GetIndexOf("key1") > 0);
        }
    }
}
