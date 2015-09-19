using System;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal sealed class WindowWallBuilder : EmptyWallBuilder
    {
        private const int VertexStepCount = 24;

        private float _windowWidthRatio = 0.5f;
        private float _windowHeightRatio = 0.5f;

        private float _emptySpaceWidth;
        private float _emptySpaceHeight;

        private int _windowSkipIndex;

        public override int CalculateVertexCount(Vector3 start, Vector3 end)
        {
            var direction = (end - start);
            direction.Normalize();

            var distance = Vector3.Distance(start, end);

            var xIterCount = (int)Math.Ceiling(distance / MinStepWidth);
            var yIterCount = (int)Math.Ceiling(Height / MinStepHeight);

            if (distance < MinWallWidth)
                return base.CalculateVertexCount(start, end);

            return xIterCount * yIterCount * VertexStepCount;
        }

        protected override void OnParametersCalculated()
        {
            _emptySpaceWidth = XStep * (1 - _windowWidthRatio) / 2;
            _emptySpaceHeight = YStep * (1 - _windowHeightRatio) / 2;

            _windowSkipIndex = XIterCount/2;
        }

        protected override void BuildSegment(int i, int j, float yStart)
        {
            if (Distance < MinWallWidth)
            {
                base.BuildSegment(i, j, yStart);
                return;
            }

            var startIndex = VertStartIndex + (VertexStepCount * XIterCount) * j + i * VertexStepCount;

            if (IsNonWindowSegment(i))
            {
                if (j == 0)
                    BuildEntrance(i, j, yStart, startIndex);
                else
                    BuildEmptySpace(i, j, yStart, startIndex);
            }
            else
                BuildWindow(i, j, yStart, startIndex);
        }

        private float MinWallWidth { get { return MinStepWidth* 1.5f; } }

        private bool IsNonWindowSegment(int i)
        {
            return i != 0 && i != XIterCount - 1 && i%_windowSkipIndex == 0;
        }

        private void BuildWindow(int i, int j, float yStart, int startIndex)
        {
            var x1 = new Vector3(Start.x, yStart, Start.z) + Direction * (i * XStep);
            var q1 = x1 + Direction * _emptySpaceWidth;
            var x2 = x1 + Direction * XStep;
            var q2 = x2 - Direction * _emptySpaceWidth;

             BuildPlane(x1, q1, 
                new Vector3(q1.x, q1.y + YStep, q1.z),
                new Vector3(x1.x, x1.y + YStep, x1.z), startIndex);

            BuildPlane(q1, q2,
                new Vector3(q2.x, q2.y + _emptySpaceHeight, q2.z),
                new Vector3(q1.x, q1.y + _emptySpaceHeight, q1.z), startIndex + 6);

            BuildPlane(new Vector3(q1.x, q1.y + YStep - _emptySpaceHeight, q1.z),
                       new Vector3(q2.x, q2.y + YStep - _emptySpaceHeight, q2.z),
                       new Vector3(q2.x, q2.y + YStep, q2.z),
                       new Vector3(q1.x, q1.y + YStep, q1.z), startIndex + 12);

            BuildPlane(q2, x2,
               new Vector3(x2.x, x2.y + YStep, x2.z),
               new Vector3(q2.x, q2.y + YStep, q2.z), startIndex + 18);
        }

        private void BuildEmptySpace(int i, int j, float yStart, int startIndex)
        {
            var stepVector = XStep/4 * Direction;
            var x1 = new Vector3(Start.x, yStart, Start.z) + Direction * (i * XStep);
            var q1 = x1 + stepVector;
            var q2 = q1 + stepVector;
            var q3 = q2 + stepVector;
            var x2 = x1 + Direction * XStep;

            BuildPlane(x1, q1,
                new Vector3(q1.x, q1.y + YStep, q1.z),
                new Vector3(x1.x, x1.y + YStep, x1.z), startIndex);

            BuildPlane(q1, q2,
                new Vector3(q2.x, q2.y + YStep, q2.z),
                new Vector3(q1.x, q1.y + YStep, q1.z), startIndex + 6);

            BuildPlane(q2, q3,
                new Vector3(q3.x, q3.y + YStep, q3.z),
                new Vector3(q2.x, q2.y + YStep, q2.z), startIndex + 12);

            BuildPlane(q3, x2,
                new Vector3(x2.x, x2.y + YStep, x2.z),
                new Vector3(q3.x, q3.y + YStep, q3.z), startIndex + 18);
        }

        public void BuildEntrance(int i, int j, float yStart, int startIndex)
        {
            var yEnd = yStart + YStep;
            yStart = yEnd - _emptySpaceHeight;

            var x1 = new Vector3(Start.x, yStart, Start.z) + Direction * (i * XStep);
            var x2 = x1 + Direction * XStep;
            x2 = new Vector3(x2.x, yStart, x2.z);
            var x3 = new Vector3(x2.x, yEnd, x2.z);
            var x4 = new Vector3(x1.x, yEnd, x1.z);

            BuildPlane(x1, x2, x3, x4, startIndex);
            // NOTE skip vertices
            MeshData.NextIndex += 18;
        }

        private void BuildPlane(Vector3 x1, Vector3 x2, Vector3 x3, Vector3 x4, int startIndex)
        {
            var count = startIndex;

            #region Vertices
            Vertices[count] = x1;
            Vertices[count + HalfVertCount] = x1;
            Vertices[++count] = x4;
            Vertices[count + HalfVertCount] = x4;
            Vertices[++count] = x3;
            Vertices[count + HalfVertCount] = x3;

            Vertices[++count] = x2;
            Vertices[count + HalfVertCount] = x2;
            Vertices[++count] = x1;
            Vertices[count + HalfVertCount] = x1;
            Vertices[++count] = x3;
            Vertices[count + HalfVertCount] = x3;
            #endregion

            #region Triangles
            for (int i = startIndex; i < startIndex + 6; i++)
                Triangles[i] = i;

            var lastIndex = startIndex + HalfVertCount + 6;
            for (int i = startIndex + HalfVertCount; i < lastIndex; i++)
            {
                var rest = i % 3;
                Triangles[i] = rest == 0 ? i : (rest == 1 ? i + 1 : i - 1);
            }
            #endregion

            #region Colors
            count = startIndex;
            var color = GetColor(x1);
            Colors[count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            
            color = GetColor(x2);
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            #endregion

            MeshData.NextIndex += 6;
        }
    }
}
