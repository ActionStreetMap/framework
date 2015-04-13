using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Explorer.Geometry;

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
        private readonly float _x;
        private readonly float _y;

        private readonly MapPoint _bottomLeft;

        private readonly Range[] _ranges;

        /// <summary> Creates instance of <see cref="TerrainMeshIndex"/>. </summary>
        /// <param name="columnCount">Column count of given bounding box.</param>
        /// <param name="rowCount">Row count of given bounding box.</param>
        /// <param name="boundingBox">Bounding box.</param>
        public TerrainMeshIndex(int columnCount, int rowCount, MapRectangle boundingBox)
        {
            _columnCount = columnCount;
            _rowCount = rowCount;
            _x = boundingBox.BottomLeft.X;
            _y = boundingBox.BottomLeft.Y;

            _bottomLeft = boundingBox.BottomLeft;

            _xAxisStep = boundingBox.Width/columnCount;
            _yAxisStep = boundingBox.Height/rowCount;

            _ranges = new Range[rowCount * columnCount];
        }

        /// <summary> Builds index from given list of triangles. Also performs its sorting. </summary>
        public void Build(MeshData meshData)
        {
            var triangles = meshData.Triangles;
            triangles.Sort(Comparer);

            var rangeIndex = -1;
            for (int i = 0; i < triangles.Count; i++)
            {
                var triangle = triangles[i];
                if (triangle.Region != rangeIndex)
                {
                    if (i != 0)
                        _ranges[rangeIndex].End = i - 1;

                    rangeIndex = triangle.Region;
                    _ranges[rangeIndex].Start = i;
                }
            }
            _ranges[rangeIndex].End = triangles.Count - 1;
        }

        /// <summary> Adds triagnle data to index. </summary>
        public void AddTriangle(MeshTriangle triangle)
        {
            var p0 = triangle.Vertex0;
            var p1 = triangle.Vertex1;
            var p2 = triangle.Vertex2;
            var centroid = new MapPoint((p0.X + p1.X + p2.X) / 3, (p0.Y + p1.Y + p2.Y) / 3);
            var i = (int)Math.Floor((centroid.X - _x) / _xAxisStep);
            var j = (int)Math.Floor((centroid.Y - _y) / _yAxisStep);

            triangle.Region = _columnCount * j + i;
        }

        /// <summary> Returns list of afected indecies from triangle collection. </summary>
        public List<int> GetAfectedIndices(MapPoint center, float radius)
        {
            var result = new List<int>(32);

            var x = (int)Math.Floor((center.X - _x) / _xAxisStep);
            var y = (int)Math.Floor((center.Y - _y) / _yAxisStep);

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
            return result;
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