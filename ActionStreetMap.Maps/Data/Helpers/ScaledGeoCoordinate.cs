using ActionStreetMap.Core;

namespace ActionStreetMap.Maps.Data.Helpers
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
            Latitude = (int)(coordinate.Latitude * MapConsts.ScaleFactor);
            Longitude = (int)(coordinate.Longitude * MapConsts.ScaleFactor);
        }

        public GeoCoordinate Unscale()
        {
            double latitude = ((double)Latitude) / MapConsts.ScaleFactor;
            double longitude = ((double)Longitude) / MapConsts.ScaleFactor;
            return new GeoCoordinate(latitude, longitude);
        }
    }
}
