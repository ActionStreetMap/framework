using System;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal class EmptySideBuilder : SideBuilder
    {
        private GradientWrapper _facadeGradient;
        private float _maxWidth = 4f;

        public EmptySideBuilder(MeshData meshData, float height, System.Random random)
            : base(meshData, height, random)
        {
        }

        public EmptySideBuilder SetFacadeGradient(GradientWrapper gradient)
        {
            _facadeGradient = gradient;
            return this;
        }

        public EmptySideBuilder SetMaxWidth(float width)
        {
            _maxWidth = width;
            return this;
        }

        public override bool CanBuild(MapPoint start, MapPoint end)
        {
            return true;
        }

        protected override void BuildGroundFloor(MapPoint s, MapPoint e, float floorHeight)
        {
            var start = new Vector2(s.X, s.Y);
            var end = new Vector2(e.X, e.Y);

            var floor = Elevation;
            var ceiling = floor + floorHeight;

            var distance = Vector2.Distance(start, end);
            var count = (float)Math.Ceiling(distance / _maxWidth);
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
            BuildSpan(step, a, b, c, d);
        }

        protected override void BuildSpan(int step, MapPoint a, MapPoint b, MapPoint c, MapPoint d)
        {
            var color1 = GetColor(_facadeGradient, a);
            var color2 = GetColor(_facadeGradient, b);
            AddPlane(color1, color2, a, b, c, d);
        }

        protected override void BuildGlass(MapPoint a, MapPoint b, MapPoint c, MapPoint d)
        {
        }
    }
}
