using System;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Provides road junction utility methods.
    /// </summary>
    internal static class RoadJunctionUtils
    {
        private static readonly List<int> Buffer = new List<int>(4);

        /// <summary>
        ///     Gets point along AB at given distance from A and modifies points list to use it.
        ///     width value should not be big in order not to affect direction.
        /// </summary>
        public static MapPoint TruncateToDistance(List<MapPoint> points, float width, bool fromFirst)
        {
            float distance = 0;
            int increment;
            int index;
            int count = points.Count;
            if (fromFirst) { index = 0; increment = 1; }
            else { index = count - 1; increment = -1; }

            MapPoint a = points[index];
            MapPoint b;
            do
            {
                index += increment;
                b = points[index];
                distance = a.DistanceTo(b);
                if (distance >= width)
                    break;
                // NOTE actually, this can be replaced with simple RemoveAt(index), but
                // this operation takes O(n) as it includes array copying at every interation
                // and I want to avoid this
                Buffer.Add(index);

            } while (--count > 1);

            distance = distance < width ? distance : width;

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

            if (Buffer.Count == 0)
                points[index] = truncPoint;
            else if (Buffer.Count == 1)
                points[Buffer[0]] = truncPoint;
            else
            {
                // now need to remove skipped items
                var length = Buffer.Count;
                var firstIndex = fromFirst ? Buffer[0] : Buffer[length - 1];
                points.RemoveRange(firstIndex, length - 1);
                points[firstIndex] = truncPoint;
            }

            Buffer.Clear();
            return truncPoint;
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
            {
                return 360 - angleDiff;
            }
            return -angleDiff; //I need the results to be always positive so flip sign
        }
    }
}