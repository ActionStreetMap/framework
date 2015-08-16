using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Utilities;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;

namespace ActionStreetMap.Core.Scene.Terrain
{
    internal class MeshCanvasBuilder
    {
        private readonly IObjectPool _objectPool;
        private readonly Clipper _clipper;
        private readonly ClipperOffset _offset;

        private Tile _tile;
        private float _scale;

        private MeshCanvas.Region _background;
        private MeshCanvas.Region _water;
        private MeshCanvas.Region _carRoads;
        private MeshCanvas.Region _walkRoads;
        private List<MeshCanvas.Region> _surfaces;

        public MeshCanvasBuilder(IObjectPool objectPool)
        {
            _objectPool = objectPool;
            _clipper = objectPool.NewObject<Clipper>();
            _offset = objectPool.NewObject<ClipperOffset>();
        }

        public MeshCanvasBuilder SetScale(float scale)
        {
            _scale = scale;
            return this;
        }

        public MeshCanvasBuilder SetTile(Tile tile)
        {
            _tile = tile;
            return this;
        }

        public MeshCanvas Build(RenderMode mode)
        {
            CheckState();

            var rect = _tile.Rectangle;
            var clipRect = new Path
            {
                new IntPoint(rect.Left*_scale, rect.Bottom*_scale),
                new IntPoint(rect.Right*_scale, rect.Bottom*_scale),
                new IntPoint(rect.Right*_scale, rect.Top*_scale),
                new IntPoint(rect.Left*_scale, rect.Top*_scale)
            };

            BuildWater(clipRect);
            BuildRoads(clipRect);
            BuildSurfaces(clipRect);
            BuildBackground(clipRect);

            Cleanup();

            return new MeshCanvas
            {
                RenderMode = mode,
                Rect = _tile.Rectangle,
                Background = _background,
                Water = _water,
                CarRoads = _carRoads,
                WalkRoads = _walkRoads,
                Surfaces = _surfaces
            };
        }

        #region Background

        private void BuildBackground(Path clipRect)
        {
            _clipper.AddPath(clipRect, PolyType.ptSubject, true);

            _clipper.AddPaths(_carRoads.Shape, PolyType.ptClip, true);
            _clipper.AddPaths(_walkRoads.Shape, PolyType.ptClip, true);
            _clipper.AddPaths(_water.Shape, PolyType.ptClip, true);
            _clipper.AddPaths(_surfaces.SelectMany(r => r.Shape), PolyType.ptClip, true);
            var solution = new Paths();
            _clipper.Execute(ClipType.ctDifference, solution, PolyFillType.pftPositive,
                PolyFillType.pftPositive);
            _clipper.Clear();

            _background = new MeshCanvas.Region() { Shape = solution };
        }

        #endregion

        #region Water

        private void BuildWater(Path clipRect)
        {
            _clipper.AddPaths(_tile.Canvas.Water.Select(a => a.Points),
                PolyType.ptSubject, _scale, true);
            var solution = new Paths();
            _clipper.Execute(ClipType.ctUnion, solution);
            _clipper.Clear();
            _water = new MeshCanvas.Region { Shape = ClipByRectangle(clipRect, solution) };
        }

        #endregion

        #region Surfaces

        private void BuildSurfaces(Path clipRect)
        {
            var regions = new List<MeshCanvas.Region>();
            foreach (var group in _tile.Canvas.Surfaces.GroupBy(s => s.Item1.GradientKey))
            {
                var paths = group.Select(a => a.Item1.Points);
                _clipper.AddPaths(paths, PolyType.ptSubject, _scale, true);

                var surfacesUnion = new Paths();
                _clipper.Execute(ClipType.ctUnion, surfacesUnion);

                _clipper.Clear();
                _clipper.AddPaths(_carRoads.Shape, PolyType.ptClip, true);
                _clipper.AddPaths(_walkRoads.Shape, PolyType.ptClip, true);
                _clipper.AddPaths(_water.Shape, PolyType.ptClip, true);
                _clipper.AddPaths(regions.SelectMany(r => r.Shape).ToList(), PolyType.ptClip, true);
                _clipper.AddPaths(surfacesUnion, PolyType.ptSubject, true);
                var surfacesResult = new Paths();
                _clipper.Execute(ClipType.ctDifference, surfacesResult, PolyFillType.pftPositive,
                    PolyFillType.pftPositive);
                _clipper.Clear();

                // NOTE we use values from first item of group
                var first = group.First();

                regions.Add(new MeshCanvas.Region
                {
                    GradientKey = group.Key,
                    ElevationNoiseFreq = first.Item1.ElevationNoise,
                    ColorNoiseFreq = first.Item1.ColorNoise,
                    ModifyMeshAction = first.Item2,

                    Shape = ClipByRectangle(clipRect, surfacesResult)
                });
            }
            _surfaces = regions;
        }

