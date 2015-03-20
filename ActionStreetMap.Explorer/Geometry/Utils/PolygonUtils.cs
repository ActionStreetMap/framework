using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry.Polygons;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    /// <summary> Provides some polygon util methods. </summary>
    public static class PolygonUtils
    {
        /// <summary> Triangulates given polygon. </summary>
        /// <param name="points">Points which represents polygon.</param>
        /// <param name="objectPool">Object pool.</param>
        /// <param name="reverse">Reverse points.</param>
        /// <returns>Triangles.</returns>
        public static List<int> Triangulate(List<MapPoint> points, IObjectPool objectPool, bool reverse = true)
        {
            var indices = objectPool.NewList<int>();
            Triangulator.Triangulate(points, indices, reverse);
            return indices;
        }

        /// <summary> Calcs center of polygon. </summary>
        /// <param name="polygon">Polygon.</param>
        /// <returns>Center of polygon.</returns>
        public static MapPoint GetCentroid(List<MapPoint> polygon)
        {
            float centroidX = 0.0f;
            float centroidY = 0.0f;

            for (int i = 0; i < polygon.Count; i++)
            {
                centroidX += polygon[i].X;
                centroidY += polygon[i].Y;
            }
            centroidX /= polygon.Count;
            centroidY /= polygon.Count;

            return (new MapPoint(centroidX, centroidY));
        }

        /// <summary> Simplifies polygon using Douglas Peucker algorithim. </summary>
        /// <param name="source">Source.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="tolerance">Tolerance.</param>
        /// <param name="objectPool">Object pool.</param>
        public static void Simplify(List<MapPoint> source, List<MapPoint> destination, float tolerance, 
            IObjectPool objectPool)
        {
            DouglasPeuckerReduction.Reduce(source, destination, tolerance, objectPool);
        }
    }
}
