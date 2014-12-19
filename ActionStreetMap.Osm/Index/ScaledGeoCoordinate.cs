using ActionStreetMap.Core;

namespace ActionStreetMap.Osm.Index
{
    public struct ScaledGeoCoordinate
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
            Latitude = (int)(coordinate.Latitude * Utils.ScaleFactor);
            Longitude = (int)(coordinate.Longitude * Utils.ScaleFactor);
        }

        public GeoCoordinate Unscale()
        {
            double latitude = ((double)Latitude) / Utils.ScaleFactor;
            double longitude = ((double)Longitude) / Utils.ScaleFactor;
            return new GeoCoordinate(latitude, longitude);
        }
    }
}
