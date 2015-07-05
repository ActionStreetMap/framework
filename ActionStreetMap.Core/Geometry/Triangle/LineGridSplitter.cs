using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Geometry.Triangle
{
    /// <summary> Splits line to segments according to axis alignet grid. </summary>
    internal sealed class LineGridSplitter
    {
        //private readonly float _minDistance;
        private readonly int _cellSize;
        private readonly int _roundDigitCount;
        private static readonly Comparison<Point> Reverse = (a, b) => -1 * a.X.CompareTo(b.X);

        /// <summary> Creates instance of <see cref="LineGridSplitter"/>. </summary>
        /// <param name="cellSize">Grid cell size.</param>
        /// <param name="roundDigitCount">Round digits</param>
        public LineGridSplitter(int cellSize, int roundDigitCount)
        {
            _cellSize = cellSize;
            _roundDigitCount = roundDigitCount;
            //_minDistance = cellSize/2f;
        }

        /// <summary> Splits line to segments. </summary>
        public void Split(Point s, Point e, IObjectPool objectPool, List<Point> result)
        {
            var isLeftRight = s.X < e.X;
            var isBottomTop = s.Y < e.Y;

            var start = new Point(s.X, s.Y);
            var end = new Point(e.X, e.Y);
            var points = objectPool.NewList<Point>();
            points.Add(s);

            if (!isLeftRight)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            double slope = (e.Y - s.Y)*1.0/(e.X - s.X);

            if (double.IsInfinity(slope) || Math.Abs(slope) < double.Epsilon)
                ZeroSlope(s, e, points);
            else
            {
                double inverseSlope = 1/slope;

                var xStart = (int) Math.Ceiling(start.X/_cellSize)*_cellSize;
                var xEnd = (int) Math.Floor(end.X/_cellSize)*_cellSize;
                for (int x = xStart; x <= xEnd; x += _cellSize)
                    points.Add(new Point(x, Math.Round((slope * (x - start.X) + start.Y)), _roundDigitCount));

                if (!isBottomTop)
                {
                    var tmp = start;
                    start = end;
                    end = tmp;
                }

                var yStart = (int) Math.Ceiling(start.Y/_cellSize)*_cellSize;
                var yEnd = (int) Math.Floor(end.Y/_cellSize)*_cellSize;
                for (int y = yStart; y <= yEnd; y += _cellSize)
                    points.Add(new Point(Math.Round((inverseSlope*(y - start.Y) + start.X), _roundDigitCount), y));

                if (isLeftRight) points.Sort();
                else points.Sort(Reverse);
            }

            MergeLists(points, result);
            objectPool.StoreList(points);
        }

        private void ZeroSlope(Point s, Point e, List<Point> points)
        {
            // TODO
            //throw new NotImplementedException();
        }

        private void MergeLists(List<Point> points, List<Point> result)
        {
            result.Add(points[0]);
            //result.AddRange(points);
            //return;
            /*if (points.Count == 1)
                result.AddRange(points);
            else if (points.Count > 1)
            {
                // NOTE do not add point which is close to previous
                for (int i = 0; i < points.Count; i++)
                {
                    var candidate = points[i];
                    if (!result.Any() || i == 0)
                    {
                        result.Add(candidate);
                        continue;
                    }

                    var last = result[result.Count - 1];
                    if (Math.Sqrt(last.X - candidate.X) * (last.X - candidate.X) +
                        (last.Y - candidate.Y) * (last.Y - candidate.Y) > 0.001)
                        result.Add(candidate);
                }
            }*/
        }
    }
}
