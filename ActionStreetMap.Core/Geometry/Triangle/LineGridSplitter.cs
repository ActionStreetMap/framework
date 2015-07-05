using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Geometry.Triangle
{
    /// <summary> Splits line to segments according to axis alignet grid. </summary>
    internal struct LineGridSplitter
    {
        //private readonly float _minDistance;
        private readonly int _cellSize;
        private readonly int _roundDigitCount;

        private static readonly Comparison<Point> SortX = (a, b) => a.X.CompareTo(b.X);
        private static readonly Comparison<Point> ReverseSortX = (a, b) => -1 * a.X.CompareTo(b.X);

        private static readonly Comparison<Point> SortY = (a, b) => a.Y.CompareTo(b.Y);
        private static readonly Comparison<Point> ReverseSortY = (a, b) => -1 * a.Y.CompareTo(b.Y);

        /// <summary> Creates instance of <see cref="LineGridSplitter"/>. </summary>
        /// <param name="cellSize">Grid cell size.</param>
        /// <param name="roundDigitCount">Round digits</param>
        public LineGridSplitter(int cellSize, int roundDigitCount)
        {
            _cellSize = cellSize;
            _roundDigitCount = roundDigitCount;
        }

        /// <summary> Splits line to segments. </summary>
        public void Split(Point s, Point e, IObjectPool objectPool, List<Point> result)
        {
            var start = new Point(s.X, s.Y);
            var end = new Point(e.X, e.Y);

            var points = objectPool.NewList<Point>();
            points.Add(s);

            double slope = (e.Y - s.Y) * 1.0 / (e.X - s.X);

            if (double.IsInfinity(slope) || Math.Abs(slope) < double.Epsilon)
                ZeroSlope(s, e, points);
            else
                NormalCase(start, end, slope, points);

            result.AddRange(points);
            objectPool.StoreList(points);
        }

        private void NormalCase(Point start, Point end, double slope, List<Point> points)
        {
            var isLeftRight = start.X < end.X;
            var isBottomTop = start.Y < end.Y;

            double inverseSlope = 1 / slope;

            if (!isLeftRight)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            var xStart = (int)Math.Ceiling(start.X / _cellSize) * _cellSize;
            var xEnd = (int)Math.Floor(end.X / _cellSize) * _cellSize;
            for (int x = xStart; x <= xEnd; x += _cellSize)
                points.Add(new Point(x, Math.Round((slope * (x - start.X) + start.Y), _roundDigitCount)));

            if (!isBottomTop)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            var yStart = (int)Math.Ceiling(start.Y / _cellSize) * _cellSize;
            var yEnd = (int)Math.Floor(end.Y / _cellSize) * _cellSize;
            for (int y = yStart; y <= yEnd; y += _cellSize)
                points.Add(new Point(Math.Round((inverseSlope * (y - start.Y) + start.X), _roundDigitCount), y));

            points.Sort(isLeftRight ? SortX : ReverseSortX);
        }

        private void ZeroSlope(Point start, Point end, List<Point> points)
        {
            if (Math.Abs(start.X - end.X) < double.Epsilon)
            {
                var isBottomTop = start.Y < end.Y;
                if (!isBottomTop)
                {
                    var tmp = start;
                    start = end;
                    end = tmp;
                }

                var yStart = (int)Math.Ceiling(start.Y / _cellSize) * _cellSize;
                var yEnd = (int)Math.Floor(end.Y / _cellSize) * _cellSize;
                for (int y = yStart; y <= yEnd; y += _cellSize)
                    points.Add(new Point(start.X, y));

                points.Sort(isBottomTop ? SortY : ReverseSortY);
            }
            else
            {
                var isLeftRight = start.X < end.X;
                if (!isLeftRight)
                {
                    var tmp = start;
                    start = end;
                    end = tmp;
                }

                var xStart = (int)Math.Ceiling(start.X / _cellSize) * _cellSize;
                var xEnd = (int)Math.Floor(end.X / _cellSize) * _cellSize;
                for (int x = xStart; x <= xEnd; x += _cellSize)
                    points.Add(new Point(x, start.Y));

                points.Sort(isLeftRight ? SortX : ReverseSortX);
            }
        }
    }
}
