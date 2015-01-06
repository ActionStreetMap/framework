using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.World.Roads;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.World
{
    [TestFixture]
    public class RoadJunctionUtilsTests
    {
        [Test]
        public void CanDetectJoinPointEnd()
        {
            // ARRANGE
            var width = 3;
            var roadPoints = new List<MapPoint>() {new MapPoint(0, 0), new MapPoint(5, 0), new MapPoint(10, 0)};

            // ACT
            var result = RoadJunctionUtils.CalculateJointPoint(roadPoints, width, false);

            // ASSERT
            Assert.AreEqual(new MapPoint(7, 0), result);
        }
    }
}
