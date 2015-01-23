using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Scene.Geometry;
using ActionStreetMap.Explorer.Scene.Geometry.Polygons;
using ActionStreetMap.Explorer.Scene.Geometry.Primitives;
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
        public void CanCutPolygonInTileCrossOneFace()
        {
            // ARRANGE
            var leftBottom = new MapPoint(0, 0);
            var rightUpper = new MapPoint(100, 100);

            var sourceAndResultPoints = new List<MapPoint>()
            {
                new MapPoint(10, 10),
                new MapPoint(20, 10),
                new MapPoint(30, 5),
                new MapPoint(20, -10),
                new MapPoint(10, -10),
                // should be closed
                new MapPoint(10, 10),
            };

            // ACT
            PolygonUtils.ClipPolygonByTile(leftBottom, rightUpper, sourceAndResultPoints);

            // ASSERT
            var expectedPoints = new List<MapPoint>()
            {
                new MapPoint(10, 10),
                new MapPoint(20, 10),
                new MapPoint(30, 5),
                new MapPoint(26.666666f, 0),
                new MapPoint(10, 0),
                new MapPoint(10, 10),
            };
            Assert.IsNotNull(sourceAndResultPoints);
            Assert.AreEqual(expectedPoints.Count, sourceAndResultPoints.Count);
            for (int i =0; i< expectedPoints.Count; i++)
                Assert.AreEqual(expectedPoints[i], sourceAndResultPoints[i]);
        }

        [Test]
        public void CanCutPolygonInTileCrossTwoFaces()
        {
            // ARRANGE
            var leftBottom = new MapPoint(0, 0);
            var rightUpper = new MapPoint(100, 100);

            var sourceAndResultPoints = new List<MapPoint>()
            {
                new MapPoint(30, 30),
                new MapPoint(30, -10),
                new MapPoint(-30, -10),
                new MapPoint(-30, 30),
                // should be closed
                 new MapPoint(30, 30),
            };

            // ACT
            PolygonUtils.ClipPolygonByTile(leftBottom, rightUpper, sourceAndResultPoints);

            // ASSERT
            var expectedPoints = new List<MapPoint>()
            {
                new MapPoint(30, 30),
                new MapPoint(30, 0),
                new MapPoint(0, 0),
                new MapPoint(0, 30),
                new MapPoint(30, 30),
            };
            Assert.IsNotNull(sourceAndResultPoints);
            Assert.AreEqual(expectedPoints.Count, sourceAndResultPoints.Count);
            for (int i = 0; i < expectedPoints.Count; i++)
                Assert.AreEqual(expectedPoints[i], sourceAndResultPoints[i]);
        }

        [Test]
        public void CanCutPolygonInTileCrossThreeFaces()
        {
            // ARRANGE
            var leftBottom = new MapPoint(0, 0);
            var rightUpper = new MapPoint(100, 100);

            var sourceAndResultPoints = new List<MapPoint>()
            {
                new MapPoint(30, 30),
                new MapPoint(30, -10),
                new MapPoint(-30, -10),
                new MapPoint(-30, 30),
                // should be closed
                 new MapPoint(30, 30),
            };

            // ACT
            PolygonUtils.ClipPolygonByTile(leftBottom, rightUpper, sourceAndResultPoints);

            // ASSERT
            var expectedPoints = new List<MapPoint>()
            {
                new MapPoint(30, 30),
                new MapPoint(30, 0),
                new MapPoint(0, 0),
                new MapPoint(0, 30),
                new MapPoint(30, 30),
            };
            Assert.IsNotNull(sourceAndResultPoints);
            Assert.AreEqual(expectedPoints.Count, sourceAndResultPoints.Count);
            for (int i = 0; i < expectedPoints.Count; i++)
                Assert.AreEqual(expectedPoints[i], sourceAndResultPoints[i]);
        }

        [Test]
        public void CanCutPolygonInTileTwoPolyAsResult()
        {
            // ARRANGE
            var leftBottom = new MapPoint(0, 0);
            var rightUpper = new MapPoint(100, 100);

            var sourceAndResultPoints = new List<MapPoint>()
            {
                new MapPoint(20, 20),
                new MapPoint(20, -20),
                new MapPoint(-100, -20),
                new MapPoint(-100, 120),
                new MapPoint(20, 120),
                new MapPoint(20, 80),
                new MapPoint(-20, 80),
                new MapPoint(-20, 20),
                // should be closed
                new MapPoint(20, 20),
            };

            // ACT
            PolygonUtils.ClipPolygonByTile(leftBottom, rightUpper, sourceAndResultPoints);

            // ASSERT
            var expectedPoints = new List<MapPoint>()
            {
                new MapPoint(20.0f, 20.0f),
                new MapPoint(20.0f, 0.0f),
                new MapPoint(0.0f, 0.0f),
                new MapPoint(0.0f, 100.0f),
                new MapPoint(20.0f, 100.0f),
                new MapPoint(20.0f, 80.0f),
                new MapPoint(0.0f, 80.0f),
                new MapPoint(0.0f, 20.0f),
                new MapPoint(20.0f, 20.0f),
            };
            Assert.IsNotNull(sourceAndResultPoints);
            Assert.AreEqual(expectedPoints.Count, sourceAndResultPoints.Count);
            for (int i = 0; i < expectedPoints.Count; i++)
                Assert.AreEqual(expectedPoints[i], sourceAndResultPoints[i]);
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
