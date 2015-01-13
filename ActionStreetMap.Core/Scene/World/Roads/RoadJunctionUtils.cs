using System;
using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Provides road junction utility methods.
    /// </summary>
    internal static class RoadJunctionUtils
    {
        private static readonly List<int> IndexBuffer = new List<int>(4);

        /// <summary>
        ///     Generates polygon for given road junction and modifies connected road elements.
        /// </summary>
        /// <param name="junction">Road junction.</param>
        public static RoadJunction CompleteJunction(RoadJunction junction)
        {
            // TODO use thread pool
            List<MapPoint> junctionPolygon = new List<MapPoint>(8);
            foreach (var connection in junction.Connections)
            {
                // TODO merge these two methods: TruncateToJoinPoint and GetJoinSegment?
                TruncateToJoinPoint(connection.Points, connection.Width, connection.Start == junction);
                var segment = GetJoinSegment(connection.Points, connection.Width, connection.Start == junction);
                junctionPolygon.Add(segment.Start);
                junctionPolygon.Add(segment.End);
            }
            SortByAngle(junctionPolygon[0], junction.Center, junctionPolygon);
            junction.Polygon = junctionPolygon;

            return junction;
        }

        /// <summary>
        ///     Gets point along AB at given distance from A and modifies/truncates points list to use it.
        ///     threshold value should not be big in order not to affect direction.
        /// </summary>
        internal static MapPoint TruncateToJoinPoint(List<MapPoint> points, float threshold, bool fromFirst)
        {
            float distance;
            int count = points.Count;
            int increment = fromFirst ? 1 : -1;
            int index = fromFirst ? 0 : count - 1;

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
                IndexBuffer.Add(index);

            } while (--count > 1);

            distance = distance < threshold ? distance : threshold;

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
            if (IndexBuffer.Count == 0)
            {
                points[index] = truncPoint;
                TruncatePoints(points, fromFirst ? 0 : count - 1, 1);
            }
            else if (IndexBuffer.Count == 1)
            {
                points[IndexBuffer[0]] = truncPoint;
                TruncatePoints(points, IndexBuffer[0] - increment, 1);
            }
            else
            {
                // now need to remove skipped items and replace trunc point
                var length = IndexBuffer.Count;
                var firstIndex = (fromFirst ? IndexBuffer[0] - increment : IndexBuffer[length - 1]);
                TruncatePoints(points, firstIndex, length);
                points[firstIndex] = truncPoint;
            }
            IndexBuffer.Clear();
            return truncPoint;
        }

        /// <summary>
        ///     Truncates list.
        /// </summary>
        internal static void TruncatePoints(List<MapPoint> points, int index, int count)
        {
            if (points.Count <= 2) return;
            // TODO check if points list is truncated to one or zero point count.
            points.RemoveRange(index, count);
        }

        /// <summary>
        ///     Gets join segment orthogonal to last two points of road element's points.
        /// </summary>
        internal static Segment GetJoinSegment(List<MapPoint> points, float width, bool fromFirst)
        {
            var count = points.Count;
            int increment = fromFirst ? 1 : -1;
            int index = fromFirst ? 0 : count - 1;
            var halfWidth = width/2;

            var a = points[index];
            var b = points[index + increment];

            float l = (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
            var dX = halfWidth * (b.Y - a.Y) / l;
            var dY = halfWidth * (a.X - b.X) / l;

            return new Segment(new MapPoint(a.X + dX, a.Y + dY, a.Elevation), 
                new MapPoint(a.X - dX, a.Y - dY, a.Elevation));
        }

        /// <summary>
        ///     Sorts segments by angle.
        /// </summary>
        /// <param name="original">Original point.</param>
        /// <param name="pivot">Pivot point.</param>
        /// <param name="choices">List of choice points.</param>
        /// <returns>Sorted list.</returns>
        internal static void SortByAngle(MapPoint original, MapPoint pivot, List<MapPoint> choices)
        {
            choices.Sort((v1, v2) => GetTurnAngle(original, pivot, v1).CompareTo(GetTurnAngle(original, pivot, v2)));
        }

        /// <summary>
        ///     Gets angle between sigments created by points.
        /// </summary>
        internal static double GetTurnAngle(MapPoint original, MapPoint pivot, MapPoint choice)
        {
            var angle1 = Math.Atan2(original.Y - pivot.Y, original.X - pivot.X);
            var angle2 = Math.Atan2(choice.Y - pivot.Y, choice.X - pivot.X);
            var angleDiff = (180 / Math.PI * (angle2 - angle1));

            return angleDiff > 0 ? 360 - angleDiff : -angleDiff;
        }

        #region Helpers

        internal struct Segment
        {
            public MapPoint Start;
            public MapPoint End;

            public Segment(MapPoint start, MapPoint end)
            {
                Start = start;
                End = end;
            }
        }

        #endregion
    }
}