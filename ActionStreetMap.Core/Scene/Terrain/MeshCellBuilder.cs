using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using ActionStreetMap.Infrastructure.Utilities;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;
using VertexPaths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Triangle.Geometry.Vertex>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshCellBuilder
    {
        private readonly IObjectPool _objectPool;

        internal const float Scale = 1000f;
        internal const float DoubleScale = Scale*Scale;
        internal const int RoundDigitCount = 5;

        private float _maximumArea = 4;

        public MeshCellBuilder(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        #region Public methods

        public MeshCell Build(MapRectangle rectangle, MeshCanvas content)
        {
            var renderMode = content.RenderMode;
            // NOTE the order of operation is important
            var water = CreateMeshRegions(rectangle, content.Water, renderMode, false, renderMode == RenderMode.Scene);
            var resultCarRoads = CreateMeshRegions(rectangle, content.CarRoads, renderMode, false);
            var resultWalkRoads = CreateMeshRegions(rectangle, content.WalkRoads, renderMode, true);
            var resultSurface = CreateMeshRegions(rectangle, content.Surfaces, renderMode, false);
            var background = CreateMeshRegions(rectangle, content.Background, renderMode, false);

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

        private List<MeshRegion> CreateMeshRegions(MapRectangle rectangle, List<MeshCanvas.Region> regionDatas,
            RenderMode renderMode, bool conformingDelaunay)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var regionData in regionDatas)
                meshRegions.Add(CreateMeshRegions(rectangle, regionData, renderMode, conformingDelaunay));
            return meshRegions;
        }

        private MeshRegion CreateMeshRegions(MapRectangle rectangle, MeshCanvas.Region region, RenderMode renderMode,
            bool conformingDelaunay, bool useContours = false)
        {
            var polygon = new Polygon();
            var simplifiedPath = ClipByRectangle(rectangle, region.Shape);
            var contours = useContours ? new VertexPaths(): null;
            foreach (var path in simplifiedPath)
            {
                var area = Clipper.Area(path);

                // skip small polygons to prevent triangulation issues
                if (Math.Abs(area/DoubleScale) < 0.001) continue;

                var vertices = GetVertices(path, renderMode);

                var isHole = area < 0;
                // sign of area defines polygon orientation
                polygon.AddContour(vertices, 0, isHole);

                // NOTE I don't like how this is implemented so far
                if (useContours)
                {
                    var contour = GetContour(rectangle, path);
                    if (isHole) contour.ForEach(c => c.Reverse());
                    contours.AddRange(contour);
                }
            }
            var mesh = polygon.Points.Any() ? GetMesh(polygon, renderMode, conformingDelaunay) : null;
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

        private void DumpRegion(Paths simplifiedPath)
        {
            var writer = new StreamWriter(File.Create(@"F:\gg.txt"));
            foreach (var shape in simplifiedPath)
            {
                writer.WriteLine(shape.Count);
                for(int i=0; i < shape.Count;i++)
                    writer.WriteLine("{0} {1}", shape[i].X, shape[i].Y);
            }
            writer.Close();
        }

        private List<Point> GetVertices(Path path, RenderMode renderMode)
        {
            var points = _objectPool.NewList<Point>(path.Count);
            if (renderMode == RenderMode.Overview)
            {
                path.ForEach(p => points.Add(new Point(
                    Math.Round(p.X / Scale, RoundDigitCount), 
                    Math.Round(p.Y / Scale, RoundDigitCount))));
                return points;
            }

            var lastItemIndex =  path.Count - 1;
            var lineGridSplitter = new LineGridSplitter(1, RoundDigitCount);
            
            for (int i = 0; i < lastItemIndex; i++)
            {
                var start = path[i];
                var end = path[i == lastItemIndex ? 0 : i + 1];

                lineGridSplitter.Split(
                    new Point(Math.Round(start.X / Scale, RoundDigitCount),
                              Math.Round(start.Y / Scale, RoundDigitCount)),
                    new Point(Math.Round(end.X / Scale, RoundDigitCount), 
                              Math.Round(end.Y / Scale, RoundDigitCount)),
                    _objectPool, points);
            }

            // NOTE last is not handled by splitter
            var last = path[path.Count - 1];
            points.Add(new Point(Math.Round(last.X / Scale, RoundDigitCount),
                                 Math.Round(last.Y / Scale, RoundDigitCount)));

            return points;
        }

        private VertexPaths GetContour(MapRectangle rect, Path path)
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
                new Vertex(Math.Round(p.X / Scale, RoundDigitCount), 
                           Math.Round(p.Y / Scale, RoundDigitCount))).ToList()).ToList();
        }


        private Mesh GetMesh(Polygon polygon, RenderMode renderMode, bool conformingDelaunay)
        {
            return renderMode == RenderMode.Overview
                ? (Mesh) polygon.Triangulate()
                : (Mesh) polygon.Triangulate(
                    new ConstraintOptions 
                    {
                        ConformingDelaunay = conformingDelaunay, 
                        SegmentSplitting = 1
                    },
                    new QualityOptions {MaximumArea = _maximumArea});
        }

        private Paths ClipByRectangle(MapRectangle rect, Paths subjects)
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