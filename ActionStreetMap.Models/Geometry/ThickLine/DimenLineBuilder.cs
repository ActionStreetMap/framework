using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Elevation;
using UnityEngine;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Models.Geometry.ThickLine
{
    /// <summary>
    ///     Thick line with height and width in 3D space. Can be used to represent barriers.
    /// </summary>
    public class DimenLineBuilder : ThickLineBuilder
    {
        /// <summary>
        ///     Gets or sets height.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        ///     Creates instance of <see cref="DimenLineBuilder"/>.
        /// </summary>
        /// <param name="height">Line height.</param>
        /// <param name="objectPool">Object Pool.</param>
        public DimenLineBuilder(float height, IObjectPool objectPool)
            : base(objectPool)
        {
            Height = height;
        }

        /// <inheritdoc />
        public override void Build(HeightMap heightMap, List<LineElement> elements, Action<List<Vector3>, List<int>, List<Vector2>> builder)
        {
            base.Build(heightMap, elements, (p, t, u) =>
            {
                ProcessLatestFace();
                builder(Points, Triangles, Uv);
            });
        }

        private void ProcessLatestFace()
        {
            if (Points.Count > 1)
            {
                // NOTE we have to add the latest face
                // NOTE assume that top side is added the latest

                var first = Points[Points.Count - 1];
                var second = Points[Points.Count - 2];
                base.AddTrapezoid(first, second,
                    new Vector3(second.x, second.y - Height, second.z),
                    new Vector3(first.x, first.y - Height, first.z));
            }
        }

        /// <inheritdoc />
        protected override void AddTrapezoid(Vector3 rightStart, Vector3 leftStart, Vector3 leftEnd, Vector3 rightEnd)
        {
            // move up original points
            var newRightStart = new Vector3(rightStart.x, rightStart.y + Height, rightStart.z);
            var newLeftStart = new Vector3(leftStart.x, leftStart.y + Height, leftStart.z);
            var newLeftEnd = new Vector3(leftEnd.x, leftEnd.y + Height, leftEnd.z);
            var newRightEnd = new Vector3(rightEnd.x, rightEnd.y + Height, rightEnd.z);

            // front face
            if (Triangles.Count == 0)
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
            var newFirst = new Vector3(first.x, first.y + Height, first.z);
            var newSecond = new Vector3(second.x, second.y + Height, second.z);
            var newThird = new Vector3(third.x, third.y + Height, third.z);

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
