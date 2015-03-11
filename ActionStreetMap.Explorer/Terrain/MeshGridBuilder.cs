using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Geometry;
using ActionStreetMap.Core.Polygons.Meshing;
using ActionStreetMap.Core.Polygons.Meshing.Algorithm;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;

namespace ActionStreetMap.Explorer.Terrain
{
    internal class MeshGridBuilder
    {
        private const string LogTag = "mesh.tile";
        private const float Scale = 1000f;

        // TODO make configurable
        private const float MaxCellSize = 100;
        private const float MaxArea = 30;

        [Dependency]
        public ITrace Trace { get; set; }

        public MeshGrid Build(Tile tile)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            //clipper
            var waters = BuildWaters(tile);
            var roads = BuildRoads(tile);
            var areas = BuildAreas(tile);

            var solution = GetSolution(waters, roads, areas);
            var grid = CreateGrid(solution);

            sw.Stop();
            Trace.Debug(LogTag, "Took: {0}ms", sw.ElapsedMilliseconds);

            return grid;
        }

        #region Grid

        private MeshGrid CreateGrid(Tile tile, Paths solution)
        {
            var cellRowCount = Math.Ceiling(tile.Height / MaxCellSize);
            var cellColumnCount = Math.Ceiling(tile.Width / MaxCellSize);
            var cellHeight = tile.Height / cellRowCount;
            var cellWidth = tile.Width / cellColumnCount;

            MeshGrid.Cell[,] cells = new MeshGrid.Cell[cellRowCount, cellColumnCount];

            for(int j = 0; j < cellRowCount; j++)
                for (int i = 0; i < cellColumnCount; i++)
                {
                    var point = new MapPoint(
                        tile.BottomLeft.X + i * cellWidth, 
                        tile.BottomLeft.Y + j * cellHeight);

                    cells[j, i] = CreateCell(point, cellHeight, cellWidth)
                }
            return new MeshGrid() 
            {
                Cells = cells
            };
        }

        private MeshGrid.Cell CreateCell(MapPoint leftBottom, float height, float width, Paths solution)
        {
            // triangle
            var meshRegions = new List<MeshRegion>();

             var polygon = new Polygon();
            polygon.AddContour(new Collection<Vertex>
            {
                new Vertex(leftBottom.X, tile.leftBottom.Y),
                new Vertex(leftBottom.X + width, leftBottom.Y),
                new Vertex(leftBottom.X + width, leftBottom.Y + height),
                new Vertex(leftBottom.X, leftBottom.Y + height)
            });

            AddRegions(polygon, roads, meshRegions, null);

            var options = new ConstraintOptions { UseRegions = true };
            var quality = new QualityOptions { MaximumArea = 30 };

            var mesh = polygon.Triangulate(options, quality, new Incremental());

            return new MeshGrid.Cell()
            {
                Mesh = mesh,
                Regions = meshRegions
            };
        }

        private static void AddRegions(Polygon polygon, Paths paths, List<MeshRegion> meshRegions, 
            IMeshRegionVisitor visitor)
        {
            foreach (var path in Clipper.SimplifyPolygons(paths))
            {
                var orientation = Clipper.Orientation(path);
                if (orientation)
                {
                    var vertex = GetAnyPointInsidePolygon(path);
                    polygon.Regions.Add(new RegionPointer(vertex.X, vertex.Y, 0));
                    polygon.AddContour(path.Select(p => new Vertex(p.X / Scale, p.Y / Scale)));
                    meshRegions.Add(new MeshRegion()
                    {
                        Visitor = visitor,
                        Anchor = vertex
                    });
                }
                else
                    polygon.AddContour(path.Select(p => new Vertex(p.X / Scale, p.Y / Scale)));
            }
        }

