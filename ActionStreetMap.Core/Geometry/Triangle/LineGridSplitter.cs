using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Geometry.Triangle
{
    /// <summary> Splits line to segments according to axis alignet grid. </summary>
    internal class LineGridSplitter
    {
        private const int CellSize = 1;

        private static readonly Comparison<Point> SortX = (a, b) => a.X.CompareTo(b.X);
        private static readonly Comparison<Point> ReverseSortX = (a, b) => -1 * a.X.CompareTo(b.X);

        private static readonly Comparison<Point> SortY = (a, b) => a.Y.CompareTo(b.Y);
        private static readonly Comparison<Point> ReverseSortY = (a, b) => -1 * a.Y.CompareTo(b.Y);

        /// <summary> Splits line to segments. </summary>
        public void Split(Point s, Point e, IObjectPool objectPool, List<Point> result)
        {
            var start = new Point(s.X, s.Y);
            var end = new Point(e.X, e.Y);

            var points = objectPool.NewList<Point>();
            points.Add(s);

            double slope = (e.Y - s.Y) / (e.X - s.X);

            if (double.IsInfinity(slope) || Math.Abs(slope) < double.Epsilon)
                ZeroSlope(s, e, points);
            else
                NormalCase(start, end, slope, points);

            MergeResults(points, result);

            objectPool.StoreList(points);
        }

        private void NormalCase(Point start, Point end, double slope, List<Point> points)
        {
            var isLeftRight = start.X < end.X;

            double inverseSlope = 1 / slope;

            var b = start.Y - slope * start.X;

            if (!isLeftRight)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            var xStart = (int)Math.Ceiling(start.X);
            var xEnd = (int)Math.Floor(end.X);
            for (int x = xStart; x <= xEnd; x += CellSize)
                points.Add(new Point(x, Math.Round(slope * x + b, MathUtils.RoundDigitCount)));

            var isBottomTop = start.Y < end.Y;

            if (!isBottomTop)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            var yStart = (int)Math.Ceiling(start.Y);
            var yEnd = (int)Math.Floor(end.Y);
            for (int y = yStart; y <= yEnd; y += CellSize)
                points.Add(new Point(Math.Round((y - b) * inverseSlope, MathUtils.RoundDigitCount), y));

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

                var yStart = (int)Math.Ceiling(start.Y);
                var yEnd = (int)Math.Floor(end.Y);
                for (int y = yStart; y <= yEnd; y += CellSize)
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

                var xStart = (int)Math.Ceiling(start.X);
                var xEnd = (int)Math.Floor(end.X);
                for (int x = xStart; x <= xEnd; x += CellSize)
                    points.Add(new Point(x, start.Y));

                points.Sort(isLeftRight ? SortX : ReverseSortX);
            }
        }

        private void MergeResults(List<Point> points, List<Point> result)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var candidate = points[i];
                if (result.Any())
                {
                    var last = result[result.Count - 1];
                    var distance = Math.Sqrt((last.X - candidate.X) * (last.X - candidate.X) +
                                             (last.Y - candidate.Y) * (last.Y - candidate.Y));
                    if ((Math.Abs(distance) < MathUtils.Epsion))
                        continue;
                }

                result.Add(candidate);
            }
        }
    }
}