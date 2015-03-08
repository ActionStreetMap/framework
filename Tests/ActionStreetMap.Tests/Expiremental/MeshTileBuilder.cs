using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Geometry;
using ActionStreetMap.Core.Polygons.Meshing;
using ActionStreetMap.Core.Polygons.Meshing.Algorithm;
using ActionStreetMap.Core.Polygons.Meshing.Iterators;
using ActionStreetMap.Core.Polygons.Tools;
using ActionStreetMap.Core.Polygons.Topology;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utilities;
using UnityEngine;
using Mesh = ActionStreetMap.Core.Polygons.Mesh;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;

namespace ActionStreetMap.Tests.Expiremental
{
    public class MeshTileBuilder
    {
        public const float Scale = 1000f;

        public static void Build(Tile tile)
        {
            //clipper
            var roads = BuildRoads(tile);
            var solution = ClipByTile(tile, roads);
            SaveSvg(solution);

            // triangle
            Dictionary<int, Vertex> regionVertexMap;
            var polygon = GetPolygon(tile, solution, out regionVertexMap);
            var options = new ConstraintOptions() { UseRegions = true, ConformingDelaunay = true, Convex = true };
            var quality = new QualityOptions() { MaximumArea = 30 };
            var mesh = polygon.Triangulate(options, quality, new Incremental());

            BuildTile(mesh, regionVertexMap);

            Console.WriteLine("Done");
        }

        #region Tile

        private static void BuildTile(Mesh mesh, Dictionary<int, Vertex> regionVertexMap)
        {
            var vertices = new List<Vector3>(mesh.Vertices.Count);
            var triangles = new List<int>(mesh.Triangles.Count);

            var elevationProvider = Program._container.Resolve<IElevationProvider>();

            var hashMap = new Dictionary<int, int>();
            foreach (var triangle in mesh.Triangles)
            {
                var p0 = triangle.GetVertex(0);
                var coord0 = GeoProjection.ToGeoCoordinate(Program.StartGeoCoordinate, new MapPoint((float)p0.X, (float)p0.Y));
                var ele0 = elevationProvider.GetElevation(coord0.Latitude, coord0.Longitude);
                //ele0 += elevationNoise.GetValue((float)p0.X, ele0, (float)p0.X);
                vertices.Add(new Vector3((float)p0.X, ele0, (float)p0.Y));

                var p1 = triangle.GetVertex(1);
                var coord1 = GeoProjection.ToGeoCoordinate(Program.StartGeoCoordinate, new MapPoint((float)p1.X, (float)p1.Y));
                var ele1 = elevationProvider.GetElevation(coord1.Latitude, coord1.Longitude);
                //ele1 += elevationNoise.GetValue((float)p1.X, ele1, (float)p1.X);
                vertices.Add(new Vector3((float)p1.X, ele1, (float)p1.Y));

                var p2 = triangle.GetVertex(2);
                var coord2 = GeoProjection.ToGeoCoordinate(Program.StartGeoCoordinate, new MapPoint((float)p2.X, (float)p2.Y));
                var ele2 = elevationProvider.GetElevation(coord2.Latitude, coord2.Longitude);
                //ele2 += elevationNoise.GetValue((float) p2.X, ele2, (float) p2.X);
                vertices.Add(new Vector3((float)p2.X, ele2, (float)p2.Y));

                var index = vertices.Count;
                triangles.Add(--index);
                triangles.Add(--index);
                triangles.Add(--index);

                hashMap.Add(triangle.GetHashCode(), index);
            }

            FillRegions(mesh, regionVertexMap, hashMap);
        }

        private static void FillRegions(Mesh mesh, Dictionary<int, Vertex> regionVertexMap, Dictionary<int, int> hashMap)
        {
            var tree = new QuadTree(mesh);
            RegionIterator iterator = new RegionIterator(mesh);
            foreach (var keyValuePair in regionVertexMap)
            {
                var point = keyValuePair.Value;
                var start = (Triangle)tree.Query(point.X, point.Y);

                int count = 0;
                iterator.Process(start, triangle =>
                {
                    var index = hashMap[triangle.GetHashCode()];
                    count++;
                });
                Console.WriteLine("Region: {0}, processed: {1}", start.Region, count);
            }
        }

        #endregion

        #region Triangle

        public static Polygon GetPolygon(Tile tile, Paths roads, out Dictionary<int, Vertex> regionVertexMap)
        {
            var polygon = new Polygon();

            polygon.AddContour(new Collection<Vertex>()
            {
                new Vertex(tile.BottomLeft.X, tile.BottomLeft.Y),
                new Vertex(tile.BottomRight.X, tile.BottomRight.Y),
                new Vertex(tile.TopRight.X, tile.TopRight.Y),
                new Vertex(tile.TopLeft.X, tile.TopLeft.Y),
            });

            regionVertexMap = new Dictionary<int, Vertex>();

            int nextRegionId = 1;
            foreach (var road in roads)
            {
                var orientation = Clipper.Orientation(road);
                if (orientation)
                {
                    var vertex = GetAnyPointInsidePolygon(road);
                    polygon.Regions.Add(new RegionPointer(vertex.X, vertex.Y, nextRegionId));
                    polygon.AddContour(road.Select(p => new Vertex(p.X / Scale, p.Y / Scale)));
                    regionVertexMap.Add(nextRegionId, vertex);
                    nextRegionId++;
                }
                else
                    polygon.AddContour(road.Select(p => new Vertex(p.X / Scale, p.Y / Scale)));

            }
            return polygon;
        }

        private static Vertex GetAnyPointInsidePolygon(Path path)
        {
            // TODO Find better algorithm!

            var p = path[0];
            var delta = 1f;
            if (Clipper.PointInPolygon(new IntPoint(p.X + delta, p.Y), path) > 0)
                return new Vertex((p.X + delta) / Scale, p.Y / Scale);

            if (Clipper.PointInPolygon(new IntPoint(p.X, p.Y + delta), path) > 0)
                return new Vertex(p.X / Scale, (p.Y + delta) / Scale);

            if (Clipper.PointInPolygon(new IntPoint(p.X - delta, p.Y), path) > 0)
                return new Vertex((p.X - delta) / Scale, p.Y / Scale);

            if (Clipper.PointInPolygon(new IntPoint(p.X, p.Y - delta), path) > 0)
                return new Vertex(p.X / Scale, (p.Y - delta) / Scale);

            const double radInDegree = Math.PI / 180;
            for (int i = 0; i < 360; i += 5)
            {
                var angle = i * radInDegree;
                var x = Math.Round(p.X + delta * Math.Cos(angle), MidpointRounding.AwayFromZero);
                var y = Math.Round(p.Y + delta * Math.Sin(angle), MidpointRounding.AwayFromZero);

                if (Clipper.PointInPolygon(new IntPoint(x, y), path) > 0)
                    return new Vertex(x / Scale, y / Scale);
            }


            throw new InvalidOperationException("GetAnyPointInsidePolygon is wrong");
        }

        #endregion

        #region Clipper logic

        private static Paths ClipByTile(Tile tile, Paths subjects)
        {
            var clipper = new Clipper();
            clipper.AddPaths(subjects, PolyType.ptSubject, true);
            clipper.AddPath(new Path()
            {
                new IntPoint(tile.BottomLeft.X * Scale, tile.BottomLeft.Y * Scale),
                new IntPoint(tile.BottomRight.X * Scale, tile.BottomRight.Y * Scale),
                new IntPoint(tile.TopRight.X * Scale, tile.TopRight.Y * Scale),
                new IntPoint(tile.TopLeft.X * Scale, tile.TopLeft.Y * Scale),
            }, PolyType.ptClip, true);
            var solution = new Paths();
            clipper.Execute(ClipType.ctIntersection, solution);
            return solution;
        }

        private static Paths BuildRoads(Tile tile)
        {
            var carRoads = GetOffsetSolution(BuildRoadMap(tile.Canvas.RoadElementsTest.Where(r => r.Type == RoadType.Car)));
            var walkRoads = GetOffsetSolution(BuildRoadMap(tile.Canvas.RoadElementsTest.Where(r => r.Type == RoadType.Pedestrian)));

            var clipper = new Clipper();
            clipper.AddPaths(carRoads, PolyType.ptClip, true);
            clipper.AddPaths(walkRoads, PolyType.ptSubject, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctUnion, solution);

            return solution;
        }

        private static Dictionary<float, Paths> BuildRoadMap(IEnumerable<RoadElement> elements)
        {
            // Can be done by LINQ, but this code will be reused in production in slightly different context
            var roadMap = new Dictionary<float, Paths>();
            foreach (var roadElement in elements)
                AddRoad(roadMap, roadElement.Points, roadElement.Width * Scale / 2);
            return roadMap;
        }

        private static void AddRoad(Dictionary<float, Paths> roadMap, List<MapPoint> points, float width)
        {
            var path = new Path(points.Count);
            foreach (var p in points)
                path.Add(new IntPoint(p.X * Scale, p.Y * Scale));

            lock (roadMap)
            {
                if (!roadMap.ContainsKey(width))
                    roadMap.Add(width, new Paths());
                roadMap[width].Add(path);
            }
        }

        private static Paths GetOffsetSolution(Dictionary<float, Paths> roads)
        {
            var polyClipper = new Clipper();
            var offsetClipper = new ClipperOffset();
            foreach (var carRoadEntry in roads)
            {
                var offsetSolution = new Paths();
                offsetClipper.AddPaths(carRoadEntry.Value, JoinType.jtMiter, EndType.etOpenRound);
                offsetClipper.Execute(ref offsetSolution, carRoadEntry.Key);
                polyClipper.AddPaths(offsetSolution, PolyType.ptSubject, true);
                offsetClipper.Clear();
            }
            var polySolution = new Paths();
            polyClipper.Execute(ClipType.ctUnion, polySolution, PolyFillType.pftPositive, PolyFillType.pftPositive);

            return polySolution;
        }

        private static void SaveSvg(Paths solution)
        {
            var svgBuilder = new SVGBuilder();
            svgBuilder.AddPaths(solution);
            svgBuilder.SaveToFile("solution.svg", 0.01);
        }

        #endregion
    }
}
