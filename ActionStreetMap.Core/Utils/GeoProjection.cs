using System;

namespace ActionStreetMap.Core.Utils
{
    /// <summary>
    ///     Provides the methods to convert geo coordinates to map coordinates and vice versa
    /// </summary>
    public static class GeoProjection
    {
        #region Coordinate convertion

        /// <summary>
        ///     The circumference at the equator (latitude 0)
        /// </summary>
        private const int LatitudeEquator = 40075160;

        /// <summary>
        ///     distance of full circle around the earth through the poles
        /// </summary>
        private const int CircleDistance = 40008000;

        /// <summary>
        ///     Calculates map coordinate from geo coordinate
        ///     see http://stackoverflow.com/questions/3024404/transform-longitude-latitude-into-meters?rq=1
        /// </summary>
        public static MapPoint ToMapCoordinate(GeoCoordinate relativeNullPoint, GeoCoordinate coordinate)
        {
            double deltaLatitude = coordinate.Latitude - relativeNullPoint.Latitude;
            double deltaLongitude = coordinate.Longitude - relativeNullPoint.Longitude;
            double latitudeCircumference = LatitudeEquator*Math.Cos(MathUtils.Deg2Rad(relativeNullPoint.Latitude));
            double resultX = deltaLongitude*latitudeCircumference/360;
            double resultY = deltaLatitude*CircleDistance/360;

            return new MapPoint((float) resultX, (float) resultY);
        }

        /// <summary>
        ///     Calculates geo coordinate from map coordinate. Reverse operation to ToMapCoordinates()
        /// </summary>
        public static GeoCoordinate ToGeoCoordinate(GeoCoordinate relativeNullPoint, MapPoint mapPoint)
        {
            return ToGeoCoordinate(relativeNullPoint, mapPoint.X, mapPoint.Y);
        }

        /// <summary>
        ///     Calculates geo coordinate from map coordinate. Reverse operation to ToMapCoordinates()
        /// </summary>
        public static GeoCoordinate ToGeoCoordinate(GeoCoordinate relativeNullPoint, float x, float y)
        {
            double latitudeCircumference = LatitudeEquator*Math.Cos(MathUtils.Deg2Rad(relativeNullPoint.Latitude));

            var deltaLongitude = (x*360)/latitudeCircumference;
            var deltaLatitude = (y*360)/CircleDistance;

            return new GeoCoordinate(relativeNullPoint.Latitude + deltaLatitude,
                relativeNullPoint.Longitude + deltaLongitude);
        }

        #endregion

        #region Distance specific

        // Semi-axes of WGS-84 geoidal reference
        private const double WGS84_a = 6378137.0; // Major semiaxis [m]
        private const double WGS84_b = 6356752.3; // Minor semiaxis [m]

        /// <summary>
        ///     Calculates distance between two coordinates.
        /// </summary>
        /// <param name="first">First coordinate.</param>
        /// <param name="second">Second coordinate.</param>
        /// <returns>Distance.</returns>
        public static double Distance(GeoCoordinate first, GeoCoordinate second)
        {
            var dLat = MathUtils.Deg2Rad((first.Latitude - second.Latitude));
            var dLon = MathUtils.Deg2Rad((first.Longitude - second.Longitude));

            var lat1 = MathUtils.Deg2Rad(first.Latitude);
            var lat2 = MathUtils.Deg2Rad(second.Latitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var radius = WGS84EarthRadius(dLat);

            return radius * c;
        }

        /// <summary>
        ///     Earth radius at a given latitude, according to the WGS-84 ellipsoid [m]
        /// </summary>
        public static double WGS84EarthRadius(double lat)
        {
            // http://en.wikipedia.org/wiki/Earth_radius
            var an = WGS84_a * WGS84_a * Math.Cos(lat);
            var bn = WGS84_b * WGS84_b * Math.Sin(lat);
            var ad = WGS84_a * Math.Cos(lat);
            var bd = WGS84_b * Math.Sin(lat);
            return Math.Sqrt((an * an + bn * bn) / (ad * ad + bd * bd));
        }

        #endregion
    }
}