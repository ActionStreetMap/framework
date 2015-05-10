using System;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;
using Random = System.Random;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal abstract class SideBuilder
    {
        private float _positionNoiseFreq = 0.05f;
        private float _colorNoiseFreq = 0.2f;
        private float _firstFloorHeight = 4;
        private int _floorCount = 3;
        private float _floorHeight = 3f;
        private float _entranceWidth = 0;
        private float _floorSpanDiff = 0.2f;
        private float _windowWidthThreshold = 12f;

        protected float Elevation;
        protected readonly float Height;
        protected readonly Random Random;
        protected readonly MeshData MeshData;

        protected SideBuilder(MeshData meshData, float height, Random random)
        {
            Height = height;
            Random = random;
            MeshData = meshData;
        }

        #region Fluent interface setters

        public SideBuilder SetFloors(int count)
        {
            _floorCount = count;
            return this;
        }

        public SideBuilder SetElevation(float elevation)
        {
            Elevation = elevation;
            return this;
        }

        public SideBuilder SetEntranceWidth(float width)
        {
            _entranceWidth = width;
            return this;
        }

        public SideBuilder SetFirstFloorHeight(float height)
        {
            _firstFloorHeight = height;
            return this;
        }

        public SideBuilder SetFloorSpan(float span)
        {
            _floorSpanDiff = span;
            return this;
        }

        public SideBuilder SetPositionNoise(float freq)
        {
            _positionNoiseFreq = freq;
            return this;
        }

        public SideBuilder SetColorNoise(float freq)
        {
            _colorNoiseFreq = freq;
            return this;
        }

        public SideBuilder SetFloorHeight(float floorHeight)
        {
            _floorHeight = floorHeight;
            return this;
        }

        public SideBuilder SetWindowWidthThreshold(float threshold)
        {
            _windowWidthThreshold = threshold;
            return this;
        }

        public SideBuilder CalculateFloors()
        {
            var floorHeights = Height - _firstFloorHeight;
            _floorCount = (int)Math.Ceiling(floorHeights / _floorHeight);

            return this;
        }

        #endregion

        public virtual void Build(MapPoint s, MapPoint e)
        {
            var start = new Vector2(s.X, s.Y);
            var end = new Vector2(e.X, e.Y);
            var distance = Vector2.Distance(start, end);

            // TODO improve elevation processing
            var elevation = Elevation;

            var heightStep = (Height - _firstFloorHeight) / _floorCount;

            BuildGroundFloor(s, e, _firstFloorHeight);

            if (Math.Abs(_entranceWidth) < 0.001f)
                _entranceWidth = GetEntranceWidth(distance);

            var count = GetEntranceCount(distance);
            var widthStep = distance / count;

            // floor iterator
            for (int i = 0; i < _floorCount; i++)
            {
                var isWindowFloor = i % 2 == 1;
                var isFirst = i == 0;
                var isLast = i == _floorCount - 1;

                var floor = elevation + _firstFloorHeight + i * heightStep + (isWindowFloor ? -_floorSpanDiff : 0);
                var ceiling = floor + heightStep + (isLast ? 0 : (isWindowFloor ? _floorSpanDiff : -_floorSpanDiff));

                // latest floor without windows
                if (isLast && isWindowFloor) ceiling += _floorSpanDiff;
                var direction = (end - start).normalized;

                // building entrance iterator
                for (int k = 0; k < count; k++)
                {
                    var p1 = start + direction * (widthStep * k);
                    var p2 = start + direction * (widthStep * (k + 1));

                    var floorNoise1 = isFirst ? 0 : GetPositionNoise(new MapPoint(p1.x, p1.y, floor));
                    var floorNoise2 = isFirst ? 0 : GetPositionNoise(new MapPoint(p2.x, p2.y, floor));
                    var ceilingNoise1 = isLast ? 0 : GetPositionNoise(new MapPoint(p1.x, p1.y, ceiling));
                    var ceilingNoise2 = isLast ? 0 : GetPositionNoise(new MapPoint(p2.x, p2.y, ceiling));

                    var a = new MapPoint(p1.x + floorNoise1, p1.y + floorNoise1, floor + floorNoise1);
                    var b = new MapPoint(p2.x + floorNoise2, p2.y + floorNoise2, floor + floorNoise2);
                    var c = new MapPoint(p2.x + ceilingNoise2, p2.y + ceilingNoise2, ceiling + ceilingNoise2);
                    var d = new MapPoint(p1.x + ceilingNoise1, p1.y + ceilingNoise1, ceiling + ceilingNoise1);

                    if (isWindowFloor && distance > _windowWidthThreshold)
                        BuildWindow(k, a, b, c, d);
                    else
                        BuildSpan(k, a, b, c, d);
                }
            }
        }

        #region Abstract methods

        protected abstract void BuildGroundFloor(MapPoint start, MapPoint end, float floorHeight);
        protected abstract void BuildWindow(int step, MapPoint a, MapPoint b, MapPoint c, MapPoint d);
        protected abstract void BuildSpan(int step, MapPoint a, MapPoint b, MapPoint c, MapPoint d);
        protected abstract void BuildGlass(MapPoint a, MapPoint b, MapPoint c, MapPoint d);

        #endregion

        #region Behavior specific

        public virtual bool CanBuild(MapPoint start, MapPoint end)
        {
            return start.DistanceTo(end) > _entranceWidth * 2;
        }

        protected virtual float GetEntranceCount(float distance)
        {
            return (float)Math.Ceiling(distance / _entranceWidth);
        }

        protected virtual float GetEntranceWidth(float distance)
        {
            if (distance > 50)
                return Random.NextFloat(9, 11);
            if (distance > 30)
                return Random.NextFloat(8, 9);
            if (distance > 20)
                return Random.NextFloat(5, 8);
            return Random.NextFloat(3, 5);
        }

        #endregion

        #region Noise specific

        protected float GetPositionNoise(MapPoint point)
        {
            return Noise.Perlin3D(new Vector3(point.X, point.Elevation, point.Y), _positionNoiseFreq);
        }

        protected Color GetColor(GradientWrapper gradient, MapPoint point)
        {
            var value = (Noise.Perlin3D(new Vector3(point.X, point.Elevation, point.Y), _colorNoiseFreq) + 1f) / 2f;
            return gradient.Evaluate(value);
        }

        #endregion

        #region Planes

        /// <summary> Adds plane. </summary>
        protected void AddPlane(Color color1, Color color2, MapPoint a, MapPoint b, MapPoint c, MapPoint d)
        {
            MeshData.AddTriangle(a, c, b, color1);
            MeshData.AddTriangle(d, c, a, color2);
        }

        /// <summary> Adds plane. </summary>
        protected void AddPlane(Color color, MapPoint a, MapPoint b, MapPoint c, MapPoint d)
        {
            AddPlane(color, color, a, b, c, d);
        }

        #endregion
    }
}