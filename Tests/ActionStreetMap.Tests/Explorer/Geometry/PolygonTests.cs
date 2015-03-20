using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Polygons;
using ActionStreetMap.Explorer.Geometry.Primitives;
using ActionStreetMap.Explorer.Geometry.Utils;
using NUnit.Framework;
using UnityEngine;

namespace ActionStreetMap.Tests.Explorer.Geometry
{
    [TestFixture]
    public class PolygonTests
    {
        [Test]
        public void CanCreatePolygonFromMapPoints()
        {
            // ARRANGE
            var mapPoints = new List<MapPoint>()
            {
                new MapPoint(0, 0),
                new MapPoint(0, 100),
                new MapPoint(100, 100),
                new MapPoint(100, 0)
            };

            // ACT
            var polygon = new Polygon(mapPoints);

            // ASSERT
            Assert.AreEqual(4, polygon.Verticies.Length);
            Assert.AreEqual(4, polygon.Segments.Length);
            Assert.AreEqual(new Vector3(0, 0, 0), polygon.Segments[0].Start);
            Assert.AreEqual(new Vector3(0, 0, 100), polygon.Segments[0].End);
            Assert.AreEqual(new Vector3(0, 0, 100), polygon.Segments[1].Start);
            Assert.AreEqual(new Vector3(100, 0, 100), polygon.Segments[1].End);
            Assert.AreEqual(new Vector3(100, 0, 100), polygon.Segments[2].Start);
            Assert.AreEqual(new Vector3(100, 0, 0), polygon.Segments[2].End);
            Assert.AreEqual(new Vector3(100, 0, 0), polygon.Segments[3].Start);
            Assert.AreEqual(new Vector3(0, 0, 0), polygon.Segments[3].End);
        }

        [Test]
        public void CanGetCentroid()
        {
            // ARRANGE
            var polygon = new List<MapPoint>()
            {
                new MapPoint(0, 0),
                new MapPoint(0, 10),
                new MapPoint(10, 10),
                new MapPoint(10, 0),
            };

            // ACT
            var center = PolygonUtils.GetCentroid(polygon);

            // ASSERT
            Assert.AreEqual(new MapPoint(5, 5), center);
        }

        [Test]
        public void CanCalculateStraightSkeleton()
        {
            // ARRANGE
            var polygon = new List<MapPoint>()
            {
                new MapPoint(0, 0),
                new MapPoint(0, 10),
                new MapPoint(10, 20),
                new MapPoint(20, 10),
                new MapPoint(20, 0),
            };

            // ACT
            var skeleton = StraightSkeleton.Calculate(polygon);

            // ASSERT
            Assert.IsNotNull(skeleton);
            Assert.AreEqual(21, skeleton.Item1.Count);
            Assert.AreEqual(2, skeleton.Item2.Count);
        }
    }
}
