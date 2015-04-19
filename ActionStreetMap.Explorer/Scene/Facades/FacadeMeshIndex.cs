using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    /// <summary> Represents facade index. </summary>
    internal class FacadeMeshIndex: IMeshIndex
    {
        private List<MeshTriangle> _triangles;
        private int _region = -1;
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
        public List<int> Query(MapPoint center, float radius, out MapPoint direction)
        {
            var range = GetRange(center, radius);

            // get normal to side
            var sideDirection = (range.SideEnd - range.SideStart).Normalize();
            direction = new MapPoint(sideDirection.Y, -sideDirection.X);

            return Enumerable.Range(range.VertexStart, range.VertexEnd - range.VertexStart + 1).ToList();
        }

        #endregion

        /// <summary> Gets affected range. </summary>
        private Range GetRange(MapPoint center, float radius)
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
