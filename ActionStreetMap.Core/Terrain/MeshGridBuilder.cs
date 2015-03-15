using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Geometry;
using ActionStreetMap.Core.Polygons.Meshing;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;

using Path = System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshGridBuilder
    {
        private const string LogTag = "mesh.tile";
        private const float Scale = 10000f;
        private readonly object _lock = new object();

        private const float MaximumArea = 10;

        private readonly ITrace _trace;

        public MeshGridBuilder(ITrace trace)
        {
            _trace = trace;
        }

        #region Grid

        public CanvasData GetCanvasData(Tile tile)
        {
            var water = BuildWater(tile);
            var roads = BuildRoads(tile, water);
            var surfaces = BuildSurfaces(tile, water, roads.Item1, roads.Item2);

            return new CanvasData
            {
                Water = water,
                CarRoads = roads.Item1,
                WalkRoads = roads.Item2,
                Surfaces = surfaces
            };
        }

        public MeshGridCell CreateCell(Rectangle rectangle, CanvasData content)
        {
            // build polygon
            var polygon = new Polygon();
            var options = new ConstraintOptions {UseRegions = true};
            var quality = new QualityOptions {MaximumArea = MaximumArea};
            polygon.AddContour(new Collection<Vertex>
            {
                new Vertex(rectangle.Left, rectangle.Bottom),
                new Vertex(rectangle.Right, rectangle.Bottom),
                new Vertex(rectangle.Right, rectangle.Top),
                new Vertex(rectangle.Left, rectangle.Top)
            });

            // NOTE the order of operation is important
            var water = CreateMeshRegions(polygon, rectangle, content.Water);
            var resultCarRoads = CreateMeshRegions(polygon, rectangle, content.CarRoads);
            var resultWalkRoads = CreateMeshRegions(polygon, rectangle, content.WalkRoads);
            var resultSurface = CreateMeshRegions(polygon, rectangle, content.Surfaces);

            Mesh mesh;
            lock (_lock)
            {
                mesh = polygon.Triangulate(options, quality);
            }
            return new MeshGridCell
            {
                Mesh = mesh,
                Water = water,
                CarRoads = resultCarRoads,
                WalkRoads = resultWalkRoads,
                Surfaces = resultSurface
            };
        }

        private MeshRegion CreateMeshRegions(Polygon polygon, Rectangle rectangle, RegionData regionData)
        {
            var fillRegions = new List<MeshFillRegion>();
            var simplifiedPath = Clipper.SimplifyPolygons(ClipByRectangle(rectangle, regionData.Shape));
            var contours = new VertexPaths(4);
            var holes = new VertexPaths(2);
            foreach (var path in simplifiedPath)
            {
                var orientation = Clipper.Orientation(path);
                var vertices = path.Select(p => new Vertex(p.X/Scale, p.Y/Scale)).ToList();
                if (orientation)
                {
                    var vertex = GetAnyPointInsidePolygon(path);
                    polygon.Regions.Add(new RegionPointer(vertex.X, vertex.Y, 0));
                    polygon.AddContour(vertices);
                    fillRegions.Add(new MeshFillRegion
                    {
                        SplatId = regionData.SplatId,
                        Anchor = vertex
                    });
                    contours.AddRange(GetContour(rectangle, path));
                }
                else
                {
                    polygon.AddContour(vertices);
                    var ggg = GetContour(rectangle, path);
                    ggg.ForEach(g => g.Reverse());
                    contours.AddRange(ggg);
                }
            }
            return new MeshRegion
            {
                Contours = contours,
                Holes = holes,
                FillRegions = fillRegions
            };
        }

        private VertexPaths GetContour(Rectangle rect, Path path)
        {
            ClipperOffset offset = new ClipperOffset();
            offset.AddPath(path, JoinType.jtMiter, EndType.etClosedLine);
            var offsetPath = new Paths();
            offset.Execute(ref offsetPath, 10);

            var intRect = new Path
            {
                new IntPoint(rect.Left*Scale, rect.Bottom*Scale),
                new IntPoint(rect.Right*Scale, rect.Bottom*Scale),
                new IntPoint(rect.Right*Scale, rect.Top*Scale),
                new IntPoint(rect.Left*Scale, rect.Top*Scale)
            };

            offset.Clear();
            offset.AddPath(intRect, JoinType.jtMiter, EndType.etClosedLine);
            var offsetRect = new Paths();
            offset.Execute(ref offsetRect, 10);

            var clipper = new Clipper();
            clipper.AddPaths(offsetPath, PolyType.ptSubject, true);
            clipper.AddPaths(offsetRect, PolyType.ptClip, true);
            var ggg = new Paths();
            clipper.Execute(ClipType.ctDifference, ggg, PolyFillType.pftPositive, PolyFillType.pftEvenOdd);

            clipper.Clear();
            clipper.AddPaths(ggg, PolyType.ptSubject, true);
            clipper.AddPath(intRect, PolyType.ptClip, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctIntersection, solution);

            return solution.Select(c => c.Select(p => new Vertex(p.X/Scale, p.Y/Scale)).ToList()).ToList();
        }

        private List<MeshRegion> CreateMeshRegions(Polygon polygon, Rectangle rectangle, List<RegionData> regionDatas)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var regionData in regionDatas)
            {
                meshRegions.Add(CreateMeshRegions(polygon, rectangle, regionData));
            }
            return meshRegions;
        }

        private Vertex GetAnyPointInsidePolygon(Path path)
        {
            if (path.Count == 3)
            {
                var p1 = path[0];
                var p2 = path[1];
                var p3 = path[2];

                var scaleDown = Scale*3f;
                return new Vertex((p1.X + p2.X + p3.X)/scaleDown, (p1.Y + p2.Y + p3.Y)/scaleDown);
            }

            // see http://stackoverflow.com/questions/9797448/get-a-point-inside-the-polygon
            // Chose first 3 consecutive points from the polygon
            // Check, if the halfway point between the first and the third point is inside the polygon
            // If yes: You found your point
            // If no: Drop first point, add next point and goto 2.
            // This is guaranteed to end, as every strictly closed polygon  has at least one triangle, 
            // that is completly part of the polygon.
            var count = path.Count;
            var circleIndex = count - 2;
            for (int i = 0; i < count; i++)
            {
                IntPoint p1 = path[i];
                IntPoint p3;
                if (i < circleIndex)
                    p3 = path[i + 2];
                else if (i == count - 2)
                    p3 = path[0];
                else
                    p3 = path[1];

                var middlePoint = new IntPoint((p1.X + p3.X)/2, (p1.Y + p3.Y)/2);
                if (Clipper.PointInPolygon(middlePoint, path) > 0)
                    return new Vertex(middlePoint.X/Scale, middlePoint.Y/Scale);
            }
            _trace.Warn(LogTag, "Cannot find point inside polygon");
            throw new AlgorithmException("Cannot find point inside polygon");
        }

        #endregion

        #region Clipper

        private static Paths ClipByTile(Tile tile, Paths subjects)
        {
            return ClipByRectangle(new Rectangle(
                tile.BottomLeft.X,
                tile.BottomLeft.Y,
                tile.Width,
                tile.Height),
                subjects);
        }

        private static Paths ClipByRectangle(Rectangle rect, Path subject)
        {
            var clipper = new Clipper();
            clipper.AddPath(subject, PolyType.ptSubject, true);
            return ClipByRectangle(rect, clipper);
        }

        private static Paths ClipByRectangle(Rectangle rect, Paths subjects)
        {
            var clipper = new Clipper();
            clipper.AddPaths(subjects, PolyType.ptSubject, true);
            return ClipByRectangle(rect, clipper);
        }

        private static Paths ClipByRectangle(Rectangle rect, Clipper clipper)
        {
            clipper.AddPath(new Path
            {
                new IntPoint(rect.Left*Scale, rect.Bottom*Scale),
                new IntPoint(rect.Right*Scale, rect.Bottom*Scale),
                new IntPoint(rect.Right*Scale, rect.Top*Scale),
                new IntPoint(rect.Left*Scale, rect.Top*Scale)
            }, PolyType.ptClip, true);
            var solution = new Paths();
            clipper.Execute(ClipType.ctIntersection, solution);
            return solution;
        }

        private static RegionData BuildWater(Tile tile)
        {
            var clipper = new Clipper();
            clipper.AddPaths(tile.Canvas.Water
                .Select(a => a.Points.Select(p => new IntPoint(p.X*Scale, p.Y*Scale)).ToList()).ToList(),
                PolyType.ptSubject, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive, PolyFillType.pftPositive);
            return new RegionData
            {
                SplatId = 0,
                Shape = ClipByTile(tile, solution)
            };
        }

        private static List<RegionData> BuildSurfaces(Tile tile, RegionData water, RegionData carRoads,
            RegionData walkRoads)
        {
            var regions = new List<RegionData>();
            foreach (var group in tile.Canvas.Areas.GroupBy(s => s.SplatIndex))
            {
                var clipper = new Clipper();
                clipper.AddPaths(group.Select(a => a.Points
                    .Select(p => new IntPoint(p.X*Scale, p.Y*Scale)).ToList()).ToList(),
                    PolyType.ptSubject, true);

                var surfacesUnion = new Paths();
                clipper.Execute(ClipType.ctUnion, surfacesUnion, PolyFillType.pftPositive, PolyFillType.pftPositive);

                clipper.Clear();
                clipper.AddPaths(carRoads.Shape, PolyType.ptClip, true);
                clipper.AddPaths(walkRoads.Shape, PolyType.ptClip, true);
                clipper.AddPaths(water.Shape, PolyType.ptClip, true);
                clipper.AddPaths(regions.SelectMany(r => r.Shape).ToList(), PolyType.ptClip, true);
                clipper.AddPaths(surfacesUnion, PolyType.ptSubject, true);
                var surfacesResult = new Paths();
                clipper.Execute(ClipType.ctDifference, surfacesResult, PolyFillType.pftPositive,
                    PolyFillType.pftPositive);
                regions.Add(new RegionData
                {
                    SplatId = group.Key,
                    Shape = ClipByTile(tile, surfacesResult)
                });
            }
            return regions;
        }

        private static Tuple<RegionData, RegionData> BuildRoads(Tile tile, RegionData water)
        {
            var carRoads = GetOffsetSolution(BuildRoadMap(tile.Canvas.Roads.Where(r => r.Type == RoadType.Car)));
            var walkRoads = GetOffsetSolution(BuildRoadMap(tile.Canvas.Roads.Where(r => r.Type == RoadType.Pedestrian)));

            var clipper = new Clipper();
            clipper.AddPaths(carRoads, PolyType.ptClip, true);
            clipper.AddPaths(walkRoads, PolyType.ptSubject, true);
            var extrudedWalkRoads = new Paths();
            clipper.Execute(ClipType.ctDifference, extrudedWalkRoads);

            return new Tuple<RegionData, RegionData>(
                CreateRoadRegionData(tile, water, carRoads),
                CreateRoadRegionData(tile, water, extrudedWalkRoads));
        }

        private static RegionData CreateRoadRegionData(Tile tile, RegionData water, Paths subject)
        {
            var clipper = new Clipper();
            var resultRoads = new Paths();
            clipper.AddPaths(water.Shape, PolyType.ptClip, true);
            clipper.AddPaths(subject, PolyType.ptSubject, true);
            clipper.Execute(ClipType.ctDifference, resultRoads, PolyFillType.pftPositive, PolyFillType.pftPositive);

            return new RegionData
            {
                SplatId = 0,
                Shape = ClipByTile(tile, resultRoads)
            };
        }

        private static Dictionary<float, Paths> BuildRoadMap(IEnumerable<RoadElement> elements)
        {
            // TODO optimize that
            var roadMap = new Dictionary<float, Paths>();
            foreach (var roadElement in elements)
            {
                var path = new Path(roadElement.Points.Count);
                var width = roadElement.Width*Scale/2;
                path.AddRange(roadElement.Points.Select(p => new IntPoint(p.X*Scale, p.Y*Scale)));
                lock (roadMap)
                {
                    if (!roadMap.ContainsKey(width))
                        roadMap.Add(width, new Paths());
                    roadMap[width].Add(path);
                }
            }
            return roadMap;
        }

        private static Paths GetOffsetSolution(Dictionary<float, Paths> roads)
        {
            var polyClipper = new Clipper();
            var offsetClipper = new ClipperOffset();
            foreach (var carRoadEntry in roads)
            {
                var offsetSolution = new Paths();
                offsetClipper.AddPaths(carRoadEntry.Value, JoinType.jtMiter, EndType.etOpenButt);
                offsetClipper.Execute(ref offsetSolution, carRoadEntry.Key);
                polyClipper.AddPaths(offsetSolution, PolyType.ptSubject, true);
                offsetClipper.Clear();
            }
            var polySolution = new Paths();
            polyClipper.Execute(ClipType.ctUnion, polySolution, PolyFillType.pftPositive, PolyFillType.pftPositive);

            return polySolution;
        }

        #endregion

        #region Nested classes

        internal class CanvasData
        {
            public RegionData Water;
            public List<RegionData> Surfaces;
            public RegionData CarRoads;
            public RegionData WalkRoads;
        }

        internal class RegionData
        {
            public int SplatId;
            public Paths Shape;
        }

        #endregion
    }
}