using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    internal class FacadeMeshIndex: IMeshIndex
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

        public List<int> GetAfectedIndices(MapPoint center, float radius)
        {
            throw new NotImplementedException();
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
