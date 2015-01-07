
using System;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Scene.World.Roads
{
    /// <summary>
    ///     Provides road junction utility methods.
    /// </summary>
    internal static class RoadJunctionUtils
    {
        /// <summary>
        ///     Gets point along AB at given distance from A.
        /// </summary>
        public static MapPoint CalculateJointPoint(List<MapPoint> points, float width, bool isFirst)
        {
            var count = points.Count;
            MapPoint a;
            MapPoint b;
            if (isFirst)
            {
                a = points[0];
                b = points[1];
            }
            else
            {
                a = points[count - 1];
                b = points[count - 2];
            }

            var abDistance = Math.Max(0.5f, a.DistanceTo(b));
            var distance = abDistance < width? abDistance: width;

            // TODO ensure that generated point has valid direction:
            // AB' + B'B = AB It's possible that "distance" variable is greater than AB 

            // a. calculate the vector from o to g:
            float vectorX = b.X - a.X;
            float vectorY = b.Y - a.Y;

            // b. calculate the proportion of hypotenuse
            var factor = (float)(distance / Math.Sqrt(vectorX * vectorX + vectorY * vectorY));

            // c. factor the lengths
            vectorX *= factor;
            vectorY *= factor;

            // d. calculate and Draw the new vector,
            return new MapPoint(a.X + vectorX, a.Y + vectorY, a.Elevation);
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
;