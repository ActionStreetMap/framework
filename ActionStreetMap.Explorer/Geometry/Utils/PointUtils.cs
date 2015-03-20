using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Utilities;

namespace ActionStreetMap.Explorer.Geometry.Utils
{
    /// <summary> Provids some helper methods for points. </summary>
    public static class PointUtils
    {
        #region Points for polygons

        /// <summary> Converts geo coordinates to map coordinates without elevation data. </summary>
        /// <param name="center">Map center.</param>
        /// <param name="geoCoordinates">Geo coordinates.</param>
        /// <param name="points">Output points.</param>
        public static void GetClockwisePolygonPoints(GeoCoordinate center, List<GeoCoordinate> geoCoordinates,
            List<MapPoint> points)
        {
            GetPointsNoElevation(center, geoCoordinates, points, true);
        }

        /// <summary> Converts geo coordinates to map coordinates with elevation data. </summary>
        /// <param name="elevationProvider">Elevation provider.</param>
        /// <param name="center">Map center.</param>
        /// <param name="geoCoordinates">Geo coordinates.</param>
        /// <param name="points">Output points.</param>
        public static void GetClockwisePolygonPoints(IElevationProvider elevationProvider, GeoCoordinate center, List<GeoCoordinate> geoCoordinates,
            List<MapPoint> points)
        {
            GetPointsElevation(elevationProvider, center, geoCoordinates, points, true);
        }

        /// <summary>
        ///     Converts geo coordinates to map coordinates without sorting.
        /// </summary>
        /// <param name="center">Map center.</param>
        /// <param name="geoCoordinates">Geo coordinates.</param>
        /// <param name="points">Output points.</param>
        public static void GetPolygonPoints(GeoCoordinate center, List<GeoCoordinate> geoCoordinates,
            List<MapPoint> points)
        {
            GetPointsNoElevation(center, geoCoordinates, points, false);
        }

        /// <summary> Converts geo coordinates to map coordinates with elevation data without sorting. </summary>
        /// <param name="elevationProvider">Elevation provider.</param>
        /// <param name="center">Map center.</param>
        /// <param name="geoCoordinates">Geo coordinates.</param>
        /// <param name="points">Output points.</param>
        public static void GetPolygonPoints(IElevationProvider elevationProvider, GeoCoordinate center, 
            List<GeoCoordinate> geoCoordinates, List<MapPoint> points)
        {
            GetPointsElevation(elevationProvider, center, geoCoordinates, points, false);
        }

        private static void GetPointsNoElevation(GeoCoordinate center, List<GeoCoordinate> geoCoordinates, 
            List<MapPoint> verticies, bool sort)
        {
            var length = geoCoordinates.Count;

            if (geoCoordinates[0] == geoCoordinates[length - 1])
                length--;

            for (int i = 0; i < length; i++)
            {
                // skip the same points in sequence
                if (i == 0 || geoCoordinates[i] != geoCoordinates[i - 1])
                {
                    var point = GeoProjection.ToMapCoordinate(center, geoCoordinates[i]);
                    verticies.Add(point);
                }
            }

            if (sort)
                SortVertices(verticies);
        }

        private static void GetPointsElevation(IElevationProvider elevationProvider, GeoCoordinate center, 
            List<GeoCoordinate> geoCoordinates, List<MapPoint> verticies, bool sort)
        {
            var length = geoCoordinates.Count;
            if (geoCoordinates[0] == geoCoordinates[length - 1])
                length--;

            FillHeight(elevationProvider, center, geoCoordinates, verticies, length);

            if(sort)
                SortVertices(verticies);
        }

        #endregion

        /// <summary> Fills heighmap. </summary>
        /// <param name="elevationProvider">Elevation provider.</param>
        /// <param name="center">Center.</param>
        /// <param name="geoCoordinates">Geo coordinates.</param>
        /// <param name="verticies">Verticies.</param>
        public static void FillHeight(IElevationProvider elevationProvider, GeoCoordinate center, 
            List<GeoCoordinate> geoCoordinates, List<MapPoint> verticies)
        {
            FillHeight(elevationProvider, center, geoCoordinates, verticies, geoCoordinates.Count);
        }

        /// <summary> Fills heighmap. </summary>
        /// <param name="elevationProvider">Elevation provider.</param>
        /// <param name="center">Center.</param>
        /// <param name="geoCoordinates">Geo coordinates.</param>
        /// <param name="verticies">Verticies.</param>
        /// <param name="length">Count of points to be processed.</param>
        public static void FillHeight(IElevationProvider elevationProvider, GeoCoordinate center, List<GeoCoordinate> geoCoordinates,
            List<MapPoint> verticies, int length)
        {
            for (int i = 0; i < length; i++)
            {
                  // skip the same points in sequence
                if (i == 0 || geoCoordinates[i] != geoCoordinates[i - 1])
                {
                    var point = GeoProjection.ToMapCoordinate(center, geoCoordinates[i]);
                    point.Elevation = elevationProvider.GetElevation(point);
                    verticies.Add(point);
                }
            }
        }

        /// <summary> Tests whether points represent convex polygon. </summary>
        /// <param name="points">Polygon points.</param>
        /// <returns>True if polygon is convex.</returns>
        public static bool IsConvex(List<MapPoint> points)
        {
            int count = points.Count;
            if (count < 4)
                return true;
            bool sign = false;
            for (int i = 0; i < count; i++)
            {
                double dx1 = points[(i + 2)%count].X - points[(i + 1)%count].X;
                double dy1 = points[(i + 2)%count].Y - points[(i + 1)%count].Y;
                double dx2 = points[i].X - points[(i + 1)%count].X;
                double dy2 = points[i].Y - points[(i + 1)%count].Y;
                double crossProduct = dx1*dy2 - dy1*dx2;
                if (i == 0)
                    sign = crossProduct > 0;
                else if (sign != (crossProduct > 0))
                    return false;

            }
            return true;
        }

        /// <summary> Sorts verticies in clockwise order. </summary>
        private static void SortVertices(List<MapPoint> verticies)
        {
            var direction = PointsDirection(verticies);

            switch (direction)
            {
                case PolygonDirection.CountClockwise:
                    verticies.Reverse();
                    break;
                case PolygonDirection.Clockwise:
                    break;
                default:
                    throw new AlgorithmException(Strings.BugInPolygonOrderAlgorithm);
            }
        }

        private static PolygonDirection PointsDirection(List<MapPoint> points)
        {
            if (points.Count < 3)
                return PolygonDirection.Unknown;

            // Calculate signed area
            // http://en.wikipedia.org/wiki/Shoelace_formula
            double sum = 0.0;
            for (int i = 0; i < points.Count; i++)
            {
                MapPoint v1 = points[i];
                MapPoint v2 = points[(i + 1) % points.Count];
                sum += (v2.X - v1.X) * (v2.Y + v1.Y);
            }
            return sum > 0.0 ? PolygonDirection.Clockwise : PolygonDirection.CountClockwise;
        }

        internal enum PolygonDirection
        {
            Unknown,
            Clockwise,
            CountClockwise
        }
    }
}
