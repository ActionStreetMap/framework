using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Polygons.Geometry;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using UnityEngine;
using Mesh = ActionStreetMap.Core.Polygons.Mesh;

namespace ActionStreetMap.Explorer.Terrain
{
    internal class MeshTerrainBuilder
    {
        private const string LogTag = "mesh.terrain";

        private readonly IElevationProvider _elevationProvider;

        [Dependency]
        public ITrace Trace { get; set; }

        [Dependency]
        public MeshTerrainBuilder(IElevationProvider elevationProvider)
        {
            _elevationProvider = elevationProvider;
        }

        public IGameObject Build(Tile tile, Mesh terrainMesh, List<MeshRegion> meshRegions)
        {
            var vertices = new List<Vector3>(terrainMesh.Vertices.Count);
            var triangles = new List<int>(terrainMesh.Triangles.Count);

            var hashMap = new Dictionary<int, int>();
            Trace.Debug(LogTag, "Total triangles: {0}", terrainMesh.Triangles.Count);
            foreach (var triangle in terrainMesh.Triangles)
            {
                var p0 = triangle.GetVertex(0);
                var coord0 = GeoProjection.ToGeoCoordinate(tile.RelativeNullPoint, new MapPoint((float)p0.X, (float)p0.Y));
                var ele0 = _elevationProvider.GetElevation(coord0.Latitude, coord0.Longitude);
                //ele0 += elevationNoise.GetValue((float)p0.X, ele0, (float)p0.X);
                vertices.Add(new Vector3((float)p0.X, ele0, (float)p0.Y));

                var p1 = triangle.GetVertex(1);
                var coord1 = GeoProjection.ToGeoCoordinate(tile.RelativeNullPoint, new MapPoint((float)p1.X, (float)p1.Y));
                var ele1 = _elevationProvider.GetElevation(coord1.Latitude, coord1.Longitude);
                //ele1 += elevationNoise.GetValue((float)p1.X, ele1, (float)p1.X);
                vertices.Add(new Vector3((float)p1.X, ele1, (float)p1.Y));

                var p2 = triangle.GetVertex(2);
                var coord2 = GeoProjection.ToGeoCoordinate(tile.RelativeNullPoint, new MapPoint((float)p2.X, (float)p2.Y));
                var ele2 = _elevationProvider.GetElevation(coord2.Latitude, coord2.Longitude);
                //ele2 += elevationNoise.GetValue((float) p2.X, ele2, (float) p2.X);
                vertices.Add(new Vector3((float)p2.X, ele2, (float)p2.Y));

                var index = vertices.Count;
                triangles.Add(--index);
                triangles.Add(--index);
                triangles.Add(--index);

                hashMap.Add(triangle.GetHashCode(), index);
            }

            FillRegions(terrainMesh, meshRegions, hashMap);

            return null;
        }

        private void FillRegions(Mesh mesh, List<MeshRegion> meshRegions, Dictionary<int, int> hashMap)
        {
            var tree = new QuadTree(mesh);
            RegionIterator iterator = new RegionIterator(mesh);
            foreach (var region in meshRegions)
            {
                var point = region.Anchor;
                var start = (Triangle)tree.Query(point.X, point.Y);

                int count = 0;
                iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    count++;
                });
                Trace.Debug(LogTag, "Region: {0}, processed: {1}", start.Region, count);
            }
        }
    }
}
