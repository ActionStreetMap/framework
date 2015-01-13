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
        public void CanTruncateToJoinFourPoints(bool reversed)
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
            Assert.AreEqual(3, roadPoints.Count);
            Assert.AreEqual(new MapPoint(7, 0), result);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanTruncateToJoinThreePoints(bool reversed)
        {
            // ARRANGE
            var width = 3;
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(5, 0), new MapPoint(10, 0) };
            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToJoinPoint(roadPoints, width, reversed);

            // ASSERT
            Assert.AreEqual(2, roadPoints.Count);
            Assert.AreEqual(new MapPoint(7, 0), result);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanTruncateToJoinFourPointsWithClose(bool reversed)
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
        public void CanTruncateToJoinPointSkipMoreThanOne(bool reversed)
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
        public void CanTruncateToJoinPointCloseTwo(bool reversed)
        {
            // ARRANGE
            var width = 3;
            var roadPoints = new List<MapPoint>() {new MapPoint(18, 20), new MapPoint(20, 20)};

            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToJoinPoint(roadPoints, width, reversed);

            // ASSERT
            Assert.AreEqual(2, roadPoints.Count);
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
            var sorted1 = new List<MapPoint>() { left, right, top };
            var sorted2 = new List<MapPoint>() { top, right, left };
            var sorted3 = new List<MapPoint>() { right, left, top };

            // ACT
            RoadJunctionUtils.SortByAngle(bottom, pivot, sorted1);
            RoadJunctionUtils.SortByAngle(bottom, pivot, sorted2);
            RoadJunctionUtils.SortByAngle(bottom, pivot, sorted3);

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

            // ASSERT
            Assert.AreEqual(new MapPoint(20, 3), result.Start);
            Assert.AreEqual(new MapPoint(20, -3), result.End);
        }

        [Test]
        public void CanGenerateJunctionPolygon()
        {
            // ARRANGE
            var junction = new RoadJunction(new MapPoint(0, 0));
            junction.Connections.Add(new RoadElement()
            {
                Id = 0,
                Width = 3,
                End = junction,
                Points = new List<MapPoint>() { new MapPoint(-20, 0), new MapPoint(-10, 0), new MapPoint(0, 0), }
            });
            junction.Connections.Add(new RoadElement()
            {
                Id = 1,
                Width = 3,
                End = junction,
                Points = new List<MapPoint>() { new MapPoint(20, 0), new MapPoint(10, 0), new MapPoint(0, 0), }
            });
            junction.Connections.Add(new RoadElement()
            {
                Id = 3,
                Width = 3,
                Start = junction,
                Points = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(0, 10), new MapPoint(0, 20), }
            });
            junction.Connections.Add(new RoadElement()
            {
                Id = 4,
                Width = 3,
                Start = junction,
                Points = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(0, -10), new MapPoint(0, -20), }
            });

            // ACT
            RoadJunctionUtils.CompleteJunction(junction);

            // ASSERT
            Assert.AreEqual(8, junction.Polygon.Count);
        }
    }
}
