using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;
using Color32 = UnityEngine.Color32;

namespace ActionStreetMap.Explorer.Terrain
{
    internal class MeshTerrainBuilder : ITerrainBuilder
    {
        private const string LogTag = "mesh.terrain";

        private readonly IElevationProvider _elevationProvider;
        private readonly IResourceProvider _resourceProvider;
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly MeshGridBuilder _meshGridBuilder;

        [Dependency]
        public ITrace Trace { get; set; }

        [Dependency]
        public MeshTerrainBuilder(IElevationProvider elevationProvider, 
            IResourceProvider resourceProvider,
            IGameObjectFactory gameObjectFactory)
        {
            _elevationProvider = elevationProvider;
            _resourceProvider = resourceProvider;
            _gameObjectFactory = gameObjectFactory;
            _meshGridBuilder = new MeshGridBuilder();
        }

        public IGameObject Build(Tile tile, Rule rule)
        {
            Trace.Debug(LogTag, "started to build terrain");
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var terrainObject = _gameObjectFactory.CreateNew("terrain", tile.GameObject);

            var meshGrid = _meshGridBuilder.Build(tile);
            BuildCells(tile, rule, terrainObject, meshGrid);

            sw.Stop();
            Trace.Debug(LogTag, "Terrain is build in {0}ms", sw.ElapsedMilliseconds);

            return terrainObject;
        }

        private void BuildCells(Tile tile, Rule rule, IGameObject terrainObject, MeshGridCell[,] cells)
        {
            var relativeNullPoint = tile.RelativeNullPoint;
            var rowCount = cells.GetLength(0);
            var columnCount = cells.GetLength(1);
            var gradient = GetGradient();

            for (int j = 0; j < rowCount; j++)
                for (int i = 0; i < columnCount; i++)
                {
                    var cell = cells[i, j];
                    var terrainMesh = cell.Mesh;

                    var vertices = new List<Vector3>(terrainMesh.Vertices.Count);
                    var triangles = new List<int>(terrainMesh.Triangles.Count);
                    var colors = new List<Color>(terrainMesh.Vertices.Count);

                    var hashMap = new Dictionary<int, int>();
                    Trace.Debug(LogTag, "Total triangles: {0}", terrainMesh.Triangles.Count);
                    foreach (var triangle in terrainMesh.Triangles)
                    {
                        var p0 = triangle.GetVertex(0);
                        var coord0 = GeoProjection.ToGeoCoordinate(relativeNullPoint,
                            new MapPoint((float) p0.X, (float) p0.Y));
                        var ele0 = _elevationProvider.GetElevation(coord0.Latitude, coord0.Longitude);
                        //ele0 += elevationNoise.GetValue((float)p0.X, ele0, (float)p0.X);
                        vertices.Add(new Vector3((float) p0.X, ele0, (float) p0.Y));

                        var p1 = triangle.GetVertex(1);
                        var coord1 = GeoProjection.ToGeoCoordinate(relativeNullPoint,
                            new MapPoint((float) p1.X, (float) p1.Y));
                        var ele1 = _elevationProvider.GetElevation(coord1.Latitude, coord1.Longitude);
                        //ele1 += elevationNoise.GetValue((float)p1.X, ele1, (float)p1.X);
                        vertices.Add(new Vector3((float) p1.X, ele1, (float) p1.Y));

                        var p2 = triangle.GetVertex(2);
                        var coord2 = GeoProjection.ToGeoCoordinate(relativeNullPoint,
                            new MapPoint((float) p2.X, (float) p2.Y));
                        var ele2 = _elevationProvider.GetElevation(coord2.Latitude, coord2.Longitude);
                        //ele2 += elevationNoise.GetValue((float) p2.X, ele2, (float) p2.X);
                        vertices.Add(new Vector3((float) p2.X, ele2, (float) p2.Y));

                        var index = vertices.Count;
                        triangles.Add(--index);
                        triangles.Add(--index);
                        triangles.Add(--index);

                        var firstValue = (Noise.Perlin3D(new Vector3((float)p0.X, ele0, (float)p0.Y), 0.2f) + 1f) / 2f;
                        var triangleColor = gradient.Evaluate(firstValue);

                        colors.Add(triangleColor);
                        colors.Add(triangleColor);
                        colors.Add(triangleColor);

                        hashMap.Add(triangle.GetHashCode(), index);
                    }

                    FillRegions(cell, colors, hashMap);

                    var goCell = _gameObjectFactory.CreateNew(String.Format("cell {0}_{1}", i, j), terrainObject);
                    Scheduler.MainThread.Schedule(() => BuildGameObject(rule, goCell, vertices, triangles, colors));
                }
        }

        private void FillRegions(MeshGridCell cell, List<Color> colors, Dictionary<int, int> hashMap)
        {
            // TODO this should be refactored
            var tree = new QuadTree(cell.Mesh);
            RegionIterator iterator = new RegionIterator(cell.Mesh);
            foreach (var region in cell.Roads)
            {
                var point = region.Anchor;
                var start = (Triangle)tree.Query(point.X, point.Y);

                int count = 0;
                var color = Color.red;
                iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    colors[index] = color;
                    colors[index + 1] = color;
                    colors[index + 2] = color;
                    count++;
                });
                Trace.Debug(LogTag, "Road region processed: {0}", count);
            }

            foreach (var region in cell.Surfaces)
            {
                var point = region.Anchor;
                var start = (Triangle)tree.Query(point.X, point.Y);

                int count = 0;
                var color = GetColorBySplatId(region.SplatId);
                iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    colors[index] = color;
                    colors[index + 1] = color;
                    colors[index + 2] = color;
                    count++;
                });
                Trace.Debug(LogTag, "Surface region processed: {0}", count);
            }
        }

        private Color GetColorBySplatId(int id)
        {
            switch (id%3)
            {
                case 0: return Color.yellow;
                case 1: return Color.green;
                default: return Color.blue;
            }
        }

        private void BuildGameObject(Rule rule, IGameObject cellObject, 
            List<Vector3> vertices, List<int> triangles, List<Color> colors)
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

        private static Gradient GetGradient()
        {
            var gradient = new Gradient();
            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            var gck = new GradientColorKey[5];
            gck[0].color = new Color32(152, 251, 152, 1); // pale green
            gck[0].time = 0.0f;

            gck[1].color = new Color32(173, 255, 47, 1); // green yellow
            gck[1].time = 0.05f;

            gck[2].color = new Color32(0, 100, 0, 1); // dark green
            gck[2].time = 0.1f;

            gck[3].color = new Color32(85, 107, 47, 1); // dark olive green
            gck[3].time = 0.8f;

            gck[4].color = new Color32(184, 134, 11, 1); // dark golden rod 
            gck[4].time = 1f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            var gak = new GradientAlphaKey[2];
            gak[0].alpha = 1f;
            gak[0].time = 0.0f;
            gak[1].alpha = 1f;
            gak[1].time = 1.0f;
            gradient.SetKeys(gck, gak);

            return gradient;
        }
    }
}