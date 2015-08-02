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

            _xAxisStep = boundingBox.Width / columnCount;
            _yAxisStep = boundingBox.Height / rowCount;

            _ranges = new Range[rowCount * columnCount];
        }

        /// <inheritdoc />
        public void Build()
        {
            _triangles.Sort(new TriangleComparer(_columnCount,
                _left, _bottom, _xAxisStep, _yAxisStep));

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
        public MeshQuery.Result Modify(MeshQuery query)
        {
            var result = new List<int>(4);
            var center = query.Epicenter;
            var x = (int)Math.Floor((center.x - _left) / _xAxisStep);
            var y = (int)Math.Floor((center.z - _bottom) / _yAxisStep);

            var center2d = new Vector2d(center.x, center.z);
            for (int j = y - 1; j <= y + 1; j++)
                for (int i = x - 1; i <= x + 1; i++)
                {
                    var rectangle = new Rectangle2d(
                        _bottomLeft.X + i * _xAxisStep,
                        _bottomLeft.Y + j * _yAxisStep,
                        _xAxisStep,
                        _yAxisStep);

                    // NOTE enlarge search radius to prevent some issues with adjusted triangles
                    // as their position in index defined by their centroid
                    // actually, additional size depends on terrain triangle area
                    if (GeometryUtils.HasCollision(center2d, query.Radius + 6, rectangle))
                        AddRange(i, j, result);
                }

            return ModifyVertices(query, result);
        }

        private void AddRange(int i, int j, List<int> result)
        {
            var index = _columnCount * j + i;
            if (index >= _ranges.Length || index < 0 ||
                i >= _columnCount ||
                j >= _rowCount) return;

            var range = _ranges[index];
            result.AddRange(Enumerable.Range(range.Start, range.End - range.Start + 1));
        }

        #region Modification

        private MeshQuery.Result ModifyVertices(MeshQuery query, List<int> indecies)
        {
            var upMode = query.ForceDirection.y > 0;
            var modified = 0;
            var scannedTriangles = 0;

            // modify vertices
            for (int j = 0; j < indecies.Count; j++)
            {
                int outerIndex = indecies[j] * 3;
                for (var k = 0; k < 3; k++)
                {
                    var index = outerIndex + k;
                    var vertex = query.Vertices[index];
                    var distance = Vector3.Distance(vertex, query.Epicenter);
                    if (distance < query.Radius)
                    {
                        float heightDiff = query.GetForceChange(distance);
                        query.Vertices[index] = new Vector3(
                            vertex.x,
                            vertex.y + (upMode ? heightDiff : -heightDiff),
                            vertex.z);
                        modified++;
                    }
                }
                scannedTriangles++;
            }

            // search and adjust vertices on triangle sides
            for (int j = 0; j < indecies.Count; j++)
            {
                int outerIndex = indecies[j] * 3;

                for (var i = 0; i < indecies.Count; i++)
                {
                    if (i == j) continue;
                    int innerIndex = indecies[i] * 3;
                    for (int k = 0; k < 3; k++)
                    {
                        int vertIndex = innerIndex + k;
                        if (ModifyVertextOnSegment(query.Vertices, vertIndex, outerIndex + 0, outerIndex + 1) ||
                            ModifyVertextOnSegment(query.Vertices, vertIndex, outerIndex + 1, outerIndex + 2) ||
                            ModifyVertextOnSegment(query.Vertices, vertIndex, outerIndex + 2, outerIndex + 0))
                            modified++;
                    }
                }
            }

            return new MeshQuery.Result(query.Vertices)
            {
                ModifiedVertices = modified,
                ScannedTriangles = scannedTriangles
            };
        }

        private bool ModifyVertextOnSegment(Vector3[] vertices, int vIndex, int aIndex, int bIndex)
        {
            var p = vertices[vIndex];
            var a = vertices[aIndex];
            var b = vertices[bIndex];

            var vert2D = new Vector2(p.x, p.z);
            var a2D = new Vector2(a.x, a.z);
            var b2D = new Vector2(b.x, b.z);

            // check whether vertex is on segment
            if (vert2D == a2D || vert2D == b2D || (Vector2.Distance(vert2D, a2D) +  
                Vector2.Distance(vert2D, b2D) - Vector2.Distance(a2D, b2D)) > 0.01f) 
                return false;     

            var ray = b - a; // find direction from p1 to p2
            var rel = p - a; // find position relative to p1
            var n = ray.normalized; // create ray normal
            var l = Vector3.Dot(n, rel); // calculate dot
            vertices[vIndex] = a + n * l;
            return true;
        }

        #endregion

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