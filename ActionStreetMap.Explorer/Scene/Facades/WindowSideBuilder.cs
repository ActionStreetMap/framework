using System;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal class WindowSideBuilder : SideBuilder
    {
        private GradientWrapper _facadeGradient;
        private float _groundFloorEntranceWidth = 12;
        private float _windowOffset = 0.2f;
        private float _windowWidthThreshold = 1.2f;

        public WindowSideBuilder(MeshData meshData, float height, System.Random random) :
            base(meshData, height, random)
        {
        }

        public WindowSideBuilder SetFacadeGradient(GradientWrapper gradient)
        {
            _facadeGradient = gradient;
            return this;
        }

        public WindowSideBuilder SetGroundFloorEntranceWidth(float width)
        {
            _groundFloorEntranceWidth = width;
            return this;
        }

        public WindowSideBuilder SetWindowOffset(float offset)
        {
            _windowOffset = offset;
            return this;
        }

        //public WindowSideBuilder SetWindowWidthThreshold(float threshold)
        //{
        //    _windowWidthThreshold = threshold;
        //    return this;
        //}

        protected override float GetEntranceWidth(float distance)
        {
            if (distance > 50)
                return Random.NextFloat(2.8f, 3.8f);
            return Random.NextFloat(1.8f, 2.8f);
        }

        protected override void BuildGroundFloor(MapPoint s, MapPoint e, float floorHeight)
        {
            var start = new Vector2(s.X, s.Y);
            var end = new Vector2(e.X, e.Y);

            var floor = Elevation;
            var ceiling = floor + floorHeight;

            var distance = Vector2.Distance(start, end);

            var count = (float)Math.Ceiling(distance / _groundFloorEntranceWidth);

            var widthStep = distance / count;

            var direction = (end - start).normalized;
            for (int k = 0; k < count; k++)
            {
                var p1 = start + direction * (widthStep * k);
                var p2 = start + direction * (widthStep * (k + 1));

                var floorNoise1 = GetPositionNoise(new MapPoint(p1.x, p1.y, floor));
                var floorNoise2 = GetPositionNoise(new MapPoint(p2.x, p2.y, floor));

                var a = new MapPoint(p1.x + floorNoise1, p1.y + floorNoise1, floor + floorNoise1);
                var b = new MapPoint(p2.x + floorNoise2, p2.y + floorNoise2, floor + floorNoise2);
                var c = new MapPoint(p2.x, p2.y, ceiling);
                var d = new MapPoint(p1.x, p1.y, ceiling);

                AddPlane(Color.grey, Color.grey, a, b, c, d);
            }
        }

        protected override void BuildWindow(int step, MapPoint a, MapPoint b, MapPoint c, MapPoint d)
        {
            var s1 = b - a;
            var s2 = c - a;

            var side1 = new Vector3(s1.X, s1.Elevation, s1.Y);
            var side2 = new Vector3(s2.X, s2.Elevation, s2.Y);

            var color = GetColor(_facadeGradient, a);

            var perp = Vector3.Cross(side2, side1).normalized;
            var isWindowOffset = step % 2 == 1 && a.DistanceTo(b) >= _windowWidthThreshold;

            var offsetVector = new MapPoint(perp.x, perp.z, perp.y) * (isWindowOffset ? _windowOffset : 0f);
            if (isWindowOffset)
            {
                AddPlane(color, a, b, b + offsetVector, a + offsetVector);
                AddPlane(color, b, c, c + offsetVector, b + offsetVector);
                AddPlane(color, c, d, d + offsetVector, c + offsetVector);
                AddPlane(color, d, a, a + offsetVector, d + offsetVector);
            }

            if (isWindowOffset)
                BuildGlass(a + offsetVector, b + offsetVector, c + offsetVector, d + offsetVector);
            else
                AddPlane(color, a + offsetVector, b + offsetVector, c + offsetVector, d + offsetVector);
        }

        protected override void BuildSpan(int step, MapPoint a, MapPoint b, MapPoint c, MapPoint d)
        {
            var color1 = GetColor(_facadeGradient, a);
            var color2 = GetColor(_facadeGradient, b);
            AddPlane(color1, color2, a, b, c, d);
        }

        protected override void BuildGlass(MapPoint a, MapPoint b, MapPoint c, MapPoint d)
        {
            AddPlane(new Color32(198, 186, 222, 1), a, b, c, d);
        }
    }
}
