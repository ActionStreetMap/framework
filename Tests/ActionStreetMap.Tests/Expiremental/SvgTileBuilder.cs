using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Scene.Details;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Core.Tiling.Models;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;

namespace ActionStreetMap.Tests.Expiremental
{
    public class SvgTileBuilder
    {
        public static void Build(Tile tile)
        {
            Console.Write("Generating road image.. ");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var roads = BuildRoads(tile);

            var solution = ClipByTile(tile, roads);
            /*var areas = BuildSurfaces(tile.Canvas.AreasTest);

            // this will give areas split by roads

            var clippedAreas = new Paths();
            var clipper = new Clipper();
            clipper.AddPaths(areas, PolyType.ptSubject, true);
            clipper.AddPath(new Path()
            {
                new IntPoint(0, 0),
                new IntPoint(500, 0),
                new IntPoint(500, 500),
                new IntPoint(0, 500),
            }, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, clippedAreas);
            clipper.Clear();

            var solution = new Paths();
            //var clipper = new Clipper();
            clipper.AddPaths(clippedAreas, PolyType.ptSubject, true);
            clipper.AddPaths(roads, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctDifference, solution);*/

            sw.Stop();
            var svgBuilder = new SVGBuilder();
            svgBuilder.AddPaths(solution);
            svgBuilder.SaveToFile("solution.svg");
            Console.WriteLine("Size:{0}, took: {1}ms", solution.Count, sw.ElapsedMilliseconds);
        }

        private static Paths ClipByTile(Tile tile, Paths subjects)
        {
            var clipper = new Clipper();
            clipper.AddPaths(subjects, PolyType.ptSubject, true);
            clipper.AddPath(new Path()
            {
                new IntPoint(tile.BottomLeft.X, tile.BottomLeft.Y),
                new IntPoint(tile.BottomRight.X, tile.BottomRight.Y),
                new IntPoint(tile.TopRight.X, tile.TopRight.Y),
                new IntPoint(tile.TopLeft.X, tile.TopLeft.Y),
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

        public static Paths BuildSurfaces(List<Surface> surfaces)
        {  
            Clipper clipper = new Clipper();
            foreach (var group in surfaces.GroupBy(s => s.SplatIndex))
            {
                foreach (var surface in group)
                    clipper.AddPath(surface.Points
                        .Select(p => new IntPoint(p.X, p.Y)).ToList(), PolyType.ptSubject, true);
                break;
            }
            var solution = new Paths();
            clipper.Execute(ClipType.ctUnion, solution);

            return solution;
        }

        private static Dictionary<float, Paths> BuildRoadMap(IEnumerable<RoadElement> elements)
        {
            // Can be done by LINQ, but this code will be reused in production in slightly different context
            var roadMap = new Dictionary<float, Paths>();
            foreach (var roadElement in elements)
                AddRoad(roadMap, roadElement.Points, roadElement.Width/2);
            return roadMap;
        }

        private static void AddRoad(Dictionary<float, Paths> roadMap, List<MapPoint> points, float width)
        {
            var path = new Path(points.Count);
            foreach (var p in points)
                path.Add(new IntPoint(p.X, p.Y));

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
    }
}