        #endregion

        #region Roads

        private void BuildRoads(Path clipRect)
        {
            var carRoadPaths = GetOffsetSolution(
                BuildRoadMap(_tile.Canvas.Roads.Where(r => r.Type == RoadElement.RoadType.Car)));

            var walkRoadsPaths = GetOffsetSolution(
                BuildRoadMap(_tile.Canvas.Roads.Where(r => r.Type == RoadElement.RoadType.Pedestrian)));

            _clipper.AddPaths(carRoadPaths, PolyType.ptClip, true);
            _clipper.AddPaths(walkRoadsPaths, PolyType.ptSubject, true);
            var extrudedWalkRoads = new Paths();
            _clipper.Execute(ClipType.ctDifference, extrudedWalkRoads);
            _clipper.Clear();
            _carRoads = CreateRoadRegionData(clipRect, carRoadPaths);
            _walkRoads = CreateRoadRegionData(clipRect, extrudedWalkRoads);
        }

        private MeshCanvas.Region CreateRoadRegionData(Path clipRect, Paths subject)
        {
            var resultRoads = new Paths();
            _clipper.AddPaths(_water.Shape, PolyType.ptClip, true);
            _clipper.AddPaths(subject, PolyType.ptSubject, true);
            _clipper.Execute(ClipType.ctDifference, resultRoads, PolyFillType.pftPositive,
                PolyFillType.pftPositive);
            _clipper.Clear();
            return new MeshCanvas.Region { Shape = ClipByRectangle(clipRect, resultRoads) };
        }

        private Dictionary<float, Paths> BuildRoadMap(IEnumerable<RoadElement> elements)
        {
            var roadMap = new Dictionary<float, Paths>();
            foreach (var roadElement in elements)
            {
                var path = new Path(roadElement.Points.Count);
                var width = roadElement.Width * _scale;
                path.AddRange(roadElement.Points.Select(p => new IntPoint(p.X * _scale, p.Y * _scale)));
                if (!roadMap.ContainsKey(width))
                    roadMap.Add(width, new Paths());
                roadMap[width].Add(path);
            }
            return roadMap;
        }

        private Paths GetOffsetSolution(Dictionary<float, Paths> roads)
        {
            foreach (var carRoadEntry in roads)
            {
                var offsetSolution = new Paths();
                _offset.AddPaths(carRoadEntry.Value, JoinType.jtMiter, EndType.etOpenSquare);
                _offset.Execute(ref offsetSolution, carRoadEntry.Key);
                _clipper.AddPaths(offsetSolution, PolyType.ptSubject, true);
                _offset.Clear();
            }
            var polySolution = new Paths();
            _clipper.Execute(ClipType.ctUnion, polySolution, PolyFillType.pftPositive, PolyFillType.pftPositive);
            _clipper.Clear();
            return polySolution;
        }

        #endregion

        private void CheckState()
        {
            if (_tile == null)
                throw new InvalidOperationException("Tile is not set");
            if (_scale == 0)
                throw new InvalidOperationException("Scale is not set");
        }

        private Paths ClipByRectangle(Path clipRect, Paths subjects)
        {
            _clipper.AddPaths(subjects, PolyType.ptSubject, true);
            _clipper.AddPath(clipRect, PolyType.ptClip, true);
            var solution = new Paths();
            _clipper.Execute(ClipType.ctIntersection, solution);
            _clipper.Clear();
            return solution;
        }

        private void Cleanup()
        {
            _clipper.Clear();
            _offset.Clear();
            _objectPool.StoreObject(_clipper);
            _objectPool.StoreObject(_offset);
        }
    }
}