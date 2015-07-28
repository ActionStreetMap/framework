using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Explorer.Scene.Roofs;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Maps.Data.Helpers;
using ActionStreetMap.Tests.Core.MapCss;
using ActionStreetMap.Unity.Wrappers;
using Moq;
using NUnit.Framework;

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
            var resourceProvider = new Mock<IResourceProvider>();
            resourceProvider.Setup(r => r.GetGradient(It.IsAny<string>()))
                .Returns(GradientUtils.ParseGradient("gradient(#f4a460, #614126 50%, #302013)"));

            var objectPoll = TestHelper.GetObjectPool();
            _barrierModelBuilder = new TestableBarrierModelBuilder();
            _barrierModelBuilder.ObjectPool = objectPoll;
            _barrierModelBuilder.GameObjectFactory = new GameObjectFactory();
            _barrierModelBuilder.ResourceProvider = resourceProvider.Object;

            _tile = new Tile(TestHelper.BerlinTestFilePoint,
                new Vector2d(0, 0), RenderMode.Overview,
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
            Assert.IsNotNull(_barrierModelBuilder.MeshData);
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
