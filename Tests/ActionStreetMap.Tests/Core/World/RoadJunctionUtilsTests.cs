using System.Linq;
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

        [Test]
        public void CanHandleCloseJoinPointEnd()
        {
            // ARRANGE
            var width = 3;
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(8, 0), new MapPoint(10, 0) };

            // ACT
            var result = RoadJunctionUtils.CalculateJointPoint(roadPoints, width, false);

            // ASSERT
            Assert.AreEqual(new MapPoint(8, 0), result);
        }

        [TestCase(-10, 0, 90)]
        [TestCase(-5, -5, 45)]
        [TestCase(-5, 5, 135)]
        [TestCase(0, 10, 180)]
        [TestCase(5, 5, 225)]
        [TestCase(10, 0, 270)]
        [TestCase(5, -5, 315)]
        public void CanGetAngle(float x, float y, float expectedAngle)
        {
            // ARRANGE
            var pivot = new MapPoint(0, 0);
            var bottom = new MapPoint(0, -10);

            // ACT
            var angle = RoadJunctionUtils.GetTurnAngle(bottom, pivot, new MapPoint(x, y));

            // ASSERT
            Assert.AreEqual(expectedAngle, angle);
        }

        [Test]
        public void CanSortByTurnAngle()
        {
            // ARRANGE
            var pivot = new MapPoint(0, 0);
            var bottom = new MapPoint(0, -10);
            var left = new MapPoint(-10, 0);
            var right = new MapPoint(10, 0);
            var top = new MapPoint(0, 10);

            // ACT
            var sorted1 = RoadJunctionUtils.SortByAngle(bottom, pivot, new List<MapPoint>() { left, right, top }).ToList();
            var sorted2 = RoadJunctionUtils.SortByAngle(bottom, pivot, new List<MapPoint>() { top, right, left }).ToList();
            var sorted3 = RoadJunctionUtils.SortByAngle(bottom, pivot, new List<MapPoint>() { right, left, top }).ToList();

            // ASSERT
            CollectionAssert.AreEqual(new List<MapPoint>() {left, top, right}, sorted2);
            CollectionAssert.AreEqual(sorted1, sorted2);
            CollectionAssert.AreEqual(sorted1, sorted3);
        }
    }
}
