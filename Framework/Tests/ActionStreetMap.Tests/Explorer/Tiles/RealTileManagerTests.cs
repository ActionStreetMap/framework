﻿using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Dependencies.Interception.Behaviors;
using NUnit.Framework;

namespace ActionStreetMap.Maps.UnitTests.Explorer.Tiles
{
    [TestFixture]
    public class RealTileManagerTests
    {
        [Test]
        public void CanLoadTileDynamically()
        {
            // ARRANGE
            var logger = new PerformanceLogger();
            logger.Start();
            var container = new Container();
            var componentRoot = TestHelper.GetGameRunner(container);
            componentRoot.RunGame(new GeoCoordinate(52.5280173, 13.3739963));

            // ACT
            var tileLoader = container.Resolve<IPositionListener>() as TileManager;
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
            var container = new Container();

            container.AllowProxy = true;
            container.AutoGenerateProxy = true;
            container.AddGlobalBehavior(new ExecuteBehavior());

            var componentRoot = TestHelper.GetGameRunner(container);
            componentRoot.RunGame(TestHelper.BerlinInvalidenStr);

            // ACT
            var tileLoader = container.Resolve<IPositionListener>();
            tileLoader.OnMapPositionChanged(new MapPoint(0, 0));

            // ASSERT
            Assert.IsNotNull(tileLoader);
            Assert.IsTrue(tileLoader.GetType().FullName.Contains("ActionStreetMap.Dynamics"));
        }
    }
}
