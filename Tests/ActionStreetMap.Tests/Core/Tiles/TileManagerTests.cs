using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Infrastructure.Config;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Tiles
{
    [TestFixture]
    internal class TileManagerTests
    {
        private const float Size = 50;
        private const float Half = Size/2;
        private const float Offset = 5;
        private const float Sensitivity = 0;

        [Test]
        public void CanMoveLeft()
        {
            // ARRANGE
            var provider = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            provider.OnMapPositionChanged(center);

            // left tile
            var tile = CanLoadTile(provider, provider.Current,
                new MapPoint(-(Half - Offset - 1), 0),
                new MapPoint(-(Half - Offset), 0),
                new MapPoint(-(Half * 2 - Offset - 1), 0), 0);

            Assert.AreEqual(tile.MapCenter, new MapPoint(-Size, 0));
        }

        [Test]
        public void CanMoveRight()
        {
            // ARRANGE
            var provider = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            provider.OnMapPositionChanged(center);

            // right tile
            var tile = CanLoadTile(provider, provider.Current,
                new MapPoint(Half - Offset - 1, 0),
                new MapPoint(Half - Offset, 0),
                new MapPoint(Half * 2 - Offset - 1, 0), 0);

            Assert.AreEqual(tile.MapCenter, new MapPoint(Size, 0));
        }

        [Test]
        public void CanMoveTop()
        {
            // ARRANGE
            var provider = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            provider.OnMapPositionChanged(center);

            // top tile
            var tile = CanLoadTile(provider, provider.Current,
                new MapPoint(0, Half - Offset - 1),
                new MapPoint(0, Half - Offset),
                new MapPoint(0, Half * 2 - Offset - 1), 0);

            Assert.AreEqual(tile.MapCenter, new MapPoint(0, Size));
        }

        [Test]
        public void CanMoveBottom()
        {
            // ARRANGE
            var provider = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            provider.OnMapPositionChanged(center);

            // bottom tile
            var tile = CanLoadTile(provider, provider.Current,
                new MapPoint(0, -(Half - Offset - 1)),
                new MapPoint(0, -(Half - Offset)),
                new MapPoint(0, -(Half * 2 - Offset - 1)), 0);

            Assert.AreEqual(tile.MapCenter, new MapPoint(0, -Size));
        }

        [Test]
        public void CanMoveAround()
        {
            // ARRANGE
            var provider = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            provider.OnMapPositionChanged(center);

            var tileCenter = provider.Current;
            // left tile
            CanLoadTile(provider, tileCenter,
                new MapPoint(-(Half - Offset - 1), 0),
                new MapPoint(-(Half - Offset), 0),
                new MapPoint(-(Half*2 - Offset - 1), 0), 0);

            // right tile
            CanLoadTile(provider, tileCenter,
                new MapPoint(Half - Offset - 1, 0),
                new MapPoint(Half - Offset, 0),
                new MapPoint(Half*2 - Offset - 1, 0), 1);

            // top tile
            CanLoadTile(provider, tileCenter,
                new MapPoint(0, Half - Offset - 1),
                new MapPoint(0, Half - Offset),
                new MapPoint(0, Half*2 - Offset - 1), 2);

            // bottom tile
            CanLoadTile(provider, tileCenter,
                new MapPoint(0, -(Half - Offset - 1)),
                new MapPoint(0, -(Half - Offset)),
                new MapPoint(0, -(Half*2 - Offset - 1)), 3);
        }

        [Test]
        public void CanMoveIntoDirection()
        {
            // ARRANGE
            var provider = GetManager();
            var center = new MapPoint(0, 0);
            
            provider.OnMapPositionChanged(center);

            // ACT & ASSERT
            for (int i = 0; i < 10; i++)
            {
                provider.OnMapPositionChanged(new MapPoint(i* Size + Half - Offset, 0));
                Assert.AreEqual(i+2, provider.Count);
            }
        }

        [Test]
        public void CanMoveInTileWithoutPreload()
        {
            // ARRANGE
            var provider = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    provider.OnMapPositionChanged(new MapPoint(i, j));
                    Assert.AreEqual(1, provider.Count);
                }
            }
        }

        private TileManager GetManager()
        {
            var sceneBuilderMock = new Mock<ITileLoader>();
            var heightMapProvider = new HeightMapProvider(new Mock<IElevationProvider>().Object);
            var activatorMock = new Mock<ITileActivator>();

            var configMock = new Mock<IConfigSection>();
            configMock.Setup(c => c.GetFloat("size")).Returns(Size);
            configMock.Setup(c => c.GetFloat("offset")).Returns(Offset);
            configMock.Setup(c => c.GetFloat("sensitivity")).Returns(Sensitivity);
            configMock.Setup(c => c.GetBool("autoclean", true)).Returns(false);

            var provider = new TileManager(sceneBuilderMock.Object, heightMapProvider, 
                activatorMock.Object, new MessageBus());
            provider.Configure(configMock.Object);
            
            return provider;
        }

        private Tile CanLoadTile(TileManager manager, Tile tileCenter,
            MapPoint first, MapPoint second, MapPoint third, int tileCount)
        {
            // this shouldn't load new tile
            manager.OnMapPositionChanged(first);
            Assert.AreSame(tileCenter, manager.Current);

            ++tileCount;

            // this force to load new tile but we still in first
            manager.OnMapPositionChanged(second);
            
            Assert.AreSame(tileCenter, manager.Current);
            Assert.AreEqual(++tileCount, manager.Count);

            var previous = manager.Current;
            // this shouldn't load new tile but we're in next now
            manager.OnMapPositionChanged(third);
            Assert.AreNotSame(previous, manager.Current);
            Assert.AreEqual(tileCount, manager.Count);

            return manager.Current;
        }
    }
}