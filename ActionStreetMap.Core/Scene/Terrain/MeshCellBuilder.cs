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
        internal const float DoubleScale = Scale * Scale;

        private readonly LineGridSplitter _lineGridSplitter = new LineGridSplitter();
        private float _maximumArea = 6;

        /// <summary> Creates instance of <see cref="MeshCellBuilder"/>. </summary>
        /// <param name="objectPool"></param>
        public MeshCellBuilder(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        #region Public methods

        /// <summary> Builds mesh cell. </summary>
        public MeshCell Build(MeshCanvas content, Rectangle2d rectangle)
        {
            var renderMode = content.RenderMode;

            var clipRect = new Path
            {
                new IntPoint(rectangle.Left*Scale, rectangle.Bottom*Scale),
                new IntPoint(rectangle.Right*Scale, rectangle.Bottom*Scale),
                new IntPoint(rectangle.Right*Scale, rectangle.Top*Scale),
                new IntPoint(rectangle.Left*Scale, rectangle.Top*Scale)
            };

            var useContours = renderMode == RenderMode.Scene;

            // NOTE the order of operation is important
            var water = CreateMeshRegions(clipRect, content.Water, renderMode, useContours);
            var resultCarRoads = CreateMeshRegions(clipRect, content.CarRoads, renderMode, useContours);
            var resultWalkRoads = CreateMeshRegions(clipRect, content.WalkRoads, renderMode);
            var resultSurface = CreateMeshRegions(clipRect, content.Surfaces, renderMode);
            var background = CreateMeshRegions(clipRect, content.Background, renderMode);

            return new MeshCell
            {
                Water = water,
                CarRoads = resultCarRoads,
                WalkRoads = resultWalkRoads,
                Surfaces = resultSurface,
                Background = background,
                Rectangle = rectangle
            };
        }

        /// <summary> Sets max area of triangle </summary>
        public void SetMaxArea(float maxArea)
        {
            _maximumArea = maxArea;
        }

        #endregion

        private List<MeshRegion> CreateMeshRegions(Path clipRect, List<MeshCanvas.Region> regionDatas,
            RenderMode renderMode)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var regionData in regionDatas)
                meshRegions.Add(CreateMeshRegions(clipRect, regionData, renderMode));
            return meshRegions;
        }

        private MeshRegion CreateMeshRegions(Path clipRect, MeshCanvas.Region region,
            RenderMode renderMode, bool useContours = false)
        {
            using (var polygon = new Polygon(256, _objectPool))
            {
                var simplifiedPath = ClipByRectangle(clipRect, region.Shape);
                var contours = new List<List<Point>>(useContours ? simplifiedPath.Count : 0);
                foreach (var path in simplifiedPath)
                {
                    var area = Clipper.Area(path);

                    // skip small polygons to prevent triangulation issues
                    if (Math.Abs(area / DoubleScale) < 0.001) continue;

                    var vertices = GetVertices(path, renderMode);

                    // sign of area defines polygon orientation
                    polygon.AddContour(vertices, area < 0);

                    if (useContours)
                        contours.Add(vertices);
                }

                contours.ForEach(c => c.Reverse());

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
            var lastItemIndex = path.Count - 1;

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

        private Paths ClipByRectangle(Path clipRect, Paths subjects)
        {
            Clipper clipper = _objectPool.NewObject<Clipper>();
            clipper.AddPath(clipRect, PolyType.ptClip, true);
            clipper.AddPaths(subjects, PolyType.ptSubject, true);
            var solution = new Paths();
            clipper.Execute(ClipType.ctIntersection, solution);

            clipper.Clear();
            _objectPool.StoreObject(clipper);
            return solution;
        }
    }
}