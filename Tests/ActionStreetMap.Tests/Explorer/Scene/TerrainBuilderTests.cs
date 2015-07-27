using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.MapCss;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Scene.Terrain;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Scene
{
    [TestFixture]
    class TerrainBuilderTests
    {
        private const double TileSize = 400;

        private TerrainBuilder _terrainBuilder;
        private IObjectPool _objectPool;
        private Stylesheet _stylesheet;

        [SetUp]
        public void SetUp()
        {
            var container = new Container();
            TestHelper.GetGameRunner(container);

            _terrainBuilder = container.Resolve<ITerrainBuilder>() as TerrainBuilder;
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
        }

        private Tile CreateTile(RenderMode renderMode)
        {
            return new Tile(TestHelper.BerlinInvalidenStr,
                new Vector2d(0, 0), renderMode,
                new Canvas(_objectPool), TileSize, TileSize);
        }
    }
}
