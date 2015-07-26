using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    /// <summary>
    ///     Implements the Douglas Peucker algorithim to reduce the number of points
    /// </summary>
    internal class DouglasPeuckerReduction
    {
        /// <summary>
        ///     Reduces the number of points
        /// </summary>
        public static void Reduce(List<Vector2d> source, List<Vector2d> destination, Double tolerance, IObjectPool objectPool)
        {
            if (source == null || source.Count < 3)
            {
                destination.AddRange(source);
                return;
            }

            Int32 firstPoint = 0;
            Int32 lastPoint = source.Count - 1;
            var pointIndexsToKeep = objectPool.NewList<int>(128);

            //Add the first and last index to the keepers
            pointIndexsToKeep.Add(firstPoint);
            pointIndexsToKeep.Add(lastPoint);

            //The first and the last point can not be the same
            while (source[firstPoint].Equals(source[lastPoint]))
                lastPoint--;

            Reduce(source, firstPoint, lastPoint, tolerance, pointIndexsToKeep);

            pointIndexsToKeep.Sort();

            for (int i = 0; i < pointIndexsToKeep.Count; i++)
            {
                var index = pointIndexsToKeep[i];
                // NOTE do not add items twice due to bug in implementation
                if (i > 0 && pointIndexsToKeep[i - 1] == pointIndexsToKeep[i])
                    continue;
                destination.Add(source[index]);
            }
            objectPool.StoreList(pointIndexsToKeep);
        }

        /// <summary>
        ///     Douglases the peucker reduction.
        /// </summary>
        private static void Reduce(List<Vector2d> source, Int32 firstPoint, Int32 lastPoint, Double tolerance, List<int> pointIndexsToKeep)
        {
            Double maxDistance = 0;
            Int32 indexFarthest = 0;

            for (Int32 index = firstPoint; index < lastPoint; index++)
            {
                Double distance = PerpendicularDistance(source[firstPoint], source[lastPoint], source[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

         
            if (maxDistance > tolerance && indexFarthest != 0
                // NOTE second rule is perventing stack overflow due to bug in implementation
                && pointIndexsToKeep.Count <= source.Count) 
            {
                //Add the largest point that exceeds the tolerance
                pointIndexsToKeep.Add(indexFarthest);

                Reduce(source, firstPoint, indexFarthest, tolerance, pointIndexsToKeep);
                Reduce(source, indexFarthest, lastPoint, tolerance, pointIndexsToKeep);
            }
        }

        /// <summary>
        ///     The distance of a point from a line made from point1 and point2.
        /// </summary>
        public static Double PerpendicularDistance(Vector2d Point1, Vector2d Point2, Vector2d Point)
        {
            Double area =
                Math.Abs(.5*
                         (Point1.X*Point2.Y + Point2.X*Point.Y + Point.X*Point1.Y - Point2.X*Point1.Y - Point.X*Point2.Y -
                          Point1.X*Point.Y));
            Double bottom = Math.Sqrt(Math.Pow(Point1.X - Point2.X, 2) + Math.Pow(Point1.Y - Point2.Y, 2));
            Double height = area/bottom*2;

            return height;
        }
    }
}