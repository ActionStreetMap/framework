using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Polygons.Geometry;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Explorer.Terrain.Layers;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Terrain
{
    internal class MeshTerrainBuilder : ITerrainBuilder, IConfigurable
    {
        private const string LogTag = "mesh.terrain";

        private readonly IResourceProvider _resourceProvider;
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly MeshCellBuilder _meshCellBuilder;

        private readonly ILayerBuilder _canvasLayerBuilder;
        private readonly ILayerBuilder _waterLayerBuilder;
        private readonly ILayerBuilder _carRoadLayerBuilder;
        private readonly ILayerBuilder _walkRoadLayerBuilder;
        private readonly ILayerBuilder _surfaceRoadLayerBuilder;

        [Dependency]
        public ITrace Trace { get; set; }

        private float _maxCellSize = 100;

        [Dependency]
        public MeshTerrainBuilder(IEnumerable<ILayerBuilder> layerBuilders,
            IResourceProvider resourceProvider,
            IGameObjectFactory gameObjectFactory)
        {
            _resourceProvider = resourceProvider;
            _gameObjectFactory = gameObjectFactory;
            _meshCellBuilder = new MeshCellBuilder();

            var layerBuildersList = layerBuilders.ToArray();

            _canvasLayerBuilder = layerBuildersList.Single(l => l.Name == "canvas");
            _waterLayerBuilder = layerBuildersList.Single(l => l.Name == "water");
            _carRoadLayerBuilder = layerBuildersList.Single(l => l.Name == "car");
            _walkRoadLayerBuilder = layerBuildersList.Single(l => l.Name == "walk");
            _surfaceRoadLayerBuilder = layerBuildersList.Single(l => l.Name == "surface");
        }

        public IGameObject Build(Tile tile, Rule rule)
        {
            Trace.Debug(LogTag, "Started to build terrain");
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var terrainObject = _gameObjectFactory.CreateNew("terrain", tile.GameObject);

            // detect grid parameters
            var cellRowCount = (int) Math.Ceiling(tile.Height/_maxCellSize);
            var cellColumnCount = (int) Math.Ceiling(tile.Width/_maxCellSize);
            var cellHeight = tile.Height/cellRowCount;
            var cellWidth = tile.Width/cellColumnCount;

            Trace.Debug(LogTag, "Building mesh canvas..");
            var meshCanvas = new MeshCanvasBuilder()
                .SetTile(tile)
                .SetScale(MeshCellBuilder.Scale)
                .Build();

            Trace.Debug(LogTag, "Building mesh cells..");
            var tasks = new List<IObservable<Unit>>(cellRowCount*cellColumnCount);
            for (int j = 0; j < cellRowCount; j++)
                for (int i = 0; i < cellColumnCount; i++)
                {
                    var tileBottomLeft = tile.Rectangle.BottomLeft;
                    var rectangle = new Rectangle(
                        tileBottomLeft.X + i*cellWidth,
                        tileBottomLeft.Y + j*cellHeight,
                        cellWidth,
                        cellHeight);
                    var name = String.Format("cell {0}_{1}", i, j);
                    tasks.Add(Observable.Start(() =>
                    {
                        var cell = _meshCellBuilder.Build(rectangle, meshCanvas);
                        BuildCell(rule, terrainObject, cell, name);
                    }));
                }

            tasks.WhenAll().Wait();

            sw.Stop();
            Trace.Debug(LogTag, "Terrain is build in {0}ms", sw.ElapsedMilliseconds);

            return terrainObject;
        }

        private void BuildCell(Rule rule, IGameObject terrainObject, MeshCell cell, string name)
        {
            var terrainMesh = cell.Mesh;
            Trace.Debug(LogTag, "Total triangles: {0}", terrainMesh.Triangles.Count);

            var context = new MeshContext
            {
                Mesh = terrainMesh,
                Tree = new QuadTree(cell.Mesh),
                Iterator = new RegionIterator(cell.Mesh),
                // TODO use object pool
                TriangleMap = new Dictionary<int, int>(),
                Vertices = new List<Vector3>(terrainMesh.Vertices.Count),
                Triangles = new List<int>(terrainMesh.Triangles.Count),
                Colors = new List<Color>(terrainMesh.Vertices.Count)
            };

            // build canvas
            _canvasLayerBuilder.Build(context, null);
            // build extra layers
            _waterLayerBuilder.Build(context, cell.Water);
            _carRoadLayerBuilder.Build(context, cell.CarRoads);
            _walkRoadLayerBuilder.Build(context, cell.WalkRoads);
            foreach (var surfaceRegion in cell.Surfaces)
                _surfaceRoadLayerBuilder.Build(context, surfaceRegion);

            var goCell = _gameObjectFactory.CreateNew(name, terrainObject);
            Scheduler.MainThread.Schedule(() => BuildGameObject(rule, goCell, context));
        }

        private void BuildGameObject(Rule rule, IGameObject cellObject, MeshContext context)
        {
            var gameObject = cellObject.GetComponent<GameObject>();

            var meshData = new Mesh();
            meshData.vertices = context.Vertices.ToArray();
            meshData.triangles = context.Triangles.ToArray();
            meshData.colors = context.Colors.ToArray();
            meshData.RecalculateNormals();

            gameObject.AddComponent<MeshRenderer>().material = rule.GetMaterial(_resourceProvider);
            gameObject.AddComponent<MeshFilter>().mesh = meshData;
            gameObject.AddComponent<MeshCollider>();
        }

        public void Configure(IConfigSection configSection)
        {
            _maxCellSize = configSection.GetFloat("cell.size", 100);
            var maxArea = configSection.GetFloat("maxArea", 10);

            _meshCellBuilder.SetMaxArea(maxArea);
        }
    }
}