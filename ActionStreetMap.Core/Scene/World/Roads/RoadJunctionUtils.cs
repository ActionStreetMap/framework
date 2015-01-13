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
        ///     Gets point along AB at given distance from A and modifies/truncates points list to use it.
        ///     threshold value should not be big in order not to affect direction.
        /// </summary>
        public static MapPoint SetJoinPoint(List<MapPoint> points, float threshold, bool fromFirst)
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

            // TODO should remove join point as well
            var truncPoint = new MapPoint(a.X + vectorX, a.Y + vectorY, a.Elevation);
            if (IndexBuffer.Count == 0)
                points[index] = truncPoint;
            else if (IndexBuffer.Count == 1)
                points[IndexBuffer[0]] = truncPoint;
            else
            {
                // now need to remove skipped items
                var length = IndexBuffer.Count;
                var firstIndex = fromFirst ? IndexBuffer[0] : IndexBuffer[length - 1];
                points.RemoveRange(firstIndex, length - 1);
                points[firstIndex] = truncPoint;
            }
            IndexBuffer.Clear();
            return truncPoint;
        }

        /// <summary>
        ///     Gets join segment orthogonal to last two points of road element's points.
        /// </summary>
        public static Segment GetJoinSegment(List<MapPoint> points, float width, bool fromFirst)
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
        ///     Generates polygon for given road junction.
        /// </summary>
        /// <param name="junction">Road junction.</param>
        public static void GeneratePolygon(RoadJunction junction)
        {
        }

        /// <summary>
        ///     Sorts segments by angle.
        /// </summary>
        /// <param name="original">Original point.</param>
        /// <param name="pivot">Pivot point.</param>
        /// <param name="choices">List of choice points.</param>
        /// <returns>Sorted list.</returns>
        public static IEnumerable<MapPoint> SortByAngle(MapPoint original, MapPoint pivot, List<MapPoint> choices)
        {
            choices.Sort((v1, v2) => GetTurnAngle(original, pivot, v1).CompareTo(GetTurnAngle(original, pivot, v2)));
            return choices;
        }

        /// <summary>
        ///     Gets angle between sigments created by points.
        /// </summary>
        public static double GetTurnAngle(MapPoint original, MapPoint pivot, MapPoint choice)
        {
            var angle1 = Math.Atan2(original.Y - pivot.Y, original.X - pivot.X);
            var angle2 = Math.Atan2(choice.Y - pivot.Y, choice.X - pivot.X);
            var angleDiff = (180 / Math.PI * (angle2 - angle1));

            if (angleDiff > 0) //It went CCW so adjust
                return 360 - angleDiff;

            return -angleDiff; //I need the results to be always positive so flip sign
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