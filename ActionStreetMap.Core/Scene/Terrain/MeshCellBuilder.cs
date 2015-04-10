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

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshCellBuilder
    {
        internal const float Scale = 1000f;
        internal const float DoubleScale = Scale*Scale;
        private float _maximumArea = 4;

        private readonly object _objLock = new object();

        #region Public methods

        public MeshCell Build(Rectangle rectangle, MeshCanvas content)
        {
            // NOTE the order of operation is important
            var water = CreateMeshRegions(rectangle, content.Water, false);
            var resultCarRoads = CreateMeshRegions(rectangle, content.CarRoads, false);
            var resultWalkRoads = CreateMeshRegions(rectangle, content.WalkRoads, true);
            var resultSurface = CreateMeshRegions(rectangle, content.Surfaces, false);
            var background = CreateMeshRegions(rectangle, content.Background, false);

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

        private List<MeshRegion> CreateMeshRegions(Rectangle rectangle, List<MeshCanvas.Region> regionDatas,
            bool conformingDelaunay)
        {
            var meshRegions = new List<MeshRegion>();
            foreach (var regionData in regionDatas)
                meshRegions.Add(CreateMeshRegions(rectangle, regionData, conformingDelaunay));
            return meshRegions;
        }

        private MeshRegion CreateMeshRegions(Rectangle rectangle, MeshCanvas.Region region, bool conformingDelaunay)
        {
            var polygon = new Polygon();
            var simplifiedPath = Clipper.CleanPolygons(Clipper.SimplifyPolygons(ClipByRectangle(rectangle, region.Shape)));
            var contours = new VertexPaths(4);
            foreach (var path in simplifiedPath)
            {
                var area = Clipper.Area(path);
                // skip small polygons to prevent triangulation issues
                if (Math.Abs(area/DoubleScale) < 1)
                    continue;
                var vertices = path.Select(p => new Vertex(p.X/Scale, p.Y/Scale)).ToList();

                // sign of area defines polygon orientation
                polygon.AddContour(vertices, 0, area < 0);
            }
            var mesh = polygon.Points.Any() ? GetMesh(polygon, conformingDelaunay) : null;
            return new MeshRegion
            {
                Contours = contours,
                GradientKey = region.GradientKey,
                ModifyMeshAction = region.ModifyMeshAction,
                Mesh = mesh
            };
        }

        private Mesh GetMesh(Polygon polygon, bool conformingDelaunay)
        {
            lock (_objLock)
            {
                return polygon.Triangulate(new ConstraintOptions
                {
                    ConformingDelaunay = conformingDelaunay
                },
                new QualityOptions
                {
                    MaximumArea = _maximumArea
                });
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
    }
}