using System;
using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Explorer.Infrastructure;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Scene
{
    [TestFixture]
    public class RoadJunctionUtilsTests
    {
       
        [TestCase(false)]
        [TestCase(true)]
        public void CanTruncateToJoinFourPoints(bool reversed)
        {
            // ARRANGE
            var truncPoint = new MapPoint(7, 0);
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(3, 0), 
                new MapPoint(5, 0), new MapPoint(10, 0) };
            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToPoint(truncPoint, roadPoints, reversed, new ObjectPool());

            // ASSERT
            Assert.AreEqual(4, roadPoints.Count);
            Assert.AreEqual(truncPoint, result);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanTruncateToJoinThreePoints(bool reversed)
        {
            // ARRANGE
            var truncPoint = new MapPoint(7, 0);
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(5, 0), new MapPoint(10, 0) };
            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToPoint(truncPoint, roadPoints, reversed, new ObjectPool());

            // ASSERT
            Assert.AreEqual(3, roadPoints.Count);
            Assert.AreEqual(truncPoint, result);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanTruncateToJoinFourPointsWithClose(bool reversed)
        {
            // ARRANGE
            var truncPoint = new MapPoint(7, 0);
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(5, 0), 
                new MapPoint(9, 0), new MapPoint(10, 0) };
            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToPoint(truncPoint, roadPoints, reversed, new ObjectPool());

            // ASSERT
            Assert.AreEqual(truncPoint, result);
            Assert.AreEqual(3, roadPoints.Count);
            if (reversed)
                roadPoints.Reverse();
            Assert.AreEqual(new MapPoint(0, 0), roadPoints[0]);
            Assert.AreEqual(new MapPoint(5, 0), roadPoints[1]);
            Assert.AreEqual(truncPoint, roadPoints[2]);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanTruncateToJoinPointSkipMoreThanOne(bool reversed)
        {
            // ARRANGE
            var truncPoint = new MapPoint(7, 0);
            var roadPoints = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(3, 0), 
                new MapPoint(5, 0),  new MapPoint(8, 0), new MapPoint(8.5f, 0),
                new MapPoint(8.7f, 0), new MapPoint(9, 0), new MapPoint(9.5f, 0), new MapPoint(10, 0) };
            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToPoint(truncPoint, roadPoints, reversed, new ObjectPool());

            // ASSERT
            Assert.AreEqual(truncPoint, result);
            Assert.AreEqual(4, roadPoints.Count);
            if (reversed)
                roadPoints.Reverse();
            Assert.AreEqual(new MapPoint(0, 0), roadPoints[0]);
            Assert.AreEqual(new MapPoint(3, 0), roadPoints[1]);
            Assert.AreEqual(new MapPoint(5, 0), roadPoints[2]);
            Assert.AreEqual(truncPoint, roadPoints[3]);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CanTruncateToJoinPointCloseTwo(bool reversed)
        {
            // ARRANGE
            var roadPoints = new List<MapPoint>() {new MapPoint(18, 20), new MapPoint(20, 20)};

            if (reversed)
                roadPoints.Reverse();

            // ACT
            var result = RoadJunctionUtils.TruncateToPoint(new MapPoint(17,20), roadPoints, reversed, new ObjectPool());

            // ASSERT
            Assert.AreEqual(2, roadPoints.Count);
            Assert.AreEqual(new MapPoint(19, 20), result);
        }

        [Test]
        public void DoNotTruncateToTheEqualPointsIfTwoClosePoints()
        {
            // ARRANGE
            var roadPoints = new List<MapPoint>() { new MapPoint(-437.152069f, -34.97366f), new MapPoint(-437.646942f, -36.696228f) };

            // ACT
            RoadJunctionUtils.TruncateToPoint(new MapPoint(-438.1275f, -36.55817f), roadPoints, true, new ObjectPool());

            // ASSERT
            Assert.AreEqual(2, roadPoints.Count);
            Assert.AreNotEqual(roadPoints[0], roadPoints[1]);
        }

        [Test]
        public void CanGenerateJunctionPolygon()
        {
            // ARRANGE
            var junction = new RoadJunction(new MapPoint(0, 0));
            junction.Connections.Add(new RoadElement()
            {
                Id = 0, Width = 3, End = junction,
                Points = new List<MapPoint>() { new MapPoint(-20, 0), new MapPoint(-10, 0), new MapPoint(0, 0), }
            });
            junction.Connections.Add(new RoadElement()
            {
                Id = 1, Width = 3, End = junction,
                Points = new List<MapPoint>() { new MapPoint(20, 0), new MapPoint(10, 0), new MapPoint(0, 0), }
            });
            junction.Connections.Add(new RoadElement()
            {
                Id = 3, Width = 3, Start = junction,
                Points = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(0, 10), new MapPoint(0, 20), }
            });
            junction.Connections.Add(new RoadElement()
            {
                Id = 4, Width = 3, Start = junction,
                Points = new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(0, -10), new MapPoint(0, -20), }
            });

            // ACT
            RoadJunctionUtils.Complete(junction, new ObjectPool());

            // ASSERT
            using (var writer = new StreamWriter(new FileStream("mappoint.txt", FileMode.Create)))
            {
                foreach (var mapPoint in junction.Polygon)
                {
                    writer.WriteLine("new MapPoint({0}f, {1}f),", mapPoint.X, mapPoint.Y);
                }
            }
            Assert.AreEqual(28, junction.Polygon.Count);
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
            RoadJunctionUtils.Complete(junction, new ObjectPool());

            // ASSERT
            foreach (var point in junction.Polygon)
            {
                Assert.IsFalse(float.IsNaN(point.X));
                Assert.IsFalse(float.IsNaN(point.Y));
            }
        }
    }
}
