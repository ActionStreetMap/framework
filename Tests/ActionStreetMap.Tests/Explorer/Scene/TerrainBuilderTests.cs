using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Terrain;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using NUnit.Framework;
using UnityEngine;
using Canvas = ActionStreetMap.Core.Tiling.Models.Canvas;
using Component = ActionStreetMap.Infrastructure.Dependencies.Component;
using RenderMode = ActionStreetMap.Core.RenderMode;

namespace ActionStreetMap.Tests.Explorer.Scene
{
    [TestFixture]
    class TerrainBuilderTests
    {
        private const double TileSize = 400;

        private TestTerrainBuilder _terrainBuilder;
        private IObjectPool _objectPool;
        private Stylesheet _stylesheet;

        [SetUp]
        public void SetUp()
        {
            var container = new Container();
            var gameRunner = TestHelper.GetGameRunner(container);
            container.Register(Component
                .For<ITerrainBuilder>()
                .Use<TestTerrainBuilder>()
                .Singleton());

            gameRunner.RunGame(TestHelper.BerlinTestFilePoint);

            _terrainBuilder = container.Resolve<ITerrainBuilder>() as TestTerrainBuilder;
            _objectPool = container.Resolve<IObjectPool>();
            _stylesheet = container.Resolve<IStylesheetProvider>().Get();

            Assert.IsNotNull(_terrainBuilder);
            Assert.IsNotNull(_objectPool);
            Assert.IsNotNull(_stylesheet);
        }

        [Test]
        public void CanBuildTerrainInSceneMode()
        {
            // ARRANGE
            var tile = CreateTile(RenderMode.Scene);
            var rule = _stylesheet.GetCanvasRule(tile.Canvas);

            // ACT
            _terrainBuilder.Build(tile, rule);

            // ASSERT
            AssertTerrainBuilderResults();
        }

        [Test]
        public void CanBuildTerrainInOverviewMode()
        {
            // ARRANGE
            var tile = CreateTile(RenderMode.Overview);
            var rule = _stylesheet.GetCanvasRule(tile.Canvas);

            // ACT
            _terrainBuilder.Build(tile, rule);

            // ASSERT
            AssertTerrainBuilderResults();
        }

        private Tile CreateTile(RenderMode renderMode)
        {
            return new Tile(TestHelper.BerlinTestFilePoint,
                new Vector2d(0, 0), renderMode,
                new Canvas(_objectPool), TileSize, TileSize);
        }

        private void AssertTerrainBuilderResults()
        {
            Assert.IsNotNull(_terrainBuilder.MeshData);
            Assert.IsNotNull(_terrainBuilder.MeshData.Triangles);

            var trisCount = _terrainBuilder.MeshData.Triangles.Count;

            Assert.Greater(trisCount, 0);

            Assert.AreEqual(trisCount * 3, _terrainBuilder.Vertices.Length);
            Assert.AreEqual(trisCount * 3, _terrainBuilder.Triangles.Length);
            Assert.AreEqual(trisCount * 3, _terrainBuilder.Colors.Length);
        }

        #region Nested class

        private class TestTerrainBuilder : TerrainBuilder
        {
            public TerrainMeshData MeshData;
            public Vector3[] Vertices;
            public int[] Triangles;
            public Color[] Colors;

            [Dependency]
            public TestTerrainBuilder(BehaviourProvider behaviourProvider, 
                                      IElevationProvider elevationProvider, 
                                      IResourceProvider resourceProvider, 
                                      IGameObjectFactory gameObjectFactory, 
                                      IObjectPool objectPool) : 
                base(behaviourProvider, elevationProvider, resourceProvider, gameObjectFactory, objectPool)
            {
            }

            protected override void BuildObject(IGameObject cellGameObject, Canvas canvas, Rule rule, 
                TerrainMeshData meshData)
            {
                MeshData = meshData;
                meshData.GenerateObjectData(out Vertices, out Triangles, out Colors);
            }
        }

        #endregion
    }
}
