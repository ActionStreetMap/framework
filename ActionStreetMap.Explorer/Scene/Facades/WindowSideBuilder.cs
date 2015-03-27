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

        protected override float GetEntranceWidth(float distance)
        {
            if (distance > 30)
                return Random.NextFloat(5, 7);
            if (distance > 20)
                return Random.NextFloat(4, 5);
            if (distance > 10)
                return Random.NextFloat(3, 4);
            return Random.NextFloat(2, 3);
        }

        protected override void BuildGroundFloor(MapPoint start, MapPoint end, float floorHeight)
        {
            var floor = Elevation;
            var ceiling = floor + floorHeight;

            var distance = start.DistanceTo(end);
            var count = (float)Math.Ceiling(distance / _groundFloorEntranceWidth);

            var widthStep = distance / count;

            var direction = (end - start).Normalize();
            for (int k = 0; k < count; k++)
            {
                var p1 = start + direction * (widthStep * k);
                var p2 = start + direction * (widthStep * (k + 1));

                var floorNoise1 = GetPositionNoise(new Vector3(p1.X, floor, p1.Y));
                var floorNoise2 = GetPositionNoise(new Vector3(p2.X, floor, p2.Y));

                var a = new Vector3(p1.X + floorNoise1, floor + floorNoise1, p1.Y + floorNoise1);
                var b = new Vector3(p2.X + floorNoise2, floor + floorNoise2, p2.Y + floorNoise2);
                var c = new Vector3(p2.X, ceiling, p2.Y);
                var d = new Vector3(p1.X, ceiling, p1.Y);

                AddPlane(Color.grey, Color.grey, a, b, c, d);
            }
        }

        protected override void BuildWindow(int step, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var side1 = b - a;
            var side2 = c - a;

            var perp = Vector3.Cross(side2, side1).normalized;
            var offsetVector = perp * 0f;

            var isWindowOffset = step % 2 == 1;
            if (isWindowOffset)
            {
                offsetVector = perp * 2;
                AddPlane(Color.yellow, a, b, b + offsetVector, a + offsetVector);
                AddPlane(Color.yellow, b, c, c + offsetVector, b + offsetVector);
                AddPlane(Color.yellow, c, d, d + offsetVector, c + offsetVector);
                AddPlane(Color.yellow, d, a, a + offsetVector, d + offsetVector);
            }

            if (isWindowOffset)
                BuildGlass(a + offsetVector, b + offsetVector, c + offsetVector, d + offsetVector);
            else
            {
                var color = GetColor(_facadeGradient, a);
                AddPlane(color, a + offsetVector, b + offsetVector, c + offsetVector, d + offsetVector);
            }
        }

        protected override void BuildSpan(int step, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var color1 = GetColor(_facadeGradient, a);
            var color2 = GetColor(_facadeGradient, b);
            AddPlane(color1, color2, a, b, c, d);
        }

        protected override void BuildGlass(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            AddPlane(new Color32(198, 186, 222, 1), a, b, c, d);
        }
    }
}
