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
        public void Query(MapPoint center, float radius, Vector3[] vertices, Action<int, Vector2> modifyAction)
        {
            var range = GetRange(center);

            // get normal to side
            var sideDirection = (range.SideEnd - range.SideStart).Normalize();
            var direction = new Vector2(sideDirection.Y, -sideDirection.X);
            // get ranges and modify vertices
            var ranges = Enumerable.Range(range.VertexStart, range.VertexEnd - range.VertexStart + 1).ToList();
            ModifyVertices(ranges, vertices, new Vector3(center.X, center.Elevation, center.Y), radius,
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
        internal void SetSide(MapPoint start, MapPoint end)
        {
            if (_region >= 0)
                _ranges[_region].VertexEnd = _triangles.Count - 1;

            var range = new Range()
            {
                VertexStart = _triangles.Count,
                SideStart = start,
                SideEnd = end
            };
            _ranges[++_region] = range;
        }

        #region Modification

        private void ModifyVertices(List<int> indecies, Vector3[] vertices, Vector3 epicenter, float radius,
            Vector2 direction, Action<int, Vector2> modifyAction)
        {
            // modify vertices
            for (int j = 0; j < indecies.Count; j++)
            {
                int outerIndex = indecies[j] * 3;
                for (var k = 0; k < 3; k++)
                {
                    var index = outerIndex + k;
                    var vertex = vertices[index];
                    var distance = Vector3.Distance(vertex, epicenter);
                    if (distance < radius)
                    {
                        modifyAction(index, direction);
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
        }

        #endregion
    }
}
