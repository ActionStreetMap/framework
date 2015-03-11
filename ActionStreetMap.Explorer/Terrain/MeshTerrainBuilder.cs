using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using UnityEngine;

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

        public IGameObject Build(MeshGrid terrainGrid)
        {
            var relativeNullPoint = terrainGrid.RelativeNullPoint;

            for (int i = 0; i < terrainGrid.Cells.Length; i++)
            {
                var cell = terrainGrid.Cells[0];
                var terrainMesh = cell.SurfaceMesh;

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
            }          

            return null;
        }

        private void FillRegions(MeshGrid.Cell cell, Dictionary<int, int> hashMap)
        {
            var tree = new QuadTree(cell.SurfaceMesh);
            RegionIterator iterator = new RegionIterator(cell.SurfaceMesh);
            foreach (var region in cell.Regions)
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
