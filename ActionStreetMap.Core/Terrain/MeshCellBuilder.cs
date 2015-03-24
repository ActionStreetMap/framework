using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Triangle.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Terrain
{
    internal class MeshCellBuilder
    {
        internal const float Scale = 1000f;
        internal const float DoubleScale = Scale*Scale;
        private float _maximumArea = 8;

        private readonly object _objLock = new object();

        #region Public methods

        public MeshCell Build(Rectangle rectangle, MeshCanvas content)
        {
            // NOTE the order of operation is important
            var water = CreateMeshRegions(rectangle, content.Water);
            var resultCarRoads = CreateMeshRegions(rectangle, content.CarRoads);
            var resultWalkRoads = CreateMeshRegions(rectangle, content.WalkRoads);
            var resultSurface = CreateMeshRegions(rectangle, content.Surfaces);
            var background = CreateMeshRegions(rectangle, content.Background);

            return new MeshCell
            {
                Water = water,
                CarRoads = resultCarRoads,
                WalkRoads = resultWalkRoads,
                Surfaces = resultSurface,
                Background = background
            };
        }

        public void SetMaxArea(float maxArea)
        {
            _maximumArea = maxArea;
        }

        #endregion

        private List<MeshRegion> CreateMeshRegions(Rectangle rectangle, List<MeshCanvas.Region> regionDatas)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var regionData in regionDatas)
                meshRegions.Add(CreateMeshRegions(rectangle, regionData));
            return meshRegions;
        }

        private MeshRegion CreateMeshRegions(Rectangle rectangle, MeshCanvas.Region region)
        {
            var polygon = new Polygon();
            var simplifiedPath = Clipper.CleanPolygons(Clipper.SimplifyPolygons(ClipByRectangle(rectangle, region.Shape)));
            var contours = new VertexPaths(4);
            foreach (var path in simplifiedPath)
            {
                var area = Clipper.Area(path);
                // skip small polygons to prevent triangulation issues
                if(Math.Abs(area / DoubleScale) < 1) 
                    continue;
                var vertices = path.Select(p => new Vertex(p.X/Scale, p.Y/Scale)).ToList();
                // sign of area defines polygon orientation
                if (area > 0)
                {
                    polygon.AddContour(vertices);
                    contours.AddRange(GetContour(rectangle, path));
                }
                else
                {
                    polygon.AddContour(vertices, 0, true);
                    var contour = GetContour(rectangle, path);
                    contour.ForEach(c => c.Reverse());
                    contours.AddRange(contour);
                }
            }
            var mesh = contours.Any() ? GetMesh(polygon) : null;
            return new MeshRegion
            {
                Contours = contours,
                GradientKey = region.GradientKey,
                Mesh = mesh
            };
        }

        private Mesh GetMesh(Polygon polygon)
        {
            lock (_objLock)
            {
                return polygon.Triangulate(new ConstraintOptions {UseRegions = true},
                    new QualityOptions {MaximumArea = _maximumArea});
            }
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
    }
}