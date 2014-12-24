using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Dependencies.Interception.Behaviors;
using ActionStreetMap.Osm.Index;
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
            // free resources: this class opens various file streams
            _container.Resolve<IElementSourceProvider>().Dispose();
        }

        [Test]
        public void CanLoadTileDynamically()
        {
            // ARRANGE
            var logger = new PerformanceLogger();
            logger.Start();
            var componentRoot = TestHelper.GetGameRunner(_container);
            componentRoot.RunGame(TestHelper.BerlinTestFilePoint);

            // ACT
            var tileLoader = _container.Resolve<IPositionListener>() as TileManager;
            tileLoader.OnMapPositionChanged(new MapPoint(0, 0));
            logger.Stop();

            // ASSERT
            Assert.IsNotNull(tileLoader);
            Assert.AreEqual(1, tileLoader.Count);

            Assert.Less(logger.Seconds, 3, "Loading took to long");
            // NOTE However, we only check memory which is used after GC
            Assert.Less(logger.Memory, 40, "Memory consumption is to hight!");
        }

        [Test]
        public void CanLoadTileWithProxy()
        {
            // ARRANGE
            _container.AllowProxy = true;
            _container.AutoGenerateProxy = true;
            _container.AddGlobalBehavior(new ExecuteBehavior());

            var componentRoot = TestHelper.GetGameRunner(_container);
            componentRoot.RunGame(TestHelper.BerlinTestFilePoint);

            // ACT
            var tileLoader = _container.Resolve<IPositionListener>();
            tileLoader.OnMapPositionChanged(new MapPoint(0, 0));

            // ASSERT
            Assert.IsNotNull(tileLoader);
            Assert.IsTrue(tileLoader.GetType().FullName.Contains("ActionStreetMap.Dynamics"));
        }
    }
}
