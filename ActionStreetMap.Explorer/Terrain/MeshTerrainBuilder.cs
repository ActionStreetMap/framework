using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Geometry;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Explorer.Terrain.Layers;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;
using Color32 = UnityEngine.Color32;
using Mesh = UnityEngine.Mesh;

namespace ActionStreetMap.Explorer.Terrain
{
    internal class MeshTerrainBuilder : ITerrainBuilder, IConfigurable
    {
        private const string LogTag = "mesh.terrain";

        private readonly IElevationProvider _elevationProvider;
        private readonly IResourceProvider _resourceProvider;
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly MeshCellBuilder _meshCellBuilder;

        private readonly ILayerBuilder _waterLayerBuilder;
        private readonly ILayerBuilder _carRoadLayerBuilder;
        private readonly ILayerBuilder _walkRoadLayerBuilder;
        private readonly ILayerBuilder _surfaceRoadLayerBuilder;

        [Dependency]
        public ITrace Trace { get; set; }

        private float _maxCellSize = 100;

        [Dependency]
        public MeshTerrainBuilder(
            IElevationProvider elevationProvider,
            IEnumerable<ILayerBuilder> layerBuilders,
            IResourceProvider resourceProvider,
            IGameObjectFactory gameObjectFactory)
        {
            _elevationProvider = elevationProvider;

            _resourceProvider = resourceProvider;
            _gameObjectFactory = gameObjectFactory;
            _meshCellBuilder = new MeshCellBuilder();

            var layerBuildersList = layerBuilders.ToArray();

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
                        try
                        {
                            var cell = _meshCellBuilder.Build(rectangle, meshCanvas);
                            BuildCell(tile, rule, terrainObject, cell, name);
                        }
                        catch (Exception ex)
                        {
                            Trace.Error(LogTag, ex, "Unable to build {0}", name);
                        }
                    }));
                }

            tasks.WhenAll().Wait();

            sw.Stop();
            Trace.Debug(LogTag, "Terrain is build in {0}ms", sw.ElapsedMilliseconds);

            return terrainObject;
        }

        private void BuildCell(Tile tile, Rule rule, IGameObject terrainObject, MeshCell cell, string name)
        {
            var relativeNullPoint = tile.RelativeNullPoint;
            var gradient = GetGradient();

            var terrainMesh = cell.Mesh;

            var vertices = new List<Vector3>(terrainMesh.Vertices.Count);
            var triangles = new List<int>(terrainMesh.Triangles.Count);
            var colors = new List<Color>(terrainMesh.Vertices.Count);

            var triangleIndexMap = new Dictionary<int, int>();
            Trace.Debug(LogTag, "Total triangles: {0}", terrainMesh.Triangles.Count);
            foreach (var triangle in terrainMesh.Triangles)
            {
                var p0 = triangle.GetVertex(0);
                var coord0 = GeoProjection.ToGeoCoordinate(relativeNullPoint,
                    new MapPoint((float) p0.X, (float) p0.Y));
                var ele0 = _elevationProvider.GetElevation(coord0.Latitude, coord0.Longitude);
                if (p0.Type == VertexType.FreeVertex)
                    ele0 += Noise.Perlin3D(new Vector3((float) p0.X, 0, (float) p0.Y), 0.1f);

                vertices.Add(new Vector3((float) p0.X, ele0, (float) p0.Y));

                var p1 = triangle.GetVertex(1);
                var coord1 = GeoProjection.ToGeoCoordinate(relativeNullPoint,
                    new MapPoint((float) p1.X, (float) p1.Y));
                var ele1 = _elevationProvider.GetElevation(coord1.Latitude, coord1.Longitude);
                if (p1.Type == VertexType.FreeVertex)
                    ele1 += Noise.Perlin3D(new Vector3((float) p1.X, 0, (float) p1.Y), 0.1f);
                vertices.Add(new Vector3((float) p1.X, ele1, (float) p1.Y));

                var p2 = triangle.GetVertex(2);
                var coord2 = GeoProjection.ToGeoCoordinate(relativeNullPoint,
                    new MapPoint((float) p2.X, (float) p2.Y));
                var ele2 = _elevationProvider.GetElevation(coord2.Latitude, coord2.Longitude);
                if (p2.Type == VertexType.FreeVertex)
                    ele2 += Noise.Perlin3D(new Vector3((float) p2.X, 0, (float) p2.Y), 0.1f);
                vertices.Add(new Vector3((float) p2.X, ele2, (float) p2.Y));

                var index = vertices.Count;
                triangles.Add(--index);
                triangles.Add(--index);
                triangles.Add(--index);

                var firstValue = (Noise.Perlin3D(new Vector3((float) p0.X, ele0, (float) p0.Y), 0.2f) + 1f)/2f;
                var triangleColor = gradient.Evaluate(firstValue);

                colors.Add(triangleColor);
                colors.Add(triangleColor);
                colors.Add(triangleColor);

                triangleIndexMap.Add(triangle.GetHashCode(), index);
            }

            BuildLayers(new MeshContext
            {
                Tree = new QuadTree(cell.Mesh),
                Iterator = new RegionIterator(cell.Mesh),
                TriangleMap = triangleIndexMap,
                Vertices = vertices,
                Triangles = triangles,
                Colors = colors
            }, cell);

            var goCell = _gameObjectFactory.CreateNew(name, terrainObject);
            Scheduler.MainThread.Schedule(() => BuildGameObject(rule, goCell, vertices, triangles, colors));
        }

        private void BuildLayers(MeshContext context, MeshCell cell)
        {
            _waterLayerBuilder.Build(context, cell.Water);
            _carRoadLayerBuilder.Build(context, cell.CarRoads);
            _walkRoadLayerBuilder.Build(context, cell.WalkRoads);
            foreach (var surfaceRegion in cell.Surfaces)
                _surfaceRoadLayerBuilder.Build(context, surfaceRegion);
        }

        private void BuildGameObject(Rule rule, IGameObject cellObject, List<Vector3> vertices,
            List<int> triangles, List<Color> colors)
        {
            var gameObject = cellObject.GetComponent<GameObject>();

            var meshData = new Mesh();
            meshData.vertices = vertices.ToArray();
            meshData.triangles = triangles.ToArray();
            meshData.colors = colors.ToArray();
            meshData.RecalculateNormals();

            gameObject.AddComponent<MeshRenderer>().material = rule.GetMaterial(_resourceProvider);
            gameObject.AddComponent<MeshFilter>().mesh = meshData;
            gameObject.AddComponent<MeshCollider>();
        }

        private static GradientWrapper GetGradient()
        {
            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            var gck = new GradientWrapper.ColorKey[5];
            gck[0].Color = new Color32(152, 251, 152, 1); // pale green
            gck[0].Time = 0.0f;

            gck[1].Color = new Color32(173, 255, 47, 1); // green yellow
            gck[1].Time = 0.05f;

            gck[2].Color = new Color32(0, 100, 0, 1); // dark green
            gck[2].Time = 0.1f;

            gck[3].Color = new Color32(85, 107, 47, 1); // dark olive green
            gck[3].Time = 0.8f;

            gck[4].Color = new Color32(184, 134, 11, 1); // dark golden rod 
            gck[4].Time = 1f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            var gak = new GradientWrapper.AlphaKey[2];
            gak[0].Alpha = 1f;
            gak[0].Time = 0.0f;
            gak[1].Alpha = 1f;
            gak[1].Time = 1.0f;

            return new GradientWrapper(gck, gak);
        }

        public void Configure(IConfigSection configSection)
        {
            _maxCellSize = configSection.GetFloat("cell.size", 100);
            var maxArea = configSection.GetFloat("maxArea", 10);

            _meshCellBuilder.SetMaxArea(maxArea);
        }
    }
}