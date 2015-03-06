using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Core.Tiling.Models;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;

namespace ActionStreetMap.Tests
{
    public class SvgTileBuilder
    {
        public static void Build(Tile tile)
        {
            Console.Write("Generating road image.. ");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var solution = BuildRoads(tile);

            sw.Stop();
            var svgBuilder = new SVGBuilder();
            svgBuilder.AddPaths(solution);
            svgBuilder.SaveToFile("solution.svg");
            Console.WriteLine("Took: {0}ms", sw.ElapsedMilliseconds);
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

            /*polyClipper.Clear();
            polyClipper.AddPath(new Path()
            {
                new IntPoint(-500, -500),
                new IntPoint(500, -500),
                new IntPoint(500, 500),
                new IntPoint(-500, 500)
            }, PolyType.ptClip, true);
            polyClipper.AddPaths(polySolution, PolyType.ptSubject, true);
            var ggg = new Paths();

            polyClipper.Execute(ClipType.ctIntersection, ggg);
            return ggg;*/
        }
    }
}
