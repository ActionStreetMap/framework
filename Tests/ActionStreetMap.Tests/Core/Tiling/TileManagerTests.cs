using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Primitives;
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
            var center = new Vector2d(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<Vector2d>).OnNext(center);

            // left tile
            var tile = CanLoadTile(observer, observer.CurrentTile,
                new Vector2d(-(Half - Offset - 1), 0),
                new Vector2d(-(Half - Offset), 0),
                new Vector2d(-(Half * 2 - Offset - 1), 0), 0);

            Assert.AreEqual(tile.MapCenter, new Vector2d(-Size, 0));
        }

        [Test]
        public void CanMoveRight()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new Vector2d(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<Vector2d>).OnNext(center);

            // right tile
            var tile = CanLoadTile(observer, observer.CurrentTile,
                new Vector2d(Half - Offset - 1, 0),
                new Vector2d(Half - Offset, 0),
                new Vector2d(Half * 2 - Offset - 1, 0), 0);

            Assert.AreEqual(tile.MapCenter, new Vector2d(Size, 0));
        }

        [Test]
        public void CanMoveTop()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new Vector2d(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<Vector2d>).OnNext(center);

            // top tile
            var tile = CanLoadTile(observer, observer.CurrentTile,
                new Vector2d(0, Half - Offset - 1),
                new Vector2d(0, Half - Offset),
                new Vector2d(0, Half * 2 - Offset - 1), 0);

            Assert.AreEqual(tile.MapCenter, new Vector2d(0, Size));
        }

        [Test]
        public void CanMoveBottom()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new Vector2d(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<Vector2d>).OnNext(center);

            // bottom tile
            var tile = CanLoadTile(observer, observer.CurrentTile,
                new Vector2d(0, -(Half - Offset - 1)),
                new Vector2d(0, -(Half - Offset)),
                new Vector2d(0, -(Half * 2 - Offset - 1)), 0);

            Assert.AreEqual(tile.MapCenter, new Vector2d(0, -Size));
        }

        [Test]
        public void CanMoveAround()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new Vector2d(0, 0);

            // ACT & ASSERT
            (observer as IPositionObserver<Vector2d>).OnNext(center);

            var tileCenter = observer.CurrentTile;
            // left tile
            CanLoadTile(observer, tileCenter,
                new Vector2d(-(Half - Offset - 1), 0),
                new Vector2d(-(Half - Offset), 0),
                new Vector2d(-(Half*2 - Offset - 1), 0), 0);

            // right tile
            CanLoadTile(observer, tileCenter,
                new Vector2d(Half - Offset - 1, 0),
                new Vector2d(Half - Offset, 0),
                new Vector2d(Half*2 - Offset - 1, 0), 1);

            // top tile
            CanLoadTile(observer, tileCenter,
                new Vector2d(0, Half - Offset - 1),
                new Vector2d(0, Half - Offset),
                new Vector2d(0, Half*2 - Offset - 1), 2);

            // bottom tile
            CanLoadTile(observer, tileCenter,
                new Vector2d(0, -(Half - Offset - 1)),
                new Vector2d(0, -(Half - Offset)),
                new Vector2d(0, -(Half*2 - Offset - 1)), 3);
        }

        [Test]
        public void CanMoveIntoDirection()
        {
            // ARRANGE
            var observer = GetManager();
            var center = new Vector2d(0, 0);

            (observer as IPositionObserver<Vector2d>).OnNext(center);

            // ACT & ASSERT
            for (int i = 0; i < 10; i++)
            {
                (observer as IPositionObserver<Vector2d>).OnNext(new Vector2d(i * Size + Half - Offset, 0));
                Assert.LessOrEqual(GetSceneTileCount(observer), 3);
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
                    (observer as IPositionObserver<Vector2d>).OnNext(new Vector2d(i, j));
                    Assert.AreEqual(1, GetSceneTileCount(observer));
                }
            }
        }

        [Test]
        public void CanSwitchFromSceneToOverviewMode()
        {
            // ARRANGE
            var observer = GetManager();

            int expectedSceneTileCount = 1;
            int expectedOverviewTileCount = 8;

            // ACT & ASSERT
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {

                    if (i == 5)
                    {
                        observer.Mode = RenderMode.Overview;
                        observer.Viewport = new Rectangle2d(0, 0, Size*5, Size*3f);

                        expectedOverviewTileCount = 14;
                    }

                    // ACT
                    (observer as IPositionObserver<Vector2d>).OnNext(new Vector2d(i, j));

                    // ASSERT
                    Assert.AreEqual(expectedSceneTileCount, GetSceneTileCount(observer));
                    Assert.AreEqual(expectedOverviewTileCount, GetOverviewTileCount(observer));
                }
            }
        }

        private TileController GetManager()
        {
            var sceneBuilderMock = new Mock<ITileLoader>();
            sceneBuilderMock.Setup(l => l.Load(It.IsAny<Tile>())).Returns(Observable.Empty<Unit>());
         
            var activatorMock = new Mock<ITileActivator>();

            var configMock = new Mock<IConfigSection>();
            configMock.Setup(c => c.GetFloat("size", It.IsAny<float>())).Returns(Size);
            configMock.Setup(c => c.GetFloat("offset", It.IsAny<float>())).Returns(Offset);
            configMock.Setup(c => c.GetFloat("sensitivity", It.IsAny<float>())).Returns(Sensitivity);
            configMock.Setup(c => c.GetBool("autoclean", true)).Returns(false);
            configMock.Setup(c => c.GetString("render_mode", It.IsAny<string>())).Returns("scene");

            var observer = new TileController(sceneBuilderMock.Object,
                activatorMock.Object, new MessageBus(), TestHelper.GetObjectPool());
            observer.Configure(configMock.Object);
            
            return observer;
        }

        private Tile CanLoadTile(TileController controller, Tile tileCenter,
            Vector2d first, Vector2d second, Vector2d third, int tileCount)
        {
            var observer = controller as IPositionObserver<Vector2d>;

            // this shouldn't load new tile
            observer.OnNext(first);
            Assert.AreSame(tileCenter, controller.CurrentTile);

            ++tileCount;

            // this force to load new tile but we still in first
            observer.OnNext(second);

            Assert.AreSame(tileCenter, controller.CurrentTile);
            Assert.AreEqual(++tileCount, GetSceneTileCount(controller));

            var previous = controller.CurrentTile;
            // this shouldn't load new tile but we're in next now
            observer.OnNext(third);
            Assert.AreNotSame(previous, controller.CurrentTile);
            Assert.AreEqual(tileCount, GetSceneTileCount(controller));

            return controller.CurrentTile;
        }

        private int GetSceneTileCount(TileController controller)
        {
            return ReflectionUtils.GetFieldValue<DoubleKeyDictionary<int, int, Tile>>(controller, "_allSceneTiles").Count();
        }

        private int GetOverviewTileCount(TileController controller)
        {
            return ReflectionUtils.GetFieldValue<DoubleKeyDictionary<int, int, Tile>>(controller, "_allOverviewTiles").Count();
        }
    }
}