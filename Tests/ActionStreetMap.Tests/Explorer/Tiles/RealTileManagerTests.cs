using ActionStreetMap.Core.Tiling;
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
            

            // ACT
            componentRoot.RunGame(TestHelper.BerlinTestFilePoint);
            logger.Stop();

            // ASSERT
            var tileLoader = _container.Resolve<ITilePositionObserver>() as TileManager;
            Assert.IsNotNull(tileLoader);
            Assert.AreEqual(1, tileLoader.Count);

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
            var tileLoader = _container.Resolve<ITilePositionObserver>();
            Assert.IsNotNull(tileLoader);
            Assert.IsTrue(tileLoader.GetType().FullName.Contains("ActionStreetMap.Dynamics"));
        }
    }
}
