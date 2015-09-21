using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Maps.Data.Helpers;
using ActionStreetMap.Tests.Core.MapCss;
using Moq;
using NUnit.Framework;
using UnityEngine;
using Canvas = ActionStreetMap.Core.Tiling.Models.Canvas;
using RenderMode = ActionStreetMap.Core.RenderMode;

namespace ActionStreetMap.Tests.Explorer.Scene
{
    [TestFixture]
    class BarrierModelBuilderTests
    {
        private TestableBarrierModelBuilder _barrierModelBuilder;
        private Tile _tile;
        private Stylesheet _stylesheet;

        [SetUp]
        public void SetUp()
        {
            var objectPoll = TestHelper.GetObjectPool();
            _barrierModelBuilder = new TestableBarrierModelBuilder();
            _barrierModelBuilder.ObjectPool = objectPoll;
            _barrierModelBuilder.GameObjectFactory = new GameObjectFactory();
            _barrierModelBuilder.ElevationProvider = new Mock<IElevationProvider>().Object;
            _barrierModelBuilder.CustomizationService = TestHelper.GetCustomizationService();

            _tile = new Tile(TestHelper.BerlinTestFilePoint,
                new Vector2d(0, 0), RenderMode.Scene,
                new Canvas(objectPoll), 400, 400);

            _stylesheet = MapCssHelper.GetStylesheetFromFile(TestHelper.DefaultMapcssFile);
        }

        [Test]
        public void CanBuildBarrier()
        {
            // ARRANGE
            var points = new List<Vector2d>()
            {
                new Vector2d(0, 0),
                new Vector2d(10, 0),
                new Vector2d(10, 10)
            };
            var way = CreateWay(points);
            var rule = _stylesheet.GetModelRule(way, ZoomHelper.GetZoomLevel(_tile.RenderMode));
            
            // ACT
            _barrierModelBuilder.BuildWay(_tile, rule, CreateWay(points));

            // ASSERT
            var meshData = _barrierModelBuilder.MeshData;
            Assert.IsNotNull(meshData);
            Assert.IsNotNull(meshData.GameObject);
            Assert.IsNotNull(meshData.MaterialKey);
            Assert.IsNotNull(meshData.Index);

            Assert.AreEqual(144, meshData.Vertices.Length);
            Assert.AreEqual(144, meshData.Triangles.Length);
            Assert.AreEqual(144, meshData.Colors.Length);

            AssertPoints(points[0], meshData.Vertices[0]);
        }

        private void AssertPoints(Vector2d p1, Vector3 p2)
        {
            Assert.AreEqual(p1.X, p2.x);
            Assert.AreEqual(p1.Y, p2.z);
        }

        private Way CreateWay(List<Vector2d> points)
        {
            return new Way()
            {
                Tags = new Dictionary<string,string>() { {"barrier", "yes"}}.ToTags(),
                Points = points.Select(p => GeoProjection
                    .ToGeoCoordinate(TestHelper.BerlinTestFilePoint, p))
                    .ToList()
            };
        }

        #region Nested class

        private class TestableBarrierModelBuilder : BarrierModelBuilder
        {
            public MeshData MeshData;

            protected override void BuildObject(IGameObject parent, MeshData meshData, Rule rule, Model model)
            {
                MeshData = meshData;
            }
        }

        #endregion
    }
}
