using System;
using System.Collections.Generic;
using System.IO;
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
            const float degreePerPoint = 20;
            var connections = junction.Connections;
            // sort connections by angle between them and MapPoint(int.MaxValue, 0) clockwise
            connections.Sort((c1, c2) =>
            {
                var c1Point = c1.Start == junction ? c1.Points[1] : c1.Points[c1.Points.Count -2];
                var c2Point = c2.Start == junction ? c2.Points[1] : c2.Points[c2.Points.Count - 2];
                return GeometryUtils.GetTurnAngle(ComparePoint, junction.Center, c1Point).CompareTo(
                       GeometryUtils.GetTurnAngle(ComparePoint, junction.Center, c2Point));
            });

            // NOTE actually, capacity depends on degreePerPoint and angle between adjusted road elements
            var polygon = new List<MapPoint>(connections.Count * 7);
            // now we want to build junction polygon and truncate road elements to fit to it.

            MapPoint firstElementPoint = default(MapPoint);
            var lastElementIndex = connections.Count - 1;
            for (int i = 0; i < connections.Count; i++)
            {

                // get two adjusted segments
                var e1 = connections[i];
                var e2 = connections[i != lastElementIndex ? i + 1 : 0];

                // the same segment is straight line
                //if (e1.Id == e2.Id) 
                //    continue;

                bool e1IsStart = e1.Start == junction;
                bool e2IsStart = e2.Start == junction;

                // detect next to center point
                var e1Point = e1IsStart ? e1.Points[1] : e1.Points[e1.Points.Count - 2];
                var e2Point = e2IsStart ? e2.Points[1] : e2.Points[e2.Points.Count - 2];

                // store first element point as element will be truncated, but it's used twice
                if (i == 0) firstElementPoint = e1Point;

                // clockwise sorted: so should use e1's left segment and e2's right one
                var segment1 = GeometryUtils.GetOffsetLine(e1Point, junction.Center, e1.Width / 2, true);
                var segment2 = GeometryUtils.GetOffsetLine(i != lastElementIndex? e2Point: firstElementPoint, junction.Center, e1.Width / 2, false);


                if (GeometryUtils.IsOnLine(junction.Center, e1Point, e2Point))
                {
                    
                }

                // get polygon
                var polygonPart = GeometryUtils.CreateRoundedCorner(junction.Center, segment1.Start, segment2.Start,
                    e1.Width > e2.Width ? e1.Width : e2.Width, degreePerPoint, objectPool);

                polygon.AddRange(polygonPart);

                // truncate road element points to junction
                
                TruncateToPoint(polygonPart[0], e1.Points, e1IsStart, objectPool);
                //TruncateToPoint(polygonPart[polygonPart.Count -1], e2.Points, e2IsStart, objectPool);

                // TODO generate traffic light positions here?
            }

            junction.Polygon = polygon;

            /*using (var writer = new StreamWriter(new FileStream("mappoint.txt", FileMode.Create)))
            {
                foreach (var mapPoint in junction.Polygon)
                {
                    writer.WriteLine("new MapPoint({0}f, {1}f),", mapPoint.X, mapPoint.Y);
                }
            }*/

            return junction;
        }

        internal static MapPoint TruncateToPoint(MapPoint point, List<MapPoint> points, bool fromStart, IObjectPool objectPool)
        {
            // TODO first arg should be converted to normal to points line!
            var indexBuffer = objectPool.NewList<int>(4);
            
            float distance;
            int count = points.Count;
            int increment = fromStart ? 1 : -1;
            int index = fromStart ? 0 : count - 1;

            var projection = GeometryUtils.GetClosestPointOnLine(point, points[0], points[count - 1]);

            var threshold = projection.DistanceTo(fromStart ? points[0] : points[count - 1]);
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
            if (indexBuffer.Count == 0)
            {
                // We don't want to have list which contains two equal points
                if (points[fromStart ? 1 : count - 2] != truncPoint)
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

            // Do not truncate to one point
            // TODO however it can lead to case when points are equal
            if (count == points.Count - 1) 
                count--;

            points.RemoveRange(index, count);
        }
    }
}