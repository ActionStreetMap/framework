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
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

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

            var terrainObject = _gameObjectFactory.CreateNew("terrain");
            terrainObject.Parent = tile.GameObject;

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
            for (int j = 0; j < rowCount; j++)
                for (int i = 0; i < columnCount; i++)
                {
                    var cell = cells[i, j];
                    var terrainMesh = cell.Mesh;

                    var vertices = new List<Vector3>(terrainMesh.Vertices.Count);
                    var triangles = new List<int>(terrainMesh.Triangles.Count);

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

                        hashMap.Add(triangle.GetHashCode(), index);
                    }

                    FillRegions(cell, hashMap);

                    var goCell = _gameObjectFactory.CreateNew(String.Format("cell {0}_{1}", i, j));
                    goCell.Parent = terrainObject;

                    Scheduler.MainThread.Schedule(() => BuildGameObject(rule, goCell, vertices, triangles, null));
                }
        }

        private void FillRegions(MeshGridCell cell, Dictionary<int, int> hashMap)
        {
            var tree = new QuadTree(cell.Mesh);
            RegionIterator iterator = new RegionIterator(cell.Mesh);
            foreach (var point in cell.Roads)
            {
                var start = (Triangle) tree.Query(point.X, point.Y);

                int count = 0;
                iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    count++;
                });
                Trace.Debug(LogTag, "Region processed: {0}", count);
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
    }
}