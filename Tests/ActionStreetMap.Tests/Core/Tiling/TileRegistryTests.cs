using ActionStreetMap.Core;

using ActionStreetMap.Core.Tiling.Models;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Tiling
{
    [TestFixture]
    public class TileRegistryTests
    {
        [Test]
        public void CanRegisterLocalModel()
        {
            // ARRANGE
            int testId = 7;
            int otherId = 5;
            var tile = new Tile(new GeoCoordinate(0, 0), new MapPoint(0, 0), null, 100, 100);

            // ACT
            tile.Registry.Register(testId);

            // ASSERT
            Assert.IsTrue(tile.Registry.Contains(testId));
            Assert.IsFalse(tile.Registry.Contains(otherId));
        }

        [Test]
        public void CanRegisterGlobalId()
        {
            // ARRANGE
            int testId = 7;
            int otherId = 5;
            var tile1 = new Tile(new GeoCoordinate(0, 0), new MapPoint(0, 0), null, 100, 100);
            var tile2 = new Tile(new GeoCoordinate(0, 0), new MapPoint(100, 100), null, 100, 100);

            // ACT
            tile1.Registry.RegisterGlobal(testId);
            tile1.Registry.Register(testId);

            // ASSERT
            Assert.IsTrue(tile1.Registry.Contains(testId));
            Assert.IsFalse(tile1.Registry.Contains(otherId));
            Assert.IsTrue(tile2.Registry.Contains(testId));
            Assert.IsFalse(tile2.Registry.Contains(otherId));
        }

        [Test]
        public void CanDisposeGlobalIds()
        {
            // ARRANGE
            int testId = 7;
            var tile = new Tile(new GeoCoordinate(0, 0), new MapPoint(0, 0), null, 100, 100);

            // ACT & ASSERT
            tile.Registry.RegisterGlobal(testId);
            Assert.IsTrue(tile.Registry.Contains(testId));
            tile.Registry.Dispose();
            Assert.IsFalse(tile.Registry.Contains(testId));
        }
    }
}
