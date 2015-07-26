using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    /// <summary> Provides some polygon util methods. </summary>
    public static class PolygonUtils
    {
        /// <summary> Calcs center of polygon. </summary>
        /// <param name="polygon">Polygon.</param>
        /// <returns>Center of polygon.</returns>
        public static Vector2d GetCentroid(List<Vector2d> polygon)
        {
            var centroidX = 0.0;
            var centroidY = 0.0;

            for (int i = 0; i < polygon.Count; i++)
            {
                centroidX += polygon[i].X;
                centroidY += polygon[i].Y;
            }
            centroidX /= polygon.Count;
            centroidY /= polygon.Count;

            return (new Vector2d(centroidX, centroidY));
        }

        /// <summary> Simplifies polygon using Douglas Peucker algorithim. </summary>
        /// <param name="source">Source.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="tolerance">Tolerance.</param>
        /// <param name="objectPool">Object pool.</param>
        public static void Simplify(List<Vector2d> source, List<Vector2d> destination, 
            float tolerance, IObjectPool objectPool)
        {
            DouglasPeuckerReduction.Reduce(source, destination, tolerance, objectPool);
        }
    }
}
