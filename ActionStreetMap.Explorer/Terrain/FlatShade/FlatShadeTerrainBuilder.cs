using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Roads;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain.FlatShade
{
    /// <summary> Provides way to build terrain in flat shading style. </summary>
    internal class FlatShadeTerrainBuilder: ITerrainBuilder
    {
        private const string LogTag = "terrain.flatshade";

        private readonly IRoadBuilder _roadBuilder;
        private readonly IThemeProvider _themeProvider;
        private readonly IResourceProvider _resourceProvider;
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly IObjectPool _objectPool;
        private readonly HeightMapProcessor _heightMapProcessor;

        // TODO make it configurable
        private string _materialKey = @"Materials/Terrain/TerrainBase";

        /// <summary> Gets or sets trace. </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates instance of <see cref="FlatShadeTerrainBuilder"/>. </summary>
        /// <param name="roadBuilder">Road builder.</param>
        /// <param name="themeProvider">Theme provider.</param>
        /// <param name="resourceProvider">Resource provider.</param>
        /// <param name="gameObjectFactory">GameObject factory.</param>
        /// <param name="objectPool">Object pool.</param>
        /// <param name="heightMapProcessor">Heightmap processor.</param>
        [Dependency]
        public FlatShadeTerrainBuilder(IRoadBuilder roadBuilder, IThemeProvider themeProvider, 
            IResourceProvider resourceProvider, IGameObjectFactory gameObjectFactory, 
            IObjectPool objectPool, HeightMapProcessor heightMapProcessor)
        {
            _roadBuilder = roadBuilder;
            _themeProvider = themeProvider;
            _resourceProvider = resourceProvider;
            _gameObjectFactory = gameObjectFactory;
            _objectPool = objectPool;
            _heightMapProcessor = heightMapProcessor;
        }

        /// <inheritdoc />
        public IGameObject Build(Tile tile, Rule rule)
        {
            Trace.Debug(LogTag, "build start");

            float size = 300;
            int resolution = 180;
            int defaultGradient = 0;

            // TODO remove hardcoded parameters - read them from rule
            var gridBuilder = _objectPool.NewObject(() => new TerrainGridBuilder(size, resolution));

            var leftBottom = new Vector2(tile.BottomLeft.X, tile.BottomLeft.Y);

            // process roads and elevation areas
            ProcessTerrainObjects(tile);

            Trace.Debug(LogTag, "build cells..");
            // create terrain cells
            var terrainCells = gridBuilder
                .Move(leftBottom, tile.HeightMap.Data, _resourceProvider.GetGradient(defaultGradient))
                .Fill(GetGradientSurfaces(tile))
                .Build();
                 //.ToList(); // for console builds

            var gameObject = _gameObjectFactory.CreateNew("terrain", tile.GameObject);

            Scheduler.MainThread.Schedule(() =>
            {
                CreateTerrainObjects(tile, gameObject, terrainCells);
                _objectPool.StoreObject(gridBuilder);
                tile.Canvas.Dispose();
            });

            return gameObject;
        }

        private void ProcessTerrainObjects(Tile tile)
        {
            var canvas = tile.Canvas;
            var heightMap = tile.HeightMap;
            var roadStyleProvider = _themeProvider.Get().GetStyleProvider<IRoadStyleProvider>();

            Trace.Debug(LogTag, "build road graph..");
            var roadGraph = canvas.BuildRoadGraph();

            Trace.Debug(LogTag, "build roads..");
            var roadObservable = roadGraph.Roads.ToObservable();
            roadObservable.Subscribe(road =>
            {
                road.GameObject = _gameObjectFactory.CreateNew("road", tile.GameObject);
                _roadBuilder.BuildRoad(heightMap, road, roadStyleProvider.Get(road));
            });

            var junctionObservable = roadGraph.Junctions.ToObservable();
            junctionObservable.Subscribe(junction =>
            {
                junction.GameObject = _gameObjectFactory.CreateNew("junction", tile.GameObject);
                _roadBuilder.BuildJunction(heightMap, junction, roadStyleProvider.Get(junction));
            });

            roadObservable.Wait();
            junctionObservable.Wait();
            

            Trace.Debug(LogTag, "process elevation areas");
            // NOTE We have to do this in the last order. Otherwise, new height
            // value can affect other models (e.g. water vs road)
            if (canvas.Elevations.Any())
            {
                var elevationObservable = canvas.Elevations.ToObservable();
                elevationObservable.Subscribe(elevationArea =>
                    _heightMapProcessor.AdjustPolygon(heightMap, elevationArea.Points, elevationArea.AverageElevation));
                elevationObservable.Wait();
            }
        }

        private IEnumerable<GradientSurface> GetGradientSurfaces(Tile tile)
        {
            foreach (var surface in tile.Canvas.Areas)
            {
                yield return new GradientSurface()
                {
                    Gradient = _resourceProvider.GetGradient(surface.SplatIndex),
                    Points = surface.Points
                };
            }
        }

        private void CreateTerrainObjects(Tile tile, IGameObject gameObject, IEnumerable<TerrainCellMesh> meshes)
        {
            Trace.Debug(LogTag, "create terrain objects..");
            var grid = gameObject.GetComponent<GameObject>();

            grid.transform.parent = tile.GameObject.GetComponent<GameObject>().transform;

            foreach (var terrainCellMesh in meshes)
            {
                var mesh = new Mesh
                {
                    vertices = terrainCellMesh.Vertices,
                    triangles = terrainCellMesh.Triangles,
                    colors = terrainCellMesh.Colors
                };

                mesh.RecalculateNormals();

                var cell = new GameObject(terrainCellMesh.Name);
                cell.transform.parent = grid.transform;
                cell.AddComponent<MeshRenderer>().material = 
                    _resourceProvider.GetMatertial(_materialKey);
                cell.AddComponent<MeshFilter>().mesh = mesh;
                cell.AddComponent<MeshCollider>();
            }

            Trace.Debug(LogTag, "build finished");
            tile.Canvas.Dispose();
        }
    }
}
