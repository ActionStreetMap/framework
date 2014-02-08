﻿using Mercraft.Models.Primitives;
using Mercraft.Models.Units.Angle;
using Mercraft.Models.Units.Distance;

namespace Mercraft.Maps.Core
{
    /// <summary>
    /// Represents a standard geo coordinate.
    /// 
    /// 0: longitude.
    /// 1: latitude.
    /// </summary>
    public class GeoCoordinate : PointF2D
    {
        public const double RadiusOfEarth = 6371000;

        /// <summary>
        /// Creates a geo coordinate.
        /// </summary>
        public GeoCoordinate(double[] values)
            :base(values)
        {

        }

        /// <summary>
        /// Creates a geo coordinate.
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        public GeoCoordinate(double latitude,double longitude)
            :base(new double[]{longitude,latitude})
        {

        }

        #region Properties

        /// <summary>
        /// Gets/Sets the longitude.
        /// </summary>
        public double Longitude
        {
            get
            {
                return this[0];
            }
            //set
            //{
            //    this[0] = value;
            //}
        }

        /// <summary>
        /// Gets/Sets the latitude.
        /// </summary>
        public double Latitude
        {
            get
            {
                return this[1];
            }
            //set
            //{
            //    this[1] = value;
            //}
        }

        #endregion

        #region Calculations

        /// <summary>
        /// Calculates the distance between this point and the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double Distance(GeoCoordinate point)
        {
            return PointF2D.Distance(this, point);
        }

        /// <summary>
        /// Estimates the distance between this point and the given point in meters.
        /// Accuracy decreases with distance.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Meter DistanceEstimate(GeoCoordinate point)
        {
            Meter radius_earth = RadiusOfEarth;

            double lat1_rad = (this.Latitude / 180d) * System.Math.PI;
            double lon1_rad = (this.Longitude / 180d) * System.Math.PI;
            double lat2_rad = (point.Latitude / 180d) * System.Math.PI;
            double lon2_rad = (point.Longitude / 180d) * System.Math.PI;

            double x = (lon2_rad - lon1_rad) * System.Math.Cos((lat1_rad + lat2_rad) / 2.0);
            double y = lat2_rad - lat1_rad;

            double m = System.Math.Sqrt(x * x + y * y) * radius_earth.Value;

            return m;
        }

        /// <summary>
        /// Calculates the real distance in meters between this point and the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <remarks>http://en.wikipedia.org/wiki/Haversine_formula</remarks>
        public Meter DistanceReal(GeoCoordinate point)
        {
            Meter radius_earth = RadiusOfEarth;

            Radian lat1_rad = new Degree(this.Latitude);
            Radian lon1_rad = new Degree(this.Longitude);
            Radian lat2_rad = new Degree(point.Latitude);
            Radian lon2_rad = new Degree(point.Longitude);

            double dLat = (lat2_rad - lat1_rad).Value;
            double dLon = (lon2_rad - lon1_rad).Value;

            double a = System.Math.Pow(System.Math.Sin(dLat / 2), 2) +
                       System.Math.Cos(lat1_rad.Value) * System.Math.Cos(lat2_rad.Value) *
                       System.Math.Pow(System.Math.Sin(dLon / 2), 2);

            double c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));

            double distance = radius_earth.Value * c;

            return distance;
        }

        /// <summary>
        /// Offset this coordinate with the given distance in meter in both lat-lon directions.
        /// The difference in distance will be sqrt(2) * meter.
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        public GeoCoordinate OffsetWithDistances(Meter meter)
        {
            GeoCoordinate offsetLat = new GeoCoordinate(this.Latitude + 0.1,
                this.Longitude);
            GeoCoordinate offsetLon = new GeoCoordinate(this.Latitude,
                this.Longitude + 0.1);
            Meter latDistance = offsetLat.DistanceReal(this);
            Meter lonDistance = offsetLon.DistanceReal(this);

            return new GeoCoordinate(this.Latitude + (meter.Value / latDistance.Value) * 0.1,
                this.Longitude + (meter.Value / lonDistance.Value) * 0.1);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Adds two geo coordinates.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static GeoCoordinate operator +(GeoCoordinate a, GeoCoordinate b)
        {
            double[] c = new double[2];

            for (int idx = 0; idx < 2; idx++)
            {
                c[idx] = a[idx] + b[idx];
            }

            return new GeoCoordinate(c);
        }

        /// <summary>
        /// Divides the given geo coordinate with the given value.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static GeoCoordinate operator /(GeoCoordinate a, double value)
        {
            double[] c = new double[2];

            for (int idx = 0; idx < 2; idx++)
            {
                c[idx] = a[idx] / value;
            }

            return new GeoCoordinate(c);
        }

        #endregion

        /// <summary>
        /// Returns a description of this coordinate.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[{0},{1}]",
                this.Latitude,
                this.Longitude);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Latitude.GetHashCode() ^
                   this.Longitude.GetHashCode();
        }

        /// <summary>
        /// Returns true if both objects are equal in value.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is GeoCoordinate)
            {
                return (obj as GeoCoordinate).Latitude.Equals(this.Latitude) &&
                       (obj as GeoCoordinate).Longitude.Equals(this.Longitude);
            }
            return false;
        }
    }
}