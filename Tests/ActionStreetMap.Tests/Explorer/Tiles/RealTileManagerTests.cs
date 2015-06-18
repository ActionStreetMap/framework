using System.Linq;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Dependencies.Interception.Behaviors;
using ActionStreetMap.Infrastructure.Primitives;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Tiles
{
    [TestFixture]
    public class RealTileManagerTests
    {
        private Container _container;

        [SetUp]
        public void SetUp()
        {
            _container = new Container();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void CanLoadTileDynamically()
        {
            // ARRANGE
            var logger = new PerformanceLogger();
            logger.Start();
            var componentRoot = TestHelper.GetGameRunner(_container);
            

            // ACT
            componentRoot.RunGame(TestHelper.BerlinTestFilePoint);
            logger.Stop();

            // ASSERT
            var tileLoader = _container.Resolve<ITileController>() as TileController;
            Assert.IsNotNull(tileLoader);
            Assert.AreEqual(1, GetSceneTileCount(tileLoader));

            Assert.Less(logger.Seconds, 3, "Loading took too long");
            // NOTE Actual value should be close to expected consumption for test data
            Assert.Less(logger.Memory, 100, "Memory consumption is too high!");
        }

        [Test]
        [Ignore]
        public void CanLoadTileWithProxy()
        {
            // ARRANGE
            _container.AllowProxy = true;
            _container.AutoGenerateProxy = true;
            _container.AddGlobalBehavior(new ExecuteBehavior());
            var componentRoot = TestHelper.GetGameRunner(_container);

            // ACT
            componentRoot.RunGame(TestHelper.BerlinTestFilePoint);

            // ASSERT
            var tileLoader = _container.Resolve<ITileController>();
            Assert.IsNotNull(tileLoader);
            Assert.IsTrue(tileLoader.GetType().FullName.Contains("ActionStreetMap.Dynamics"));
        }

        private int GetSceneTileCount(TileController controller)
        {
            return ReflectionUtils.GetFieldValue<DoubleKeyDictionary<int, int, Tile>>(controller, "_allSceneTiles").Count();
        }
    }
}
