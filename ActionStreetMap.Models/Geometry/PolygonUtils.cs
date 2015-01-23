using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Models.Geometry.Polygons;
using ActionStreetMap.Models.Geometry.Primitives;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Models.Geometry
{
    /// <summary>
    ///     Provides some polygon util methods.
    /// </summary>
    public static class PolygonUtils
    {
        /// <summary>
        ///     Triangulates given polygon.
        /// </summary>
        /// <param name="points">Points which represents polygon.</param>
        /// <param name="objectPool">Object pool.</param>
        /// <param name="reverse">Reverse points.</param>
        /// <returns>Triangles.</returns>
        public static int[] Triangulate(List<MapPoint> points, IObjectPool objectPool, bool reverse = true)
        {
            var indices = objectPool.NewList<int>();
            var result = Triangulator.Triangulate(points, indices, reverse);
            objectPool.StoreList(indices);
            return result;
        }

        /// <summary>
        ///     Cuts given polygon by tile corners.
        /// </summary>
        /// <param name="leftBottom">Left bottom corner.</param>
        /// <param name="rightUpper">Right upper corner.</param>
        /// <param name="points">Closed polygon points.</param>
        public static void ClipPolygonByTile(MapPoint leftBottom, MapPoint rightUpper, List<MapPoint> points)
        {
            // TODO Optimize memory allocations for lists
            var result = PolygonClipper.GetIntersectedPolygon(points,
                new List<MapPoint>()
                {
                    leftBottom,
                    new MapPoint(leftBottom.X, rightUpper.Y),
                    rightUpper,
                    new MapPoint(rightUpper.X, leftBottom.Y),
                });
            points.Clear();
            points.AddRange(result);
        }

        /// <summary>
        ///     Cuts given polygon by tile corners.
        /// </summary>
        /// <param name="heightmap">Height map.</param>
        /// <param name="points">Closed polygon points.</param>
        public static void ClipPolygonByTile(HeightMap heightmap, List<MapPoint> points)
        {
            ClipPolygonByTile(heightmap.LeftBottomCorner, heightmap.RightUpperCorner, points);
            // set height
            for (int i = 0; i < points.Count; i++)
                points[i].SetElevation(heightmap.LookupHeight(points[i]));
        }

        /// <summary>
        ///     Make offset polygon
        /// </summary>
        /// <param name="verticies">Source polygon.</param>
        /// <param name="result">Result polygon.</param>
        /// <param name="offset">Offset.</param>
        public static void MakeOffset(List<MapPoint> verticies, List<MapPoint> result, float offset)
        {
            var polygon = new Polygon(verticies);
            for (int i = 0; i < polygon.Segments.Length; i++)
            {
                var previous = i == 0 ? polygon.Segments.Length - 1 : i - 1;

                var segment1 = polygon.Segments[previous];
                var segment2 = polygon.Segments[i];

                var parallel1 = SegmentUtils.GetParallel(segment1, offset);
                var parallel2 = SegmentUtils.GetParallel(segment2, offset);

                var ip1 = SegmentUtils.IntersectionPoint(parallel1, parallel2);

                // NOTE: Looks like a bug in this or verticies producer algorithm
                if (!float.IsNaN(ip1.x) && !float.IsNaN(ip1.z))
                    result.Add(new MapPoint(ip1.x, ip1.z));
            }
        }

        /// <summary>
        ///     Calcs center of polygon.
        /// </summary>
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

        /// <summary>
        ///     Simplifies polygon by removing all vertices which have an angle close to 180°
        ///     (i.e. where removing the vertex does not change the polygon very much).
        /// </summary>
        public static void Simplify(List<MapPoint> source, List<MapPoint> destination)
        {
            var count = source.Count;
            bool[] delete = new bool[source.Count];
            int deleteCount = 0;
            for (int i = 0; i < count; i++)
            {
                MapPoint segmentBefore = source[i] - source[Math.Abs(i - 1)%count];
                MapPoint segmentAfter = source[(i + 1)%count] - source[i];

                double dot = segmentBefore.Normalize().Dot(segmentAfter.Normalize());
                if (Math.Abs(dot - 1) < 0.05)
                {
                    delete[i] = true;
                    deleteCount += 1;
                }
            }

            destination.AddRange(source);
            if (deleteCount == 0 || deleteCount > count - 3)
                return;

            for (int i = count - 1; i >= 0; i--)
            {
                if (delete[i])
                    destination.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Simplifies polygon using Douglas Peucker algorithim.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="tolerance">Tolerance.</param>
        /// <param name="objectPool"></param>
        public static void Simplify(List<MapPoint> source, List<MapPoint> destination, float tolerance, 
            IObjectPool objectPool)
        {
            DouglasPeuckerReduction.Reduce(source, destination, tolerance, objectPool);
        }
    }
}
