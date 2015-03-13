using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Geometry;
using ActionStreetMap.Core.Polygons.Meshing;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshGridBuilder
    {
        private const string LogTag = "mesh.tile";
        private const float Scale = 10000f;

        // TODO make configurable
        private const float MaxCellSize = 100;
        private const float MaximumArea = 30;

        [Dependency]
        public ITrace Trace { get; set; }

        public MeshGridCell[,] Build(Tile tile)
        {
            // get original objects in tile
            var contentData = GetCanvasData(tile);

            // detect grid parameters
            var cellRowCount = (int) Math.Ceiling(tile.Height/MaxCellSize);
            var cellColumnCount = (int) Math.Ceiling(tile.Width/MaxCellSize);
            var cellHeight = tile.Height/cellRowCount;
            var cellWidth = tile.Width/cellColumnCount;

            var cells = new MeshGridCell[cellRowCount, cellColumnCount];
            for (int j = 0; j < cellRowCount; j++)
                for (int i = 0; i < cellColumnCount; i++)
                {
                    var rectangle = new Rectangle(
                        tile.BottomLeft.X + i*cellWidth,
                        tile.BottomLeft.Y + j*cellHeight,
                        cellWidth,
                        cellHeight);

                    cells[j, i] = CreateCell(rectangle, contentData);
                }

            return cells;
        }

        #region Grid

        private CanvasData GetCanvasData(Tile tile)
        {
            var water = BuildWater(tile);
            var roads = BuildRoads(tile, water);
            var surfaces = BuildSurfaces(tile, water, roads);

            return new CanvasData
            {
                Water = water,
                Roads = roads,
                Surfaces = surfaces
            };
        }

        private MeshGridCell CreateCell(Rectangle rectangle, CanvasData content)
        {
            // build polygon
            var polygon = new Polygon();
            var options = new ConstraintOptions { UseRegions = true };
            var quality = new QualityOptions { MaximumArea = MaximumArea };
            polygon.AddContour(new Collection<Vertex>
            {
                new Vertex(rectangle.Left, rectangle.Bottom),
                new Vertex(rectangle.Right, rectangle.Bottom),
                new Vertex(rectangle.Right, rectangle.Top),
                new Vertex(rectangle.Left, rectangle.Top)
            });

            // NOTE the order of operation is important
            var resultRoads = CreateMeshRegions(polygon, rectangle, content.Roads);
            var resultSurface = CreateMeshRegions(polygon, rectangle, content.Surfaces);

            var mesh = polygon.Triangulate(options, quality);
            return new MeshGridCell
            {
                Mesh = mesh,
                Roads = resultRoads,
                Surfaces = resultSurface,
            };
        }

        private List<MeshRegion> CreateMeshRegions(Polygon polygon, Rectangle rectangle, RegionData regionData)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var path in Clipper.SimplifyPolygons(ClipByRectangle(rectangle, regionData.Shape)))
            {
                var orientation = Clipper.Orientation(path);
                if (orientation)
                {
                    var vertex = GetAnyPointInsidePolygon(path);
                    polygon.Regions.Add(new RegionPointer(vertex.X, vertex.Y, 0));
                    polygon.AddContour(path.Select(p => new Vertex(p.X / Scale, p.Y / Scale)));
                    meshRegions.Add(new MeshRegion()
                    {
                        SplatId = regionData.SplatId,
                        Anchor = vertex
                    });
                }
                else
                    polygon.AddContour(path.Select(p => new Vertex(p.X / Scale, p.Y / Scale)));
            }
            return meshRegions;
        }

        private List<MeshRegion> CreateMeshRegions(Polygon polygon, Rectangle rectangle, List<RegionData> regionDatas)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var regionData in regionDatas)
            {
                meshRegions.AddRange(CreateMeshRegions(polygon, rectangle, regionData));
            }
            return meshRegions;
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

            var intRect = new IntRect();
            for (int index = 0; index < path.Count; ++index)
            {
                if (path[index].X < intRect.left)
                    intRect.left = path[index].X;
                else if (path[index].X > intRect.right)
                    intRect.right = path[index].X;
                // NOTE clipper uses inverted y-axis
                if (path[index].Y < intRect.top)
                    intRect.top = path[index].Y;
                else if (path[index].Y > intRect.bottom)
                    intRect.bottom = path[index].Y;
            }

            var random = new Random();
            while (true)
            {
                var x = RandomUtils.LongRandom(intRect.left, intRect.right, random);
                var y = RandomUtils.LongRandom(intRect.top, intRect.bottom, random);
                if (Clipper.PointInPolygon(new IntPoint(x, y), path) > 0)
                    return new Vertex(x/Scale, y/Scale);
            }
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

        private static Paths ClipByRectangle(Rectangle rect, Paths subjects)
        {
            var clipper = new Clipper();
            clipper.AddPaths(subjects, PolyType.ptSubject, true);
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

        private static List<RegionData> BuildSurfaces(Tile tile, RegionData waters, RegionData roads)
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
                clipper.AddPaths(roads.Shape, PolyType.ptClip, true);
                clipper.AddPaths(regions.SelectMany(r => r.Shape).ToList(), PolyType.ptClip, true);
                clipper.AddPaths(surfacesUnion, PolyType.ptSubject, true);
                var surfacesResult = new Paths();
                clipper.Execute(ClipType.ctDifference, surfacesResult, PolyFillType.pftPositive, PolyFillType.pftPositive);
                regions.Add(new RegionData()
                {
                    SplatId = group.Key,
                    Shape = ClipByTile(tile, surfacesResult)
                });
            }
            return regions;
        }

        private static RegionData BuildRoads(Tile tile, RegionData water)
        {
            var carRoads = GetOffsetSolution(BuildRoadMap(
                tile.Canvas.Roads.Where(r => r.Type == RoadType.Car)));

            var walkRoads = GetOffsetSolution(BuildRoadMap(
                tile.Canvas.Roads.Where(r => r.Type == RoadType.Pedestrian)));

            var clipper = new Clipper();
            clipper.AddPaths(carRoads, PolyType.ptClip, true);
            clipper.AddPaths(walkRoads, PolyType.ptSubject, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive, PolyFillType.pftPositive);

            return new RegionData 
            {
                SplatId = 0,
                Shape = ClipByTile(tile, solution)
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

        #region Nested classes

        private class CanvasData
        {
            public RegionData Water;
            public List<RegionData> Surfaces;
            public RegionData Roads;
        }

        private class RegionData
        {
            public int SplatId;
            public Paths Shape;
        }

        #endregion
    }
}