using System;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal class EmptyWallBuilder : WallBuilder
    {
        private Vector2[] _uvs = new Vector2[5];

        /// <inheritdoc />
        public override int CalculateVertexCount(Vector3 start, Vector3 end)
        {
            var direction = (end - start);
            direction.Normalize();

            var distance = Vector3.Distance(start, end);

            var xIterCount = (int)Math.Ceiling(distance / MinStepWidth);
            var yIterCount = (int)Math.Ceiling(Height / MinStepHeight);

            return xIterCount * yIterCount * 12;
        }

        /// <inheritdoc />
        protected override void BuildSegment(int i, int j, float yStart)
        {
            var startIndex = VertStartIndex + (12 * XIterCount) * j + i * 12;

            var x1 = Start + Direction * (i * XStep);
            var middle = x1 + Direction * (0.5f * XStep);
            var x2 = x1 + Direction * XStep;

            BuildPlane(x1, middle, x2, yStart, startIndex);
        }

        protected override void OnParametersCalculated()
        {
            InitializeUVMapping();
        }

        private void InitializeUVMapping()
        {
            _uvs[0] = Texture.Map(new Vector2(0, 0));
            _uvs[1] = Texture.Map(new Vector2(1, 0));
            _uvs[2] = Texture.Map(new Vector2(1, 1));
            _uvs[3] = Texture.Map(new Vector2(0, 1));
            _uvs[4] = Texture.Map(new Vector2(0.5f, 0.5f));
        }

        private void BuildPlane(Vector3 x1, Vector3 middle, Vector3 x2,
            float yStart, int startIndex)
        {
            var yEnd = yStart + YStep;
            var p0 = new Vector3(x1.x, yStart, x1.z);
            var p1 = new Vector3(x2.x, yStart, x2.z);
            var p2 = new Vector3(x2.x, yEnd, x2.z);
            var p3 = new Vector3(x1.x, yEnd, x1.z);

            var pc = new Vector3(middle.x, yStart + (yEnd - yStart) / 2, middle.z);

            var count = startIndex;

            #region Vertices
            Vertices[count] = p3;
            Vertices[count + HalfVertCount] = p3;
            Vertices[++count] = p0;
            Vertices[count + HalfVertCount] = p0;
            Vertices[++count] = pc;
            Vertices[count + HalfVertCount] = pc;

            Vertices[++count] = p0;
            Vertices[count + HalfVertCount] = p0;
            Vertices[++count] = p1;
            Vertices[count + HalfVertCount] = p1;
            Vertices[++count] = pc;
            Vertices[count + HalfVertCount] = pc;

            Vertices[++count] = p1;
            Vertices[count + HalfVertCount] = p1;
            Vertices[++count] = p2;
            Vertices[count + HalfVertCount] = p2;
            Vertices[++count] = pc;
            Vertices[count + HalfVertCount] = pc;

            Vertices[++count] = p2;
            Vertices[count + HalfVertCount] = p2;
            Vertices[++count] = p3;
            Vertices[count + HalfVertCount] = p3;
            Vertices[++count] = pc;
            Vertices[count + HalfVertCount] = pc;
            #endregion

            #region Triangles
            // triangles for outer part
            for (int i = startIndex; i < startIndex + 12; i++)
                Triangles[i] = i;

            var lastIndex = startIndex + HalfVertCount + 12;
            for (int i = startIndex + HalfVertCount; i < lastIndex; i++)
            {
                var rest = i % 3;
                Triangles[i] = rest == 0 ? i : (rest == 1 ? i + 1 : i - 1);
            }
            #endregion

            #region Colors
            count = startIndex;
            var color = GetColor(p3);
            Colors[count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;

            color = GetColor(p0);
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;

            color = GetColor(p1);
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;

            color = GetColor(p2);
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            Colors[++count] = color;
            Colors[count + HalfVertCount] = color;
            #endregion

            #region UVs
            count = startIndex;

            UVs[count] = _uvs[3];
            UVs[count + HalfVertCount] = _uvs[3];
            UVs[++count] = _uvs[0];
            UVs[count + HalfVertCount] = _uvs[0];
            UVs[++count] = _uvs[4];
            UVs[count + HalfVertCount] = _uvs[4];

            UVs[++count] = _uvs[0];
            UVs[count + HalfVertCount] = _uvs[0];
            UVs[++count] = _uvs[1];
            UVs[count + HalfVertCount] = _uvs[1];
            UVs[++count] = _uvs[4];
            UVs[count + HalfVertCount] = _uvs[4];

            UVs[++count] = _uvs[1];
            UVs[count + HalfVertCount] = _uvs[1];
            UVs[++count] = _uvs[2];
            UVs[count + HalfVertCount] = _uvs[2];
            UVs[++count] = _uvs[4];
            UVs[count + HalfVertCount] = _uvs[4];

            UVs[++count] = _uvs[2];
            UVs[count + HalfVertCount] = _uvs[2];
            UVs[++count] = _uvs[3];
            UVs[count + HalfVertCount] = _uvs[3];
            UVs[++count] = _uvs[4];
            UVs[count + HalfVertCount] = _uvs[4];

            #endregion

            MeshData.NextIndex += 12;
        }
    }
}