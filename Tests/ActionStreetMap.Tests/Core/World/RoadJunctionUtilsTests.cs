using System;
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
            Assert.AreEqual(4, roadPoints.Count);
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
            Assert.AreEqual(3, roadPoints.Count);
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


        [Test]
        public void CanTruncateToPointCorrectDistance()
        {
            // ARRANGE
            var threshold = 6;
            var joinPoint = new MapPoint(-2.0f, 0.0f);
            var points1 = new List<MapPoint> { new MapPoint(-39.2f, 112.4f), new MapPoint(-36.8f, 104.8f), new MapPoint(-17.6f, 43.6f), joinPoint };
            var points2 = new List<MapPoint> { joinPoint,new MapPoint(38.7f, -116.9f) };
            var points3 = new List<MapPoint> { new MapPoint(-186.9f, -125.1f), new MapPoint(-86.5f, -57.1f), joinPoint };
            var points4 = new List<MapPoint> { joinPoint, new MapPoint(67.9f, 24.0f), new MapPoint(93.1f, 32.6f) };

            // ACT
            var point1 = RoadJunctionUtils.TruncateToJoinPoint(points1, threshold, false);
            var point2 = RoadJunctionUtils.TruncateToJoinPoint(points2, threshold, true);
            var point3 = RoadJunctionUtils.TruncateToJoinPoint(points3, threshold, false);
            var point4 = RoadJunctionUtils.TruncateToJoinPoint(points4, threshold, true);

            // ASSERT
            Assert.IsTrue(Math.Abs(joinPoint.DistanceTo(point1) - joinPoint.DistanceTo(point2)) < 0.01);
            Assert.IsTrue(Math.Abs(joinPoint.DistanceTo(point1) - threshold) < 0.01);
            Assert.IsTrue(Math.Abs(joinPoint.DistanceTo(point3) - joinPoint.DistanceTo(point4)) < 0.01);
            Assert.IsTrue(Math.Abs(joinPoint.DistanceTo(point3) - threshold) < 0.01);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanTruncateClosePointsBug(bool reversed)
        {
            // ARRANGE
            var threshold = 6;
            var points = new List<MapPoint> { new MapPoint(-163.0f, 5913.2f), new MapPoint(-165.4f, 5912.4f), new MapPoint(-167.1f, 5911.2f) };

            if (reversed) points.Reverse();

            // ACT
            RoadJunctionUtils.TruncateToJoinPoint(points, threshold, true);

            // ASSERT
            Assert.AreEqual(2, points.Count);
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

        [Test]
        public void CanGenerateValidPolygon()
        {
            // ARRANGE
            var junction = new RoadJunction(new MapPoint(-242.0f, 2050.5f));
            junction.Connections.Add(new RoadElement() 
            { 
                Id =25421492, Width = 6, Type = RoadType.Car,
                Start = junction,
                Points = new List<MapPoint>()
                {
                    new MapPoint(-242.0f, 2050.5f),
                    new MapPoint(-241.2f, 2045.4f),
                    new MapPoint(-238.7f, 2042.2f),
                }
            });
            junction.Connections.Add(new RoadElement()
            {
                Id = 4215486, Width = 6, Type = RoadType.Car,
                Points = new List<MapPoint>()
                {
                    new MapPoint(-144.3f, 2176.6f),
                    new MapPoint(-157.4f, 2159.3f),
                    new MapPoint(-193.4f, 2113.7f),
                    new MapPoint(-242.0f, 2050.5f),
                }
            });
            junction.Connections.Add(new RoadElement()
            {
                Id = 4215486, Width = 6, Type = RoadType.Car,
                Start = junction,
                Points = new List<MapPoint>()
                {
                    new MapPoint(-242.0f, 2050.5f),
                    new MapPoint(-251.9f, 2038.6f),
                }
            });

            // ACT
            RoadJunctionUtils.CompleteJunction(junction);

            // ASSERT
            foreach (var point in junction.Polygon)
            {
                Assert.IsFalse(float.IsNaN(point.X));
                Assert.IsFalse(float.IsNaN(point.Y));
            }
        }
    }
}
