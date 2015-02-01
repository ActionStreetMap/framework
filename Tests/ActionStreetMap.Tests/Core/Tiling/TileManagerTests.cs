using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Reactive;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Tiling
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
            var observer = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<MapPoint>).OnNext(center);

            // left tile
            var tile = CanLoadTile(observer, observer.Current,
                new MapPoint(-(Half - Offset - 1), 0),
                new MapPoint(-(Half - Offset), 0),
                new MapPoint(-(Half * 2 - Offset - 1), 0), 0);

            Assert.AreEqual(tile.MapCenter, new MapPoint(-Size, 0));
        }

        [Test]
        public void CanMoveRight()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<MapPoint>).OnNext(center);

            // right tile
            var tile = CanLoadTile(observer, observer.Current,
                new MapPoint(Half - Offset - 1, 0),
                new MapPoint(Half - Offset, 0),
                new MapPoint(Half * 2 - Offset - 1, 0), 0);

            Assert.AreEqual(tile.MapCenter, new MapPoint(Size, 0));
        }

        [Test]
        public void CanMoveTop()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<MapPoint>).OnNext(center);

            // top tile
            var tile = CanLoadTile(observer, observer.Current,
                new MapPoint(0, Half - Offset - 1),
                new MapPoint(0, Half - Offset),
                new MapPoint(0, Half * 2 - Offset - 1), 0);

            Assert.AreEqual(tile.MapCenter, new MapPoint(0, Size));
        }

        [Test]
        public void CanMoveBottom()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<MapPoint>).OnNext(center);

            // bottom tile
            var tile = CanLoadTile(observer, observer.Current,
                new MapPoint(0, -(Half - Offset - 1)),
                new MapPoint(0, -(Half - Offset)),
                new MapPoint(0, -(Half * 2 - Offset - 1)), 0);

            Assert.AreEqual(tile.MapCenter, new MapPoint(0, -Size));
        }

        [Test]
        public void CanMoveAround()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new MapPoint(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<MapPoint>).OnNext(center);

            var tileCenter = observer.Current;
            // left tile
            CanLoadTile(observer, tileCenter,
                new MapPoint(-(Half - Offset - 1), 0),
                new MapPoint(-(Half - Offset), 0),
                new MapPoint(-(Half*2 - Offset - 1), 0), 0);

            // right tile
            CanLoadTile(observer, tileCenter,
                new MapPoint(Half - Offset - 1, 0),
                new MapPoint(Half - Offset, 0),
                new MapPoint(Half*2 - Offset - 1, 0), 1);

            // top tile
            CanLoadTile(observer, tileCenter,
                new MapPoint(0, Half - Offset - 1),
                new MapPoint(0, Half - Offset),
                new MapPoint(0, Half*2 - Offset - 1), 2);

            // bottom tile
            CanLoadTile(observer, tileCenter,
                new MapPoint(0, -(Half - Offset - 1)),
                new MapPoint(0, -(Half - Offset)),
                new MapPoint(0, -(Half*2 - Offset - 1)), 3);
        }

        [Test]
        public void CanMoveIntoDirection()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new MapPoint(0, 0);

            (observer as IPositionObserver<MapPoint>).OnNext(center);

            // ACT & ASSERT
            for (int i = 0; i < 10; i++)
            {
                (observer as IPositionObserver<MapPoint>).OnNext(new MapPoint(i * Size + Half - Offset, 0));
                Assert.AreEqual(i+2, observer.Count);
            }
        }

        [Test]
        public void CanMoveInTileWithoutPreload()
        {
            // ARRANGE
            var observer = GetManager();

            // ACT & ASSERT
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    (observer as IPositionObserver<MapPoint>).OnNext(new MapPoint(i, j));
                    Assert.AreEqual(1, observer.Count);
                }
            }
        }

        private TileManager GetManager()
        {
            var sceneBuilderMock = new Mock<ITileLoader>();
            sceneBuilderMock.Setup(l => l.Load(It.IsAny<Tile>())).Returns(Observable.Empty<Unit>());
            var heightMapobserver = new HeightMapProvider(new Mock<IElevationProvider>().Object, new ObjectPool());
            heightMapobserver.Trace = new ConsoleTrace();
            var activatorMock = new Mock<ITileActivator>();

            var configMock = new Mock<IConfigSection>();
            configMock.Setup(c => c.GetFloat("size", It.IsAny<float>())).Returns(Size);
            configMock.Setup(c => c.GetFloat("offset", It.IsAny<float>())).Returns(Offset);
            configMock.Setup(c => c.GetFloat("sensitivity", It.IsAny<float>())).Returns(Sensitivity);
            configMock.Setup(c => c.GetBool("autoclean", true)).Returns(false);

            var observer = new TileManager(sceneBuilderMock.Object, heightMapobserver, 
                activatorMock.Object, new MessageBus(), new ObjectPool());
            observer.Configure(configMock.Object);
            
            return observer;
        }

        private Tile CanLoadTile(TileManager manager, Tile tileCenter,
            MapPoint first, MapPoint second, MapPoint third, int tileCount)
        {
            var observer = manager as IPositionObserver<MapPoint>;

            // this shouldn't load new tile
            observer.OnNext(first);
            Assert.AreSame(tileCenter, manager.Current);

            ++tileCount;

            // this force to load new tile but we still in first
            observer.OnNext(second);
            
            Assert.AreSame(tileCenter, manager.Current);
            Assert.AreEqual(++tileCount, manager.Count);

            var previous = manager.Current;
            // this shouldn't load new tile but we're in next now
            observer.OnNext(third);
            Assert.AreNotSame(previous, manager.Current);
            Assert.AreEqual(tileCount, manager.Count);

            return manager.Current;
        }
    }
}