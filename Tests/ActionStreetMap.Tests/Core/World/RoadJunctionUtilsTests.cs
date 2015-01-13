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
        [TestCase(false)]
        [TestCase(true)]
        public void CanDetectJoinPoint(bool reversed)
        {
            // ARRANGE
            var width = 3;
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(3, 0), 
                new MapPoint(5, 0), new MapPoint(10, 0) };
            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToJoinPoint(roadPoints, width, reversed);

            // ASSERT
            Assert.AreEqual(new MapPoint(7, 0), result);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanHandleCloseJoinPoint(bool reversed)
        {
            // ARRANGE
            var width = 3;
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(5, 0), 
                new MapPoint(9, 0), new MapPoint(10, 0) };
            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToJoinPoint(roadPoints, width, reversed);

            // ASSERT
            Assert.AreEqual(new MapPoint(7, 0), result);
            Assert.AreEqual(3, roadPoints.Count);
            if (reversed)
                roadPoints.Reverse();
            Assert.AreEqual(new MapPoint(0, 0), roadPoints[0]);
            Assert.AreEqual(new MapPoint(5, 0), roadPoints[1]);
            Assert.AreEqual(new MapPoint(7, 0), roadPoints[2]);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanHandleCloseJoinPointSkipMoreThanOne(bool reversed)
        {
            // ARRANGE
            var width = 3;
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(3, 0), 
                new MapPoint(5, 0),  new MapPoint(8, 0), new MapPoint(8.5f, 0),
                new MapPoint(8.7f, 0), new MapPoint(9, 0), new MapPoint(9.5f, 0), new MapPoint(10, 0) };
            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToJoinPoint(roadPoints, width, reversed);

            // ASSERT
            Assert.AreEqual(new MapPoint(7, 0), result);
            Assert.AreEqual(4, roadPoints.Count);
            if (reversed)
                roadPoints.Reverse();
            Assert.AreEqual(new MapPoint(0, 0), roadPoints[0]);
            Assert.AreEqual(new MapPoint(3, 0), roadPoints[1]);
            Assert.AreEqual(new MapPoint(5, 0), roadPoints[2]);
            Assert.AreEqual(new MapPoint(7, 0), roadPoints[3]);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanHandleCloseTwoJoinPoint(bool reversed)
        {
            // ARRANGE
            var width = 3;
            var roadPoints = new List<MapPoint>() {new MapPoint(18, 20), new MapPoint(20, 20)};

            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToJoinPoint(roadPoints, width, reversed);

            // ASSERT
            Assert.AreEqual(new MapPoint(18, 20), result);
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

        [TestCase(true)]
        [TestCase(false)]
        public void CanGetJointSegment(bool reversed)
        {
            // ARRANGE
            var points = new List<MapPoint>() {new MapPoint(0, 0), new MapPoint(10, 0), new MapPoint(20, 0)};
            float width = 6;
            if (reversed) points.Reverse();

            // ACT
            var result = RoadJunctionUtils.GetJoinSegment(points, width, reversed);

            // ARRANGE
            Assert.AreEqual(new MapPoint(20, 3), result.Start);
            Assert.AreEqual(new MapPoint(20, -3), result.End);
        }
    }
}