        private static Vertex GetAnyPointInsidePolygon(Path path)
        {
            // TODO Find better algorithm!
            var p = path[0];
            var delta = 1f;
            if (Clipper.PointInPolygon(new IntPoint(p.X + delta, p.Y), path) > 0)
                return new Vertex((p.X + delta)/Scale, p.Y/Scale);

            if (Clipper.PointInPolygon(new IntPoint(p.X, p.Y + delta), path) > 0)
                return new Vertex(p.X/Scale, (p.Y + delta)/Scale);

            if (Clipper.PointInPolygon(new IntPoint(p.X - delta, p.Y), path) > 0)
                return new Vertex((p.X - delta)/Scale, p.Y/Scale);

            if (Clipper.PointInPolygon(new IntPoint(p.X, p.Y - delta), path) > 0)
                return new Vertex(p.X/Scale, (p.Y - delta)/Scale);

            IntRect intRect = new IntRect();
            for (int index = 0; index < path.Count; ++index)
            {
                if (path[index].X < intRect.left)
                    intRect.left = path[index].X;
                else if (path[index].X > intRect.right)
                    intRect.right = path[index].X;
                if (path[index].Y < intRect.top)
                    intRect.top = path[index].Y;
                else if (path[index].Y > intRect.bottom)
                    intRect.bottom = path[index].Y;
            }

            while (true)
            {
                var x = UnityEngine.Random.Range(intRect.left, intRect.right);
                var y = UnityEngine.Random.Range(intRect.bottom, intRect.top);
                if (Clipper.PointInPolygon(new IntPoint(x, y), path) > 0)
                    return new Vertex(x/Scale, y/Scale);
            }
        }

        #endregion

        #region Clipper

        private static Paths GetSolution(Paths waters, Paths roads, Paths areas)
        {
            var solution = new Paths();
            Clipper clipper = new Clipper();
            clipper.AddPaths(roads, PolyType.ptClip, true);
            clipper.AddPaths(areas, PolyType.ptSubject, true);
            clipper.Execute(ClipType.ctDifference, solution);

            return solution;
        }

        private static Paths ClipByTile(Tile tile, Paths subjects)
        {
            var clipper = new Clipper();
            clipper.AddPaths(subjects, PolyType.ptSubject, true);
            clipper.AddPath(new Path
            {
                new IntPoint(tile.BottomLeft.X*Scale, tile.BottomLeft.Y*Scale),
                new IntPoint(tile.BottomRight.X*Scale, tile.BottomRight.Y*Scale),
                new IntPoint(tile.TopRight.X*Scale, tile.TopRight.Y*Scale),
                new IntPoint(tile.TopLeft.X*Scale, tile.TopLeft.Y*Scale)
            }, PolyType.ptClip, true);
            var solution = new Paths();
            clipper.Execute(ClipType.ctIntersection, solution);
            return solution;
        }

        private static Paths BuildWaters(Tile tile)
        {
            var clipper = new Clipper();
            clipper.AddPaths(tile.Canvas.Waters
                .Select(a => a.Points.Select(p => new IntPoint(p.X * Scale, p.Y * Scale)).ToList()).ToList(),
                PolyType.ptSubject, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctUnion, solution);
            return ClipByTile(tile, solution);
        }

        private static Paths BuildAreas(Tile tile)
        {
            var clipper = new Clipper();
            clipper.AddPaths(tile.Canvas.Areas
                .Select(a => a.Points.Select(p => new IntPoint(p.X*Scale, p.Y*Scale)).ToList()).ToList(),
                PolyType.ptSubject, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctUnion, solution);
            return ClipByTile(tile, solution);
        }

        private static Paths BuildRoads(Tile tile)
        {
            var carRoads = GetOffsetSolution(BuildRoadMap(tile.Canvas.Roads.Where(r => r.Type == RoadType.Car)));
            var walkRoads =
                GetOffsetSolution(BuildRoadMap(tile.Canvas.Roads.Where(r => r.Type == RoadType.Pedestrian)));

            var clipper = new Clipper();
            clipper.AddPaths(carRoads, PolyType.ptClip, true);
            clipper.AddPaths(walkRoads, PolyType.ptSubject, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctUnion, solution);

            return ClipByTile(tile, solution);
        }

        private static Dictionary<float, Paths> BuildRoadMap(IEnumerable<RoadElement> elements)
        {
            // Can be done by LINQ, but this code will be reused in production in slightly different context
            var roadMap = new Dictionary<float, Paths>();
            foreach (var roadElement in elements)
                AddRoad(roadMap, roadElement.Points, roadElement.Width*Scale/2);
            return roadMap;
        }

        private static void AddRoad(Dictionary<float, Paths> roadMap, List<MapPoint> points, float width)
        {
            var path = new Path(points.Count);
            foreach (var p in points)
                path.Add(new IntPoint(p.X*Scale, p.Y*Scale));

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

        #endregion
    }
}