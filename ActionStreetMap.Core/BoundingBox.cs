using System;
using ActionStreetMap.Core.Utilities;

namespace ActionStreetMap.Core
{
    /// <summary>
    ///     Represents bounding box. 
    ///     See details: http://stackoverflow.com/questions/238260/how-to-calculate-the-bounding-box-for-a-given-lat-lng-location
    /// </summary>
    public class BoundingBox
    {
        /// <summary> Gets or sets point with minimal latitude and longitude. </summary>
        public GeoCoordinate MinPoint { get; set; }

        /// <summary> Gets or sets point with maximum latitude and longitude. </summary>
        public GeoCoordinate MaxPoint { get; set; }

        /// <summary> Creates bounding box from given min and max points. </summary>
        /// <param name="minPoint">Point with minimal latitude and longitude</param>
        /// <param name="maxPoint">Point with maximum latitude and longitude</param>
        public BoundingBox(GeoCoordinate minPoint, GeoCoordinate maxPoint)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
            Size = GeoProjection.Distance(minPoint, maxPoint)/Math.Sqrt(2);
        }

        /// <summary> Gets size of bounding box. Assume that it's created as square. </summary>
        public double Size { get; private set; }

        #region Operations

        /// <summary> Adds point to bounding boxes together yielding as result the smallest box that surrounds both. </summary>
        public static BoundingBox operator +(BoundingBox a, GeoCoordinate b)
        {
            var minPoint = new GeoCoordinate(
                a.MinPoint.Latitude < b.Latitude ? a.MinPoint.Latitude : b.Latitude,
                a.MinPoint.Longitude < b.Longitude ? a.MinPoint.Longitude : b.Longitude);

            var maxPoint = new GeoCoordinate(
                a.MaxPoint.Latitude > b.Latitude ? a.MaxPoint.Latitude : b.Latitude,
                a.MaxPoint.Longitude > b.Longitude ? a.MaxPoint.Longitude : b.Longitude);

            return new BoundingBox(minPoint, maxPoint);
        }

        /// <summary> Adds bounding box to current. </summary>
        public static BoundingBox operator +(BoundingBox a, BoundingBox b)
        {
            var minLat = a.MinPoint.Latitude < b.MinPoint.Latitude ? a.MinPoint.Latitude : b.MinPoint.Latitude;
            var minLon = a.MinPoint.Longitude < b.MinPoint.Longitude ? a.MinPoint.Longitude : b.MinPoint.Longitude;

            var maxLat = a.MaxPoint.Latitude > b.MaxPoint.Latitude ? a.MaxPoint.Latitude : b.MaxPoint.Latitude;
            var maxLon = a.MaxPoint.Longitude > b.MaxPoint.Longitude ? a.MaxPoint.Longitude : b.MaxPoint.Longitude;

            return new BoundingBox(new GeoCoordinate(minLat, minLon), new GeoCoordinate(maxLat, maxLon));
        }

        #endregion

        # region Creation

        /// <summary> Creates bounding box as rectangle. </summary>
        /// <param name="center">Center point.</param>
        /// <param name="width">Width in meters.</param>
        /// <param name="height">Heigh in meters.</param>
        public static BoundingBox CreateBoundingBox(GeoCoordinate center, float width, float height)
        {
            // Bounding box surrounding the point at given coordinates,
            // assuming local approximation of Earth surface as a sphere
            // of radius given by WGS84
            var lat = MathUtility.Deg2Rad(center.Latitude);
            var lon = MathUtility.Deg2Rad(center.Longitude);

            // Radius of Earth at given latitude
            var radius = GeoProjection.WGS84EarthRadius(lat);
            // Radius of the parallel at given latitude
            var pradius = radius * Math.Cos(lat);

            var dWidth = width/(2*radius);
            var dHeight = height / (2 * pradius);

            var latMin = lat - dWidth;
            var latMax = lat + dWidth;
            var lonMin = lon - dHeight;
            var lonMax = lon + dHeight;

            return new BoundingBox(
                new GeoCoordinate(MathUtility.Rad2Deg(latMin), MathUtility.Rad2Deg(lonMin)),
                new GeoCoordinate(MathUtility.Rad2Deg(latMax), MathUtility.Rad2Deg(lonMax)));
        }

        /// <summary> Creates bounding box as square. </summary>
        /// <param name="center">Center.</param>
        /// <param name="sideInMeters">Length of the bounding box.</param>
        public static BoundingBox CreateBoundingBox(GeoCoordinate center, float sideInMeters)
        {
            return CreateBoundingBox(center, sideInMeters, sideInMeters);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0},{1}", MinPoint, MaxPoint);
        }
    }
}