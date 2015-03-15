using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Polygons.Geometry;
using ActionStreetMap.Core.Polygons.Meshing;

using Path = System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.IntPoint>>;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Polygons.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshCellBuilder
    {
        internal const float Scale = 10000f;
        private float _maximumArea = 10;

        private readonly object _lock = new object();

        #region Public methods

        public MeshCell Build(Rectangle rectangle, MeshCanvas content)
        {
            // build polygon
            var polygon = new Polygon();
            var options = new ConstraintOptions {UseRegions = true};
            var quality = new QualityOptions {MaximumArea = _maximumArea};
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
            // NOTE this operation is not thread safe
            lock (_lock)
            {
                mesh = polygon.Triangulate(options, quality);
            }

            return new MeshCell
            {
                Mesh = mesh,
                Water = water,
                CarRoads = resultCarRoads,
                WalkRoads = resultWalkRoads,
                Surfaces = resultSurface
            };
        }

        public void SetMaxArea(float maxArea)
        {
            _maximumArea = maxArea;
        }

        #endregion

        private List<MeshRegion> CreateMeshRegions(Polygon polygon, Rectangle rectangle, List<MeshCanvas.Region> regionDatas)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var regionData in regionDatas)
                meshRegions.Add(CreateMeshRegions(polygon, rectangle, regionData));
            return meshRegions;
        }

        private MeshRegion CreateMeshRegions(Polygon polygon, Rectangle rectangle, MeshCanvas.Region region)
        {
            var fillRegions = new List<MeshFillRegion>();
            var simplifiedPath = Clipper.SimplifyPolygons(ClipByRectangle(rectangle, region.Shape));
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
                        SplatId = region.SplatId,
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

        private Paths ClipByRectangle(Rectangle rect, Paths subjects)
        {
            Clipper clipper = new Clipper();
            clipper.AddPath(new Path
            {
                new IntPoint(rect.Left*Scale, rect.Bottom*Scale),
                new IntPoint(rect.Right*Scale, rect.Bottom*Scale),
                new IntPoint(rect.Right*Scale, rect.Top*Scale),
                new IntPoint(rect.Left*Scale, rect.Top*Scale)
            }, PolyType.ptClip, true);
            clipper.AddPaths(subjects, PolyType.ptSubject, true);
            var solution = new Paths();
            clipper.Execute(ClipType.ctIntersection, solution);
            return solution;
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
            var diffSolution = new Paths();
            clipper.Execute(ClipType.ctDifference, diffSolution, PolyFillType.pftPositive, PolyFillType.pftEvenOdd);

            clipper.Clear();
            clipper.AddPaths(diffSolution, PolyType.ptSubject, true);
            clipper.AddPath(intRect, PolyType.ptClip, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctIntersection, solution);

            return solution.Select(c => c.Select(p => new Vertex(p.X/Scale, p.Y/Scale)).ToList()).ToList();
        }

        private Vertex GetAnyPointInsidePolygon(Path path)
        {
            if (path.Count == 3)
            {
                var p1 = path[0];
                var p2 = path[1];
                var p3 = path[2];

                const float scaleDown = Scale*3f;
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
            throw new AlgorithmException("Cannot find point inside polygon");
        }
    }
}