using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> 
    ///     Maintains index of triangles in given bounding box. The bounding box is divided 
    ///     to regions of certain size defined by column and row count. Triangle's 
    ///     centroid is used to map triangle to the corresponding region.     
    /// </summary>
    internal sealed class TerrainMeshIndex : IMeshIndex
    {
        private readonly int _columnCount;
        private readonly int _rowCount;
        private readonly double _xAxisStep;
        private readonly double _yAxisStep;
        private readonly double _left;
        private readonly double _bottom;

        private readonly Vector2d _bottomLeft;
        private readonly Range[] _ranges;
        
        internal int CheckedTriangles;

        private List<TerrainMeshTriangle> _triangles;

        /// <summary> Creates instance of <see cref="TerrainMeshIndex"/>. </summary>
        /// <param name="columnCount">Column count of given bounding box.</param>
        /// <param name="rowCount">Row count of given bounding box.</param>
        /// <param name="boundingBox">Bounding box.</param>
        /// <param name="triangles">Triangles</param>
        public TerrainMeshIndex(int columnCount, int rowCount, Rectangle2d boundingBox, 
            List<TerrainMeshTriangle> triangles)
        {
            _columnCount = columnCount;
            _rowCount = rowCount;
            _triangles = triangles;
            _left = boundingBox.Left;
            _bottom = boundingBox.Bottom;

            _bottomLeft = boundingBox.BottomLeft;

            _xAxisStep = boundingBox.Width/columnCount;
            _yAxisStep = boundingBox.Height/rowCount;

            _ranges = new Range[rowCount * columnCount];
        }

        /// <inheritdoc />
        public void Build()
        {
            _triangles.Sort(new TriangleComparer(
                _columnCount, _left, _bottom, _xAxisStep, _yAxisStep));

            var rangeIndex = -1;
            for (int i = 0; i < _triangles.Count; i++)
            {
                var triangle = _triangles[i];
                if (triangle.Region != rangeIndex)
                {
                    if (i != 0)
                        _ranges[rangeIndex].End = i - 1;

                    rangeIndex = triangle.Region;
                    _ranges[rangeIndex].Start = i;
                }
            }
            _ranges[rangeIndex].End = _triangles.Count - 1;
            _triangles = null;
        }

        /// <inheritdoc />
        public int Modify(MeshQuery query)
        {
            // NOTE not thread safe, but it's used for unit testing only
            CheckedTriangles = 0;
            var modified = 0;
            var center = query.Epicenter;
            var x = (int) Math.Floor((center.x - _left)/_xAxisStep);
            var y = (int) Math.Floor((center.z - _bottom)/_yAxisStep);

            // NOTE Actually, this is not correct in case of epicenter is 
            // above or below terrain surface
            var center2D = new Vector2d(center.x, center.z);
            for (int j = y - 1; j <= y + 1; j++)
            {
                for (int i = x - 1; i <= x + 1; i++)
                {
                    var rectangle = new Rectangle2d(
                        _bottomLeft.X + i*_xAxisStep,
                        _bottomLeft.Y + j*_yAxisStep,
                        _xAxisStep,
                        _yAxisStep);

                    if (GeometryUtils.HasCollision(center2D, query.Radius, rectangle))
                        modified += ModifyRange(query, i, j);
                }
            }
            return modified;
        }

        private int ModifyRange(MeshQuery query, int i, int j)
        {
            var rangeIndex = _columnCount*j + i;
            if (rangeIndex >= _ranges.Length ||
                rangeIndex < 0 ||
                i >= _columnCount || 
                j >= _rowCount) return 0;

            var modified = 0;
            var range = _ranges[rangeIndex];
            var vertices = query.Vertices;
            // go through all potentially affected triangles
            foreach (var triIndex in Enumerable.Range(range.Start, range.End - range.Start + 1))
            {
                CheckedTriangles++;
                int outerIndex = triIndex * 3;
                // calculate triangle centroid
                float x = 0, y = 0, z = 0;
                for (var k = outerIndex; k < outerIndex + 3; k++)
                {
                    x += vertices[k].x;
                    y += vertices[k].y;
                    z += vertices[k].z;
                }
                var centroid = new Vector3(x/3, y/3, z/3);
                // if epicenter is close to centroid then modify all triangle vertices
                // based on their distance to epicenter
                if (Vector3.Distance(centroid, query.Epicenter) < query.Radius)
                {
                    for (var k = outerIndex; k < outerIndex + 3; k++)
                    {
                        var vertex = vertices[k];
                        var distanceToCollidePoint = Vector3.Distance(vertex, query.Epicenter);
                        var forceChange = query.ForcePower*(query.Radius - distanceToCollidePoint)/2;
                        vertices[k] = vertex + forceChange*query.ForceDirection;
                    }
                    modified += 3;
                }
            }
            return modified;
        }

        #region Nested classes

        internal struct Range
        {
            public int Start;
            public int End;
        }

        /// <summary> Compares triangles based on their region. </summary>
        private class TriangleComparer : IComparer<TerrainMeshTriangle>
        {
            private readonly double _left;
            private readonly double _bottom;

            private readonly double _xAxisStep;
            private readonly double _yAxisStep;

            private readonly int _columnCount;

            public TriangleComparer(int columnCount, double left, double bottom, 
                double xAxisStep, double yAxisStep)
            {
                _columnCount = columnCount;
                _left = left;
                _bottom = bottom;
                _xAxisStep = xAxisStep;
                _yAxisStep = yAxisStep;
            }

            public int Compare(TerrainMeshTriangle x, TerrainMeshTriangle y)
            {
                EnsureRegion(x);
                EnsureRegion(y);
                return x.Region.CompareTo(y.Region);
            }

            private void EnsureRegion(TerrainMeshTriangle triangle)
            {
                if (triangle.Region != TerrainMeshTriangle.InvalidRegionIndex)
                    return;

                // TODO this method is called for offset triangles as well
                var p0 = triangle.Vertex0;
                var p1 = triangle.Vertex1;
                var p2 = triangle.Vertex2;
                var centroid = new Vector2d((p0.x + p1.x + p2.x) / 3, (p0.z + p1.z + p2.z) / 3);
                var i = (int)Math.Floor((centroid.X - _left) / _xAxisStep);
                var j = (int)Math.Floor((centroid.Y - _bottom) / _yAxisStep);

                triangle.Region = _columnCount*j + i;
            }
        }

        #endregion
    }
}