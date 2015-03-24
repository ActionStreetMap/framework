using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Core
{
    /// <summary> Represents regular geo coordinate with latitude and longitude. </summary>
    public struct GeoCoordinate
    {
        /// <summary> Latitude in degrees. </summary>
        public readonly double Latitude;

        /// <summary> Longitude in degrees. </summary>
        public readonly double Longitude;

        /// <summary> Creates geo coordinate from given latitude and longitude. </summary>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary> Compares two geo coordinates. </summary>
        public static bool operator ==(GeoCoordinate a, GeoCoordinate b)
        {
            return MathUtils.AreEqual(a.Latitude, b.Latitude) &&
                   MathUtils.AreEqual(a.Longitude, b.Longitude);
        }

        /// <summary> Compares two geo coordinates. </summary>
        public static bool operator !=(GeoCoordinate a, GeoCoordinate b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            if (!(other is GeoCoordinate))
                return false;
            var coord = (GeoCoordinate) other;
            return MathUtils.AreEqual(Latitude, coord.Latitude) &&
                   MathUtils.AreEqual(Longitude, coord.Longitude);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode() << 2;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0:F7},{1:F7}", Latitude, Longitude);
        }
    }
}