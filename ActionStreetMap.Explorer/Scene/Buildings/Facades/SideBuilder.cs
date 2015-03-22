using System;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Buildings.Facades
{
    internal abstract class SideBuilder
    {
        private float _positionNoiseFreq = 0.05f;
        private float _colorNoiseFreq = 0.2f;
        private float _firstFloorHeight = 4;
        private int _floorCount = 3;
        private float _entranceWidth = 5f;
        private float _floorSpanDiff = 1f;
        private float _positionNoisePower = 1f;

        protected bool OptimizeParams = false;
        protected float Elevation;
        protected readonly float Height;
        protected readonly MeshData MeshData;

        protected SideBuilder(MeshData meshData, float height)
        {
            Height = height;
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

        public SideBuilder SetPositionNoise(float freq, float power)
        {
            _positionNoiseFreq = freq;
            _positionNoisePower = power;
            return this;
        }

        public SideBuilder SetColorNoise(float freq)
        {
            _colorNoiseFreq = freq;
            return this;
        }

        public SideBuilder Optimize()
        {
            OptimizeParams = true;
            return this;
        }

        #endregion

        public virtual void Build(MapPoint start, MapPoint end)
        {
            var distance = start.DistanceTo(end);

            if (OptimizeParams) Recalculate(distance);

            // TODO improve elevation processing
            var elevation = Elevation;

            var heightStep = (Height - _firstFloorHeight) / _floorCount;

            BuildGroundFloor(start, end, _firstFloorHeight);

            var count = GetEntranceCount(distance);
            var widthStep = distance / count;

            // floor iterator
            for (int i = 0; i < _floorCount; i++)
            {
                var isWindowFloor = i % 2 == 1;
                var isFirst = i == 0;
                var isLast = i == _floorCount - 1;

                var floor = elevation + _firstFloorHeight + i * heightStep + (isWindowFloor ? -_floorSpanDiff : 0);
                var ceiling = floor + heightStep + (isWindowFloor ? _floorSpanDiff : -_floorSpanDiff);

                // latest floor without windows
                if (isLast) ceiling += _floorSpanDiff;
                var direction = (end - start).Normalize();

                // building entrance iterator
                for (int k = 0; k < count; k++)
                {
                    var p1 = start + direction * (widthStep * k);
                    var p2 = start + direction * (widthStep * (k + 1));

                    var floorNoise1 = isFirst ? 0 : GetPositionNoise(new Vector3(p1.X, floor, p1.Y));
                    var floorNoise2 = isFirst ? 0 : GetPositionNoise(new Vector3(p2.X, floor, p2.Y));
                    var ceilingNoise1 = isLast ? 0 : GetPositionNoise(new Vector3(p1.X, ceiling, p1.Y));
                    var ceilingNoise2 = isLast ? 0 : GetPositionNoise(new Vector3(p2.X, ceiling, p2.Y));

                    var a = new Vector3(p1.X + floorNoise1, floor + floorNoise1, p1.Y + floorNoise1);
                    var b = new Vector3(p2.X + floorNoise2, floor + floorNoise2, p2.Y + floorNoise2);
                    var c = new Vector3(p2.X + ceilingNoise2, ceiling + ceilingNoise2, p2.Y + ceilingNoise2);
                    var d = new Vector3(p1.X + ceilingNoise1, ceiling + ceilingNoise1, p1.Y + ceilingNoise1);

                    if (isWindowFloor)
                        BuildWindow(k, a, b, c, d);
                    else
                        BuildSpan(k, a, b, c, d);
                }
            }
        }

        #region Abstract methods

        protected abstract void BuildGroundFloor(MapPoint start, MapPoint end, float floorHeight);
        protected abstract void BuildWindow(int step, Vector3 a, Vector3 b, Vector3 c, Vector3 d);
        protected abstract void BuildSpan(int step, Vector3 a, Vector3 b, Vector3 c, Vector3 d);
        protected abstract void BuildGlass(Vector3 a, Vector3 b, Vector3 c, Vector3 d);

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

        protected virtual void Recalculate(float distance)
        {
            var floorHeight = Height - _firstFloorHeight;
            _floorCount = (int) Math.Ceiling(floorHeight/3f);
        }

        #endregion

        #region Noise specific

        protected float GetPositionNoise(Vector3 point)
        {
            return Noise.Perlin3D(point, _positionNoiseFreq) * _positionNoisePower;
        }

        protected Color GetColor(GradientWrapper gradient, Vector3 point)
        {
            var value = (Noise.Perlin3D(point, _colorNoiseFreq) + 1f) / 2f;
            return gradient.Evaluate(value);
        }

        #endregion

        #region Planes

        /// <summary> Adds plane with non-shared vertices. </summary>
        protected void AddPlane(Color color1, Color color2, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var vertices = MeshData.Vertices;
            var triangles = MeshData.Triangles;
            var colors = MeshData.Colors;
            var vIndex = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);

            vertices.Add(d);
            vertices.Add(a);
            vertices.Add(c);

            triangles.Add(vIndex);
            triangles.Add(vIndex + 1);
            triangles.Add(vIndex + 2);

            triangles.Add(vIndex + 3);
            triangles.Add(vIndex + 4);
            triangles.Add(vIndex + 5);

            colors.Add(color1);
            colors.Add(color1);
            colors.Add(color1);
            colors.Add(color2);
            colors.Add(color2);
            colors.Add(color2);
        }

        /// <summary> Adds plane with shared vertices. </summary>
        protected void AddPlane(Color color, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var vertices = MeshData.Vertices;
            var triangles = MeshData.Triangles;
            var colors = MeshData.Colors;

            var vIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            triangles.Add(vIndex);
            triangles.Add(vIndex + 1);
            triangles.Add(vIndex + 2);

            triangles.Add(vIndex + 3);
            triangles.Add(vIndex + 0);
            triangles.Add(vIndex + 2);

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
        }

        #endregion
    }
}