using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry.Triangle.Topology;

namespace ActionStreetMap.Explorer.Geometry
{
    internal class TriangleIndex
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

        public TriangleIndex(int columnCount, int rowCount, MapRectangle rectangle)
        {
            _columnCount = columnCount;
            _rowCount = rowCount;
            _x = rectangle.BottomLeft.X;
            _y = rectangle.BottomLeft.Y;

            _bottomLeft = rectangle.BottomLeft;

            _xAxisStep = rectangle.Width/columnCount;
            _yAxisStep = rectangle.Height/rowCount;

            _ranges = new Range[rowCount * columnCount];
        }

        public void BuiltIndex(List<Triangle> triangles)
        {
            triangles.Sort(Comparer);

            var rangeIndex = -1;
            for (int i = 0; i < triangles.Count; i++)
            {
                var triangle = triangles[i];
                if (triangle.region != rangeIndex)
                {
                    if (i != 0)
                        _ranges[rangeIndex].End = i - 1;

                    rangeIndex = triangle.region;
                    _ranges[rangeIndex].Start = i;
                }
            }
            _ranges[rangeIndex].End = triangles.Count - 1;
        }

        public int GetIndexKey(MapPoint point)
        {
            var i = (int)Math.Floor((point.X - _x) / _xAxisStep);
            var j = (int)Math.Floor((point.Y - _y) / _yAxisStep);

            return _columnCount * j + i;
        }

        public List<int> GetAfectedIndecies(MapPoint center, float radius)
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

                    if (HasCollision(center, radius, rectangle))
                        AddRange(i, j, result);
                }
            return result;
        }

        private bool HasCollision(MapPoint circle, float radius, MapRectangle rectangle)
        {
            float closestX = Clamp(circle.X, rectangle.Left, rectangle.Right);
            float closestY = Clamp(circle.Y, rectangle.Bottom, rectangle.Top);

            // Calculate the distance between the circle's center and this closest point
            float distanceX = circle.X - closestX;
            float distanceY = circle.Y - closestY;

            // If the distance is less than the circle's radius, an intersection occurs
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (radius * radius);
        }

        private void AddRange(int i, int j, List<int> result)
        {
            var index = _columnCount*j + i;
            if (index >= _ranges.Length || 
                i >= _columnCount || 
                j >= _rowCount) return;

            var range = _ranges[index];
            result.AddRange(Enumerable.Range(range.Start, range.End - range.Start + 1));
        }

        private static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        #region Nested classes

        private struct Range
        {
            public int Start;
            public int End;
        }

        private class TriangleComparer : IComparer<Triangle>
        {
            public int Compare(Triangle x, Triangle y)
            {
                return x.Region.CompareTo(y.Region);
            }
        }

        #endregion
    }
}