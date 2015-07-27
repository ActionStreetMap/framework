using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Core.Geometry.Utils
{
    /// <summary> Contains some generic geometry utility methods. </summary>
    internal class GeometryUtils
    {
        #region Triangle specific functions

        /// <summary>
        ///     Checks whether point is located in triangle
        ///     http://stackoverflow.com/questions/13300904/determine-whether-point-lies-inside-triangle
        /// </summary>
        public static bool IsPointInTriangle(Vector2d p, Vector2d p1, Vector2d p2, Vector2d p3)
        {
            var alpha = ((p2.Y - p3.Y) * (p.X - p3.X) + (p3.X - p2.X) * (p.Y - p3.Y)) /
                          ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));
            var beta = ((p3.Y - p1.Y) * (p.X - p3.X) + (p1.X - p3.X) * (p.Y - p3.Y)) /
                         ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));
            var gamma = 1.0f - alpha - beta;

            return alpha > 0 && beta > 0 && gamma > 0;
        }

        #endregion

        /// <summary> Checks collision between circle and rectangle. </summary>
        public static bool HasCollision(Vector2d circle, float radius, Rectangle2d rectangle)
        {
            var closestX = MathUtils.Clamp(circle.X, rectangle.Left, rectangle.Right);
            var closestY = MathUtils.Clamp(circle.Y, rectangle.Bottom, rectangle.Top);

            // Calculate the distance between the circle's center and this closest point
            var distanceX = circle.X - closestX;
            var distanceY = circle.Y - closestY;

            // If the distance is less than the circle's radius, an intersection occurs
            var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (radius * radius);
        }
        
        #region Line specific functions

        /// <summary> Divide line into smaller parts. </summary>
        public static void DivideLine(Vector2d start, Vector2d end, List<Vector2d> result, float maxDistance)
        {
            var point1 = start;
            var point2 = end;

            result.Add(point1);

            var distance = point1.DistanceTo(point2);
            while (distance > maxDistance)
            {
                var ration = maxDistance / distance;
                point1 = new Vector2d(point1.X + ration * (point2.X - point1.X),
                    point1.Y + ration * (point2.Y - point1.Y));

                distance = point1.DistanceTo(point2);
                result.Add(point1);
            }

            result.Add(end);
        }

        #endregion
    }
}
