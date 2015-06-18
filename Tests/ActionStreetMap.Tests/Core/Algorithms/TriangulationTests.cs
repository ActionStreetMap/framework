using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Tests.Maps;
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
            var sceneVisitor = new TestModelLoader();
            var pathResolver = new TestPathResolver();
            var config = new Mock<IConfigSection>();
            var objectPool = TestHelper.GetObjectPool();
            config.Setup(c => c.GetString("local", null)).Returns(TestHelper.MapDataPath);
            var elementSourceProvider = new ElementSourceProvider(pathResolver, TestHelper.GetFileSystemService(), objectPool);
            elementSourceProvider.Trace = new ConsoleTrace();
            elementSourceProvider.Configure(config.Object);

            var elevationProvider = new Mock<IElevationProvider>();
            elevationProvider.Setup(e => e.HasElevation(It.IsAny<BoundingBox>())).Returns(true);
            var loader = new MapTileLoader(elementSourceProvider, elevationProvider.Object, sceneVisitor, objectPool);

            var tile = new Tile(TestHelper.BerlinTestFilePoint, new MapPoint(0, 0), RenderMode.Scene,
                new Canvas(objectPool), 1000, 1000);
            loader.Load(tile).Wait();

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