using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Scene.Roads
{
    /// <summary> Provides road junction utility methods. </summary>
    internal static class RoadJunctionUtils
    {
        private static readonly MapPoint ComparePoint = new MapPoint(int.MaxValue, 0);

        /// <summary> Generates polygon for given road junction and modifies connected road elements. </summary>
        /// <param name="junction">Road junction.</param>
        /// <param name="objectPool">Object pool.</param>
        public static RoadJunction Complete(RoadJunction junction, IObjectPool objectPool)
        {
            var connections = junction.Connections;
            var elevation = junction.Center.Elevation;
            // sort connections by angle between them and MapPoint(int.MaxValue, 0) clockwise
            connections.Sort((c1, c2) =>
            {
                var c1Point = c1.Start == junction ? c1.Points[1] : c1.Points[c1.Points.Count -2];
                var c2Point = c2.Start == junction ? c2.Points[1] : c2.Points[c2.Points.Count - 2];
                return GeometryUtils.GetTurnAngle(ComparePoint, junction.Center, c1Point).CompareTo(
                       GeometryUtils.GetTurnAngle(ComparePoint, junction.Center, c2Point));
            });

            var polygon = objectPool.NewList<MapPoint>(connections.Count * 2);           
            MapPoint firstElementPoint = default(MapPoint);
            var lastElementIndex = connections.Count - 1;
            for (int i = 0; i < connections.Count; i++)
            {
                // get two adjusted segments
                var current = connections[i];
                var left = connections[i != lastElementIndex ? i + 1 : 0];
                var right = connections[i != 0 ? i - 1 : lastElementIndex];

                // boolean flags to determine which point should be used from points
                bool currentFromStart = current.Start == junction;
                bool leftFromStart = left.Start == junction;
                bool rightFromStart = right.Start == junction;

                var points = current.Points;

                // detect next to center point
                var currentPoint = currentFromStart ? points[1] : points[points.Count - 2];
                var leftPoint = i != lastElementIndex
                    ? (leftFromStart ? left.Points[1] : left.Points[left.Points.Count - 2])
                    : firstElementPoint;

                var rightPoint = rightFromStart ? right.Points[1] : right.Points[right.Points.Count - 2];

                // store element point of first element as it will be truncated before second use
                if (i == 0) firstElementPoint = currentPoint;

                // get offset segments
                var currentLeftSegment = GeometryUtils.GetOffsetLine(currentPoint, junction.Center, current.Width, true);
                var currentRightSegment = GeometryUtils.GetOffsetLine(currentPoint, junction.Center, current.Width, false);

                var leftSegment = GeometryUtils.GetOffsetLine(junction.Center, leftPoint, left.Width, true);
                var rightSegment = GeometryUtils.GetOffsetLine(junction.Center, rightPoint, left.Width, false);
                
                // get intersection points from left and right
                var leftInsectPoint = GeometryUtils.GetIntersectionPoint(currentLeftSegment, leftSegment, elevation);
                var rightInsectPoint = GeometryUtils.GetIntersectionPoint(currentRightSegment, rightSegment, elevation);

                // detect the farest point of projections
                var leftProjection = GeometryUtils.GetClosestPointOnLine(leftInsectPoint, points[0], points[points.Count - 1]);
                var rightProjection = GeometryUtils.GetClosestPointOnLine(rightInsectPoint, points[0], points[points.Count - 1]);

                // detect truncate point and truncate
                var truncatePoint = leftProjection.DistanceTo(junction.Center) >
                                    rightProjection.DistanceTo(junction.Center)
                    ? leftProjection
                    : rightProjection;
                TruncateToPoint(truncatePoint, points, currentFromStart, objectPool);

                // add points to junction polygon
                var tSegment = GeometryUtils.GetTSegment(points, current.Width, elevation, !currentFromStart);
                polygon.Add(tSegment.Start);
                polygon.Add(tSegment.End);
            }
           
            // sort clockwise
            GeometryUtils.SortByAngle(polygon[0], junction.Center, polygon);

            // copy avoiding duplicates
            junction.Polygon = objectPool.NewList<MapPoint>(polygon.Count);
            for(int i = 0; i < polygon.Count; i++)
                if (i == 0 || polygon[i - 1] != polygon[i])
                    junction.Polygon.Add(polygon[i]);

            objectPool.StoreList(polygon);

            return junction;
        }

        /// <summary> Truncates point list. </summary>
        internal static MapPoint TruncateToPoint(MapPoint point, List<MapPoint> points, bool fromStart, IObjectPool objectPool)
        {
            float distance;
            int count = points.Count;
            int increment = fromStart ? 1 : -1;
            int index = fromStart ? 0 : count - 1;
            var indexBuffer = objectPool.NewList<int>(4);
            var threshold = point.DistanceTo(fromStart ? points[0] : points[count - 1]);
            MapPoint a = points[index];
            MapPoint b;
            do
            {
                index += increment;
                b = points[index];
                distance = a.DistanceTo(b);
                if (distance >= threshold)
                    break;
                // NOTE actually, this can be replaced with simple RemoveAt(index), but
                // this operation takes O(n) as it includes array copying at every interation
                // and I want to avoid this
                indexBuffer.Add(index);

            } while (--count > 1);

            // NOTE distance / 2 prevents truncation to the same point
            distance = distance < threshold ? distance / 2 : threshold;

            // AB' + B'B = AB It's possible that "distance" variable is greater than AB 
            // a. calculate the vector from o to g:
            float vectorX = b.X - a.X;
            float vectorY = b.Y - a.Y;

            // b. calculate the proportion of hypotenuse
            var factor = (float)(distance / Math.Sqrt(vectorX * vectorX + vectorY * vectorY));

            // c. factor the lengths
            vectorX *= factor;
            vectorY *= factor;

            var truncPoint = new MapPoint(a.X + vectorX, a.Y + vectorY, a.Elevation);
            // NOTE this is workaround because of invalid map data: loop in way
            if (float.IsNaN(truncPoint.X) || float.IsNaN(truncPoint.Y))
                return truncPoint;

            if (indexBuffer.Count == 0)
            {
                // We don't want to have list which contains two equal points (ignore elevation)
                var compared = points[fromStart ? 1 : count - 2];
                if (!compared.X.Equals(truncPoint.X) || !compared.Y.Equals(truncPoint.Y))
                    points[fromStart ? 0 : count - 1] = truncPoint;
            }
            else if (indexBuffer.Count == 1)
            {
                points[indexBuffer[0]] = truncPoint;
                TruncatePoints(points, indexBuffer[0] - increment, 1);
            }
            else
            {
                // now need to remove skipped items and replace trunc point
                var length = indexBuffer.Count;
                var firstIndex = (fromStart ? indexBuffer[0] - increment : indexBuffer[length - 1]);
                TruncatePoints(points, firstIndex, length);
                points[firstIndex] = truncPoint;
            }
            objectPool.StoreList(indexBuffer);

            return truncPoint;
        }

        /// <summary> Truncates list. </summary>
        internal static void TruncatePoints(List<MapPoint> points, int index, int count)
        {
            if (points.Count <= 2) return;

            // do not truncate to one point
            // TODO however it can lead to case when points are equal
            if (count == points.Count - 1) 
                count--;

            points.RemoveRange(index, count);
        }
    }
}