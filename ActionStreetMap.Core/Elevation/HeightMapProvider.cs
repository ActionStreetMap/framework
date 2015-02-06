using System;
using System.Linq;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Core.Elevation
{
    /// <summary> Defines behavior of heightmap provider. </summary>
    public interface IHeightMapProvider
    {
        /// <summary> Returns heightmap array for given center with given resolution. </summary>
        HeightMap Get(Tile tile, int resolution);

        /// <summary> Store heightmap in object pool to reuse in next call. </summary>
        /// <param name="heightMap">Heightmap.</param>
        void Store(HeightMap heightMap);
    }

    /// <summary> Default realization of heightmap provider. </summary>
    public class HeightMapProvider: IHeightMapProvider, IConfigurable
    {
        private const string LogTag = "mapdata.ele";

        private const float MaxHeight = 8000;

        private readonly IElevationProvider _elevationProvider;
        private readonly IObjectPool _objectPool;

        private bool _isFlat;
        private bool _autoDownload;

        /// <summary> Trace. </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates HeightMapProvider. </summary>
        /// <param name="elevationProvider">Elevation provider.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public HeightMapProvider(IElevationProvider elevationProvider, IObjectPool objectPool)
        {
            _elevationProvider = elevationProvider;
            _objectPool = objectPool;
        }

        /// <inheritdoc />
        public HeightMap Get(Tile tile, int resolution)
        {
            var map = _objectPool.NewArray<float>(resolution, resolution);
            var bbox = tile.BoundingBox;

            // NOTE force to be flat if no elevation tile is defined
            // TODO actually, this doesn't handle properly all the cases 
            // (e.g. different integer part of latitude for given location)
            var tileGeoCenter = GeoProjection.ToGeoCoordinate(tile.RelativeNullPoint, tile.MapCenter);

            bool hasData = _elevationProvider.HasElevation(tileGeoCenter.Latitude, tileGeoCenter.Longitude);
            if (!hasData && _autoDownload)
            {
                Trace.Warn(LogTag, "no elevation data found for {0}", tileGeoCenter);
                _elevationProvider.Download(tileGeoCenter.Latitude, tileGeoCenter.Longitude).Wait();
                hasData = _elevationProvider.HasElevation(tileGeoCenter.Latitude, tileGeoCenter.Longitude);
            }

            var isFlat = _isFlat && !hasData;

            float maxElevation;
            float minElevation;

            if (!isFlat)
                BuildElevationMap(bbox, map, resolution, out minElevation, out maxElevation);
            else
                BuildFlatMap(map, resolution, out minElevation, out maxElevation);

            Trace.Info(LogTag, "elevation mode is flat: {0}", isFlat);
            return new HeightMap
            {
                LeftBottomCorner = tile.BottomLeft,
                RightUpperCorner = tile.TopRight,
                AxisOffset = tile.Size / resolution,
                IsFlat = _isFlat,
                Size = tile.Size,
                Data = map,
                MinElevation = minElevation,
                MaxElevation = maxElevation,
                Resolution = resolution,
            };
        }

        /// <inheritdoc />
        public void Store(HeightMap heightMap)
        {
            _objectPool.StoreArray(heightMap.Data);
            heightMap.Data = null;
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _isFlat = configSection.GetBool("flat", false);
            _autoDownload = configSection.GetBool("download", true);
        }

        #region Private members

        private void BuildElevationMap(BoundingBox bbox, float[,] map, int resolution, out float minElevation, out float maxElevation)
        {
            // NOTE Assume that [0,0] is bottom left corner
            var latStep = (bbox.MaxPoint.Latitude - bbox.MinPoint.Latitude) / resolution;
            var lonStep = (bbox.MaxPoint.Longitude - bbox.MinPoint.Longitude) / resolution;
            var startLat = bbox.MinPoint.Latitude + latStep / 2;
            var minPointLon = bbox.MinPoint.Longitude + lonStep / 2;
            var contexts = map.Parallel((start, end) =>
            {
                var context = new ElevationContext();
                for (int j = start; j < end; j++)
                {
                    var lat = startLat + j * latStep;
                    var lon = minPointLon;
                    for (int i = 0; i < resolution; i++)
                    {
                        var elevation = _elevationProvider.GetElevation(lat, lon);
                        if (elevation > context.MaxElevation)
                        {
                            if (elevation < MaxHeight) context.MaxElevation = elevation;
                            else elevation = context.MaxElevation;
                        }
                        else if (elevation < context.MinElevation)
                            context.MinElevation = elevation;

                        map[j, i] = elevation;
                        lon += lonStep;
                    }
                }
                return context;
            });
            minElevation = contexts.Min(c => c.MinElevation);
            maxElevation = contexts.Max(c => c.MaxElevation);
        }

        private void BuildFlatMap(float[,] map, int resolution, out float minElevation, out float maxElevation)
        {
            // TODO make them configurable
            maxElevation = 30;
            minElevation = 10;
            var middleValue = (maxElevation + minElevation) / 2;
            map.Parallel((start, end) =>
            {
                for (int j = start; j < end; j++)
                    for (int i = 0; i < resolution; i++)
                        map[j, i] = middleValue;
            });
        }

        #endregion

        #region Nested classes

        private class ElevationContext
        {
            public float MaxElevation = -MaxHeight;
            public float MinElevation = MaxHeight;
        }

        #endregion
    }
}
