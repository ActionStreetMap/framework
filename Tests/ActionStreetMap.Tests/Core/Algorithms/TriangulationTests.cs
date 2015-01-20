using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Osm;
using ActionStreetMap.Osm.Index;
using ActionStreetMap.Osm.Visitors;
using ActionStreetMap.Tests.Osm;
using ActionStreetMap.Models.Geometry;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Algorithms
{
    /// <summary>
    ///     These tests test functionality which seems to be depricated in near future (?)
    /// </summary>
    [TestFixture]
    public class TriangulationTests
    {
        [Test]
        public void CanTriangulateNonStandard()
        {
            // ARRANGE
            var verticies = new List<MapPoint>()
            {
                new MapPoint(669.0f, -181.5f),
                new MapPoint(671.2f, -188.2f),
                new MapPoint(682.9f, -184.4f),
                new MapPoint(688.9f, -202.4f),
                new MapPoint(670.0f, -208.6f),
                new MapPoint(664.1f, -190.5f),
                new MapPoint(671.2f, -188.2f)
            };

            // ACT & ASSERT
            Triangulator.Triangulate(verticies, new List<int>());
        }

        [Test]
        public void CanTriangulateAreasAndWays()
        {
            // ARRANGE
            var sceneVisitor = new TestModelVisitor();
            var pathResolver = new TestPathResolver();
            var config = new Mock<IConfigSection>();
            var objectPool = new ObjectPool();
            config.Setup(c => c.GetString("")).Returns(TestHelper.MapDataPath);
            var elementSourceProvider = new ElementSourceProvider(pathResolver, new FileSystemService(pathResolver));
            elementSourceProvider.Configure(config.Object);
            var loader = new MapTileLoader(elementSourceProvider, sceneVisitor, new ObjectPool());

            var tile = new Tile(TestHelper.BerlinTestFilePoint, new MapPoint(0, 0), 1000);

            loader.Load(tile);

            // ACT & ARRANGE
            Assert.Greater(sceneVisitor.Areas.Count, 0);
            foreach (var area in sceneVisitor.Areas)
            {
                var verticies = new List<MapPoint>();
                PointUtils.GetClockwisePolygonPoints(TestHelper.BerlinTestFilePoint, area.Points, verticies);
                PolygonUtils.Triangulate(verticies, objectPool);
            }

            Assert.Greater(sceneVisitor.Ways.Count, 0);
            foreach (var way in sceneVisitor.Ways)
            {
                var verticies = new List<MapPoint>();
                PointUtils.GetPolygonPoints(TestHelper.BerlinTestFilePoint, way.Points, verticies);
                var triangles = PolygonUtils.Triangulate(verticies, objectPool);
            }
        }
    }
}