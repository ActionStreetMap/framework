using System;
using System.Globalization;
using System.Linq;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.GeoCoding;

namespace ActionStreetMap.Maps.Geocoding
{
    /// <summary> Geocoder which uses osm nominatim. </summary>
    internal class NominatimGeocoder: IGeocoder, IConfigurable
    {
        private const string DefaultServer = @"http://nominatim.openstreetmap.org/search?";
        private string _searchPath = DefaultServer;

        /// <inheritdoc />
        public IObservable<GeocoderResult> Search(string name)
        {
            return Search(name, null);
        }

        /// <inheritdoc />
        public IObservable<GeocoderResult> Search(string name, BoundingBox area)
        {
            var sb = new StringBuilder(128);
            sb.Append(_searchPath);
            if (area != null)
            {
                sb.Append(Uri.EscapeDataString(String.Format(CultureInfo.InvariantCulture,
                    "viewbox={0:f4},{1:f4},{2:f4},{3:f4}&",
                    area.MinPoint.Longitude,
                    area.MinPoint.Latitude,
                    area.MaxPoint.Longitude,
                    area.MaxPoint.Latitude)));
            }
            sb.AppendFormat("q={0}&format=json", Uri.EscapeDataString(name));

            return ObservableWWW.Get(sb.ToString())
                .Take(1)
                .SelectMany(r => (
                    from JSONNode json in JSON.Parse(r).AsArray 
                    select ParseGeocoderResult(json)));
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
                return new GeoCoordinate(latitude, longitude);
            return default(GeoCoordinate);
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _searchPath = configSection.GetString("geocoding", DefaultServer);
        }
    }
}
