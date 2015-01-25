using ActionStreetMap.Core;

namespace ActionStreetMap.Maps.Index.Helpers
{
    internal struct ScaledGeoCoordinate
    {
        public int Latitude;
        public int Longitude;

        public ScaledGeoCoordinate(int scaledLatitude, int scaledLongitude)
        {
            Latitude = scaledLatitude;
            Longitude = scaledLongitude;
        }

        public ScaledGeoCoordinate(GeoCoordinate coordinate)
        {
            Latitude = (int)(coordinate.Latitude * Consts.ScaleFactor);
            Longitude = (int)(coordinate.Longitude * Consts.ScaleFactor);
        }

        public GeoCoordinate Unscale()
        {
            double latitude = ((double)Latitude) / Consts.ScaleFactor;
            double longitude = ((double)Longitude) / Consts.ScaleFactor;
            return new GeoCoordinate(latitude, longitude);
        }
    }
}
