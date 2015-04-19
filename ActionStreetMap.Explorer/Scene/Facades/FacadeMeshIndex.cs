using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    /// <summary> Represents facade index. </summary>
    internal class FacadeMeshIndex: IMeshIndex
    {
        private List<MeshTriangle> _triangles;
        private int _region = -1;
        private int _modifiedCount = 0;
        private readonly Range[] _ranges;

        /// <summary> Creates instance of <see cref="FacadeMeshIndex"/>. </summary>
        public FacadeMeshIndex(int count, List<MeshTriangle> triangles)
        {
            _triangles = triangles;
            _ranges = new Range[count];
        }

        #region IMeshIndex members

        /// <inheritdoc />
        public MapRectangle BoundingBox { get; internal set; }

        /// <inheritdoc />
        public void AddTriangle(MeshTriangle triangle)
        {
            triangle.Region = _region;
        }

        /// <inheritdoc />
        public void Build()
        {
            // don't forget to set the lates range vertex end
            _ranges[_ranges.Length - 1].VertexEnd = _triangles.Count - 1;

            // NOTE don't keep reference
            _triangles = null;
        }

        /// <inheritdoc />
        public void Query(MapPoint center, float radius, Vector3[] vertices, Action<int, float, Vector2> modifyAction)
        {
            var range = GetRange(center);

            // get normal to side
            var sideDirection = (range.SideEnd - range.SideStart).Normalize();
            var direction = new Vector2(sideDirection.Y, -sideDirection.X);

            ModifyVertices(range, vertices, new Vector3(center.X, center.Elevation, center.Y), radius,
                direction, modifyAction);
            // reset statistics
            _modifiedCount = 0;
        }

        #endregion

        /// <summary> Gets affected range. </summary>
        private Range GetRange(MapPoint center)
        {
            var index = 0;
            var minDistance = float.MaxValue;
            // find side with min distance to center point
            for (int i = 0; i < _ranges.Length; i++)
            {
                var r = _ranges[i];
                var distance = LineUtils.LineToPointDistance2D(r.SideStart, r.SideEnd, center, true);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    index = i;
                }
            }
            return _ranges[index];
        }

        /// <summary> Sets side of facade. </summary>
        internal void SetSide(MapPoint start, MapPoint end, float elevation)
        {
            if (_region >= 0)
                _ranges[_region].VertexEnd = _triangles.Count - 1;

            var range = new Range()
            {
                VertexStart = _triangles.Count,
                SideStart = start,
                SideEnd = end,
                Elevation = elevation
            };
            _ranges[++_region] = range;
        }

        #region Modification

        private void ModifyVertices(Range range, Vector3[] vertices, Vector3 epicenter, float radius,
            Vector2 direction, Action<int, float, Vector2> modifyAction)
        {
            var start = range.VertexStart;
            var end = range.VertexEnd;
            var top = range.Elevation;

            var last = range.SideEnd;
            var begin = range.SideStart;
            // NOTE actually, this value depends on noise values
            const float tolerance = 0.5f;
            // modify vertices
            for (int i = start; i <= end; i++)
            {
                int outerIndex = i * 3;
                for (var k = 0; k < 3; k++)
                {
                    var index = outerIndex + k;
                    var v = vertices[index];
                    var distance = Vector3.Distance(v, epicenter);
                    if (distance < radius && 
                        Math.Abs(v.y - top) > tolerance &&
                        Math.Abs(v.x - begin.X) > tolerance && Math.Abs(v.z - begin.Y) > tolerance &&
                        Math.Abs(v.x - last.X) > tolerance && Math.Abs(v.z - last.Y) > tolerance)
                    {
                        modifyAction(index, distance, direction);
                        _modifiedCount++;
                    }
                }
            }
        }

        #endregion

        #region Nested classes

        /// <summary> Represents range of triangles of facade side. </summary>
        private class Range
        {
            public int VertexStart;
            public int VertexEnd;

            public MapPoint SideStart;
            public MapPoint SideEnd;

            public float Elevation;
        }

        #endregion
    }
}
