using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    public class FacadeMeshIndex: IMeshIndex
    {
        private List<MeshTriangle> _triangles;
        private int _region = -1;
        private readonly Range[] _ranges;

        public FacadeMeshIndex(int count, List<MeshTriangle> triangles)
        {
            _triangles = triangles;
            _ranges = new Range[count];
        }

        #region IMeshIndex members

        public void AddTriangle(MeshTriangle triangle)
        {
            triangle.Region = _region;
        }

        public void Build()
        {
            // NOTE don't keep reference
            _triangles = null;
        }

        public List<int> GetAfectedIndices(MapPoint center, float radius, out MapPoint direction)
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
            var range = _ranges[index];

            // get normal to side
            var sideDirection = (range.SideEnd - range.SideStart).Normalize();
            direction = new MapPoint(sideDirection.Y, -sideDirection.X);

            return Enumerable.Range(range.VertexStart, range.VertexEnd - range.VertexStart + 1).ToList();
        }

        #endregion

        public void SetSide(MapPoint start, MapPoint end)
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

        private class Range
        {
            public int VertexStart;
            public int VertexEnd;

            public MapPoint SideStart;
            public MapPoint SideEnd;
        }
    }
}
