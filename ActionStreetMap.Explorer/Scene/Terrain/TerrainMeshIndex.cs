using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Explorer.Geometry;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> 
    ///     Maintains index of triangles in given bounding box. The bounding box is divided 
    ///     to regions of certain size defined by column and row count. Triangle's 
    ///     centroid is used to map triangle to the corresponding region.     
    /// </summary>
    internal class TerrainMeshIndex: IMeshIndex
    {
        private static readonly TriangleComparer Comparer = new TriangleComparer();

        private readonly int _columnCount;
        private readonly int _rowCount;
        private readonly float _xAxisStep;
        private readonly float _yAxisStep;
        private readonly float _left;
        private readonly float _right;
        private readonly float _top;
        private readonly float _bottom;

        private readonly MapPoint _bottomLeft;
        private readonly Range[] _ranges;

        private List<MeshTriangle> _triangles;
        private int _modifiedCount = 0;

        /// <summary> Creates instance of <see cref="TerrainMeshIndex"/>. </summary>
        /// <param name="columnCount">Column count of given bounding box.</param>
        /// <param name="rowCount">Row count of given bounding box.</param>
        /// <param name="boundingBox">Bounding box.</param>
        /// <param name="triangles">Triangles</param>
        public TerrainMeshIndex(int columnCount, int rowCount, MapRectangle boundingBox, List<MeshTriangle> triangles)
        {
            _columnCount = columnCount;
            _rowCount = rowCount;
            _triangles = triangles;
            _left = boundingBox.Left;
            _right = boundingBox.Right;
            _top = boundingBox.Top;
            _bottom = boundingBox.Bottom;

            _bottomLeft = boundingBox.BottomLeft;

            _xAxisStep = boundingBox.Width/columnCount;
            _yAxisStep = boundingBox.Height/rowCount;

            _ranges = new Range[rowCount * columnCount];
        }

        /// <inheritdoc />
        public void Build()
        {
            _triangles.Sort(Comparer);

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
        public void AddTriangle(MeshTriangle triangle)
        {
            var p0 = triangle.Vertex0;
            var p1 = triangle.Vertex1;
            var p2 = triangle.Vertex2;
            var centroid = new MapPoint((p0.X + p1.X + p2.X) / 3, (p0.Y + p1.Y + p2.Y) / 3);
            var i = (int)Math.Floor((centroid.X - _left) / _xAxisStep);
            var j = (int)Math.Floor((centroid.Y - _bottom) / _yAxisStep);

            triangle.Region = _columnCount * j + i;
        }

        /// <inheritdoc />
        public void Query(MapPoint center, float radius, Vector3[] vertices, Action<int, Vector2> modifyAction)
        {
            var result = new List<int>(32);

            var x = (int)Math.Floor((center.X - _left) / _xAxisStep);
            var y = (int)Math.Floor((center.Y - _bottom) / _yAxisStep);

            for (int j = y - 1; j <= y + 1; j++)
                for (int i = x - 1; i <= x + 1; i++)
                {
                    var rectangle = new MapRectangle(
                        _bottomLeft.X + i*_xAxisStep,
                        _bottomLeft.Y + j*_yAxisStep,
                        _xAxisStep,
                        _yAxisStep);

                    if (GeometryUtils.HasCollision(center, radius, rectangle))
                        AddRange(i, j, result);
                }
            ModifyVertices(result, vertices, new Vector3(center.X, center.Elevation, center.Y), 
                radius, modifyAction);

            // reset statisitcs
            _modifiedCount = 0;
        }

        private void AddRange(int i, int j, List<int> result)
        {
            var index = _columnCount*j + i;
            if (index >= _ranges.Length || 
                index < 0 ||
                i >= _columnCount || 
                j >= _rowCount) return;

            var range = _ranges[index];
            result.AddRange(Enumerable.Range(range.Start, range.End - range.Start + 1));
        }

        #region Modification

        private void ModifyVertices(List<int> indecies, Vector3[] vertices, Vector3 epicenter, float radius,
            Action<int, Vector2> modifyAction)
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
                    if (distance < radius && IsNotBorderVertex(vertex))
                        modifyAction(index, new Vector2());
                }
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
                        if (ModifyVertextOnSegment(vertices, vertIndex, outerIndex + 0, outerIndex + 1) ||
                            ModifyVertextOnSegment(vertices, vertIndex, outerIndex + 1, outerIndex + 2) ||
                            ModifyVertextOnSegment(vertices, vertIndex, outerIndex + 2, outerIndex + 0))
                            _modifiedCount++;
                    }
                }
            }
        }

        private bool IsNotBorderVertex(Vector3 vertex)
        {
            return Math.Abs(vertex.x - _left) > float.Epsilon &&
                   Math.Abs(vertex.x - _right) > float.Epsilon &&
                   Math.Abs(vertex.z - _top) > float.Epsilon &&
                   Math.Abs(vertex.z - _bottom) > float.Epsilon;
        }

        private bool IsVertextOnSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            var vert2D = new Vector2(p.x, p.z);
            var a2D = new Vector2(a.x, a.z);
            var b2D = new Vector2(b.x, b.z);

            if (vert2D == a2D || vert2D == b2D) return false;
            return Math.Abs(Vector2.Distance(vert2D, a2D) + Vector2.Distance(vert2D, b2D) - Vector2.Distance(a2D, b2D)) < 0.01f;
        }

        private bool ModifyVertextOnSegment(Vector3[] vertices, int vIndex, int aIndex, int bIndex)
        {
            var p = vertices[vIndex];
            var a = vertices[aIndex];
            var b = vertices[bIndex];

            if (!IsVertextOnSegment(p, a, b))
                return false;

            var ray = b - a; // find direction from p1 to p2
            var rel = p - a; // find position relative to p1
            var n = ray.normalized; // create ray normal
            var l = Vector3.Dot(n, rel); // calculate dot
            var result = a + n * l; // convert back into world space

            vertices[vIndex] = result;

            return true;
        }

        #endregion

        #region Nested classes

        private struct Range
        {
            public int Start;
            public int End;
        }

        private class TriangleComparer : IComparer<MeshTriangle>
        {
            public int Compare(MeshTriangle x, MeshTriangle y)
            {
                return x.Region.CompareTo(y.Region);
            }
        }

        #endregion
    }
}