using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Tests.Infrastructure.Stubs;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Infrastructure
{
    [TestFixture]
    public class PrimitiveTypeTests
    {
        [Test]
        public void CanUseTuple()
        {
            // ARRANGE & ACT
            var tuple1 = new MutableTuple<string, int>("key", 7);
            var tuple2 = new MutableTuple<string, int>("key", 7);
            
            // ASSERT
            Assert.AreEqual("key", tuple1.Item1);
            Assert.AreEqual(7, tuple1.Item2);
            Assert.AreEqual(tuple1, tuple2);
        }
    }
}