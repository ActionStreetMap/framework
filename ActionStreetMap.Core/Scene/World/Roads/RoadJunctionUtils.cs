
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
    }
}
;