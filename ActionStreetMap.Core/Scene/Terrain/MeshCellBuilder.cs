using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Utilities;

using Path = System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Triangle.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    /// <summary> Creates mesh cell from polygon data. </summary>
    internal class MeshCellBuilder
    {
        private readonly IObjectPool _objectPool;

        internal const float Scale = 1000f;
        internal const float DoubleScale = Scale*Scale;

        private float _maximumArea = 6;

        private LineGridSplitter _lineGridSplitter = new LineGridSplitter();

        /// <summary> Creates instance of <see cref="MeshCellBuilder"/>. </summary>
        /// <param name="objectPool"></param>
        public MeshCellBuilder(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        #region Public methods

        /// <summary> Builds mesh cell. </summary>
        public MeshCell Build(Rectangle2d rectangle, MeshCanvas content)
        {
            var renderMode = content.RenderMode;
            // NOTE the order of operation is important
            var water = CreateMeshRegions(rectangle, content.Water, renderMode, renderMode == RenderMode.Scene);
            var resultCarRoads = CreateMeshRegions(rectangle, content.CarRoads, renderMode);
            var resultWalkRoads = CreateMeshRegions(rectangle, content.WalkRoads, renderMode);
            var resultSurface = CreateMeshRegions(rectangle, content.Surfaces, renderMode);
            var background = CreateMeshRegions(rectangle, content.Background, renderMode);

            return new MeshCell
            {
                Water = water,
                CarRoads = resultCarRoads,
                WalkRoads = resultWalkRoads,
                Surfaces = resultSurface,
                Background = background
            };
        }

        /// <summary> Sets max area of triangle </summary>
        public void SetMaxArea(float maxArea)
        {
            _maximumArea = maxArea;
        }

        #endregion

        private List<MeshRegion> CreateMeshRegions(Rectangle2d rectangle, List<MeshCanvas.Region> regionDatas,
            RenderMode renderMode)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var regionData in regionDatas)
                meshRegions.Add(CreateMeshRegions(rectangle, regionData, renderMode));
            return meshRegions;
        }

        private MeshRegion CreateMeshRegions(Rectangle2d rectangle, MeshCanvas.Region region, 
            RenderMode renderMode, bool useContours = false)
        {
            using (var polygon = new Polygon(256, _objectPool))
            {
                var simplifiedPath = ClipByRectangle(rectangle, region.Shape);
                var contours = useContours ? new VertexPaths() : null;
                foreach (var path in simplifiedPath)
                {
                    var area = Clipper.Area(path);

                    // skip small polygons to prevent triangulation issues
                    if (Math.Abs(area/DoubleScale) < 0.001) continue;

                    var vertices = GetVertices(path, renderMode);

                    var isHole = area < 0;
                    // sign of area defines polygon orientation
                    polygon.AddContour(vertices, isHole);

                    // NOTE I don't like how this is implemented so far
                    if (useContours)
                    {
                        var contour = GetContour(rectangle, path);
                        if (isHole) contour.ForEach(c => c.Reverse());
                        contours.AddRange(contour);
                    }
                }
                var mesh = polygon.Points.Any() ? GetMesh(polygon, renderMode) : null;
                return new MeshRegion
                {
                    GradientKey = region.GradientKey,
                    ElevationNoiseFreq = region.ElevationNoiseFreq,
                    ColorNoiseFreq = region.ColorNoiseFreq,
                    ModifyMeshAction = region.ModifyMeshAction,
                    Mesh = mesh,
                    Contours = contours
                };
            }
        }

        private List<Point> GetVertices(Path path, RenderMode renderMode)
        {
            // do not split path for overview mode
            var points = _objectPool.NewList<Point>(path.Count);
            if (renderMode == RenderMode.Overview)
            {
                path.ForEach(p => points.Add(new Point(
                    Math.Round(p.X / Scale, MathUtils.RoundDigitCount),
                    Math.Round(p.Y / Scale, MathUtils.RoundDigitCount))));
                return points;
            }

            // split path for scene mode
            var lastItemIndex =  path.Count - 1;
           
            for (int i = 0; i <= lastItemIndex; i++)
            {
                var start = path[i];
                var end = path[i == lastItemIndex ? 0 : i + 1];

                _lineGridSplitter.Split(
                    new Point(Math.Round(start.X / Scale, MathUtils.RoundDigitCount),
                              Math.Round(start.Y / Scale, MathUtils.RoundDigitCount)),
                    new Point(Math.Round(end.X / Scale, MathUtils.RoundDigitCount),
                              Math.Round(end.Y / Scale, MathUtils.RoundDigitCount)),
                    _objectPool, points);
            }

            return points;
        }

        private VertexPaths GetContour(Rectangle2d rect, Path path)
        {
            ClipperOffset offset = _objectPool.NewObject<ClipperOffset>();
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
            offset.Clear();
            _objectPool.StoreObject(offset);

            var clipper = _objectPool.NewObject<Clipper>();
            clipper.AddPaths(offsetPath, PolyType.ptSubject, true);
            clipper.AddPaths(offsetRect, PolyType.ptClip, true);
            var diffSolution = new Paths();
            clipper.Execute(ClipType.ctDifference, diffSolution, PolyFillType.pftPositive, PolyFillType.pftEvenOdd);

            clipper.Clear();
            clipper.AddPaths(diffSolution, PolyType.ptSubject, true);
            clipper.AddPath(intRect, PolyType.ptClip, true);

            var solution = new Paths();
            clipper.Execute(ClipType.ctIntersection, solution);
            clipper.Clear();
            _objectPool.StoreObject(clipper);

            return solution.Select(c => c.Select(p =>
                new Vertex(Math.Round(p.X / Scale, MathUtils.RoundDigitCount),
                           Math.Round(p.Y / Scale, MathUtils.RoundDigitCount))).ToList()).ToList();
        }

        private Mesh GetMesh(Polygon polygon, RenderMode renderMode)
        {
            return renderMode == RenderMode.Overview
                ? polygon.Triangulate()
                : polygon.Triangulate(
                    new ConstraintOptions 
                    {
                        ConformingDelaunay = false, 
                        SegmentSplitting = 1
                    },
                    new QualityOptions { MaximumArea = _maximumArea });
        }

        private Paths ClipByRectangle(Rectangle2d rect, Paths subjects)
        {
            Clipper clipper = _objectPool.NewObject<Clipper>();
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
            
            clipper.Clear();
            _objectPool.StoreObject(clipper);
            return solution;
        }
    }
}