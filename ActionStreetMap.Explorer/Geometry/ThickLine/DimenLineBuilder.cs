using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.ThickLine
{
    /// <summary> Thick line with height and width in 3D space. </summary>
    public class DimenLineBuilder : ThickLineBuilder
    {
        private float _height;

        /// <summary> Creates instance of <see cref="DimenLineBuilder"/>. </summary>
        /// <param name="elevationProvider">Elevation provider.</param>
        /// <param name="objectPool">Object Pool.</param>
        public DimenLineBuilder(IElevationProvider elevationProvider, IObjectPool objectPool)
            : base(elevationProvider, objectPool)
        {
            _height = 2f;
        }

        /// <summary> Sets height. </summary>
        public DimenLineBuilder SetHeight(float height)
        {
            _height = height;
            return this;
        }

        /// <inheritdoc />
        public override void Build(MapRectangle rectangle, List<LineElement> elements, Action<MeshData> builder)
        {
            base.Build(rectangle, elements, (data) =>
            {
                ProcessLatestFace();
                builder(data);
            });
        }

        private void ProcessLatestFace()
        {
            if (!Data.Triangles.Any())
                return;

            // NOTE we have to add the latest face
            // NOTE assume that top side is added the latest
            var lastTriangle = Data.Triangles[Data.Triangles.Count - 1];
            var first = lastTriangle.Vertex2;
            var second = lastTriangle.Vertex0;
            base.AddTrapezoid(
                new Vector3(first.X, first.Elevation, first.Y),
                new Vector3(second.X, second.Elevation, second.Y),
                new Vector3(second.X, second.Elevation - _height, second.Y),
                new Vector3(first.X, first.Elevation - _height, first.Y));
        }

        /// <inheritdoc />
        protected override void AddTrapezoid(Vector3 rightStart, Vector3 leftStart, Vector3 leftEnd, Vector3 rightEnd)
        {
            // move up original points
            var newRightStart = new Vector3(rightStart.x, rightStart.y + _height, rightStart.z);
            var newLeftStart = new Vector3(leftStart.x, leftStart.y + _height, leftStart.z);
            var newLeftEnd = new Vector3(leftEnd.x, leftEnd.y + _height, leftEnd.z);
            var newRightEnd = new Vector3(rightEnd.x, rightEnd.y + _height, rightEnd.z);

            // front face
            if (Data.Triangles.Count == 0)
                base.AddTrapezoid(leftStart, newLeftStart, newRightStart, rightStart);

            // right side
            base.AddTrapezoid(rightStart, newRightStart, newRightEnd, rightEnd);

            // left side
            base.AddTrapezoid(leftEnd, newLeftEnd, newLeftStart, leftStart);

            // add top
            base.AddTrapezoid(newRightStart, newLeftStart, newLeftEnd, newRightEnd);
        }

        /// <inheritdoc />
        protected override void AddTriangle(Vector3 first, Vector3 second, Vector3 third, bool invert)
        {
            var newFirst = new Vector3(first.x, first.y + _height, first.z);
            var newSecond = new Vector3(second.x, second.y + _height, second.z);
            var newThird = new Vector3(third.x, third.y + _height, third.z);

            // side
            if (invert)
                base.AddTrapezoid(first, newFirst, newThird, third);
            else
                base.AddTrapezoid(third, newThird, newFirst, first);

            // top
            base.AddTriangle(newFirst, newSecond, newThird, invert);
        }
    }
}
