
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Maps.GeoCoding
{
    /// <summary> Defines reverse geocoder API. </summary>
    public interface IGeocoder
    {
        /// <summary> Performs reverse geocoding for given string. </summary>
        /// <param name="name">Search query.</param>
        /// <returns>Results.</returns>
        IObservable<GeocoderResult> Search(string name);

        /// <summary> Performas reverse geocoding for given string and area. </summary>
        /// <param name="name">Search query.</param>
        /// <param name="area">Bounding box.</param>
        /// <returns>Results.</returns>
        IObservable<GeocoderResult> Search(string name, BoundingBox area);
    }

    /// <summary> Represents geocoding results. </summary>
    public class GeocoderResult
    {
        /// <summary>Gets or sets place id.</summary>
        public long PlaceId { get; set; }

        /// <summary>Gets or sets osm id.</summary>
        public long OsmId { get; set; }

        /// <summary>Gets or sets osm type.</summary>
        public string OsmType { get; set; }

        /// <summary>Gets or sets formatted name of search result.</summary>
        public string DisplayName { get; set; }

        /// <summary>Gets or sets class of object.</summary>
        public string Class { get; set; }

        /// <summary>Gets or sets type of object.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets geo coordinate of search result center.</summary>
        public GeoCoordinate Coordinate { get; set; }

        /// <summary>Gets or sets earch's bounding box.</summary>
        public BoundingBox BoundginBox { get; set; }
    }
}
