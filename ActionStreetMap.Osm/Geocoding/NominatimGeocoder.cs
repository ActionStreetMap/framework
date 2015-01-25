using System;
using System.Globalization;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Osm.GeoCoding;

namespace ActionStreetMap.Osm.Geocoding
{
    /// <summary> Geocoder which uses osm nominatim. </summary>
    public class NominatimGeocoder: IGeocoder, IConfigurable
    {
        private const string DefaultServer = @"http://nominatim.openstreetmap.org/search";
        private string _searchPath = DefaultServer;
        private string _urlSchema = "q={0}&viewbox={1}&format=json";

        public IObservable<GeocoderResult> Search(string name, BoundingBox area)
        {
            // TODO can be optimized with StringBuilder
            string bounds = string.Format(CultureInfo.InvariantCulture,
                "{0:f4},{1:f4},{2:f4},{3:f4}",
                area.MinPoint.Longitude,
                area.MinPoint.Latitude,
                area.MaxPoint.Longitude,
                area.MaxPoint.Latitude);

            var queryString = String.Format(@"{0}?{1}", _searchPath,
                String.Format(_urlSchema, Uri.EscapeDataString(name), Uri.EscapeDataString(bounds)));

            return ObservableWWW.Get(queryString)
                .ObserveOn(Scheduler.ThreadPool)
                .SelectMany(r =>
                {
                    var jsonArray = JSON.Parse(r).AsArray;
                    return (from JSONNode json in jsonArray 
                            select ParseGeocoderResult(json));
                });
        }

        private GeocoderResult ParseGeocoderResult(JSONNode resultNode)
        {
            BoundingBox bbox = null;
            string[] bboxArray = resultNode["boundingbox"].Value.Split(',');
            if (bboxArray.Length == 4)
            {
                bbox = new BoundingBox(ParseGeoCoordinate(bboxArray[0], bboxArray[2]), 
                    ParseGeoCoordinate(bboxArray[1], bboxArray[3]));
            }

            return new GeocoderResult()
            {
                PlaceId = long.Parse(resultNode["place_id"].Value),
                OsmId = long.Parse(resultNode["osm_id"].Value),
                OsmType = resultNode["osm_type"].Value,
                DisplayName = resultNode["display_name"].Value,
                Class = resultNode["class"].Value,
                Type = resultNode["type"].Value,
                Coordinate = ParseGeoCoordinate(resultNode["lat"].Value, resultNode["lon"].Value),
                BoundginBox = bbox,
            };
        }

        private static GeoCoordinate ParseGeoCoordinate(string latStr, string lonStr)
        {
            double latitude, longitude;
            if (double.TryParse(latStr, out latitude) && double.TryParse(lonStr, out longitude))
            {
                return new GeoCoordinate(latitude, longitude);
            }
            return default(GeoCoordinate);
        }

        public void Configure(IConfigSection configSection)
        {
            _searchPath = configSection.GetString("geocoding", DefaultServer);
        }
    }
}
