using System;
using System.Linq;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Core.Elevation
{
    /// <summary>
    ///     Defines behavior of heightmap provider.
    /// </summary>
    public interface IHeightMapProvider
    {
        /// <summary>
        ///     Returns heightmap array for given center with given resolution/
        /// </summary>
        HeightMap Get(Tile tile, int resolution);

        /// <summary>
        ///     Store heightmap in object pool to reuse in next call.
        /// </summary>
        /// <param name="heightMap">Heightmap.</param>
        void Store(HeightMap heightMap);
    }

    /// <summary>
    ///     Default realization of heightmap provider.
    /// </summary>
    public class HeightMapProvider: IHeightMapProvider, IConfigurable
    {
        private const float MaxHeight = 8000;

        private readonly IElevationProvider _elevationProvider;

        private bool _isFlat = false;
        private float[,] _map;

        /// <summary>
        ///     Creates HeightMapProvider.
        /// </summary>
        /// <param name="elevationProvider">Elevation provider.</param>
        [Dependency]
        public HeightMapProvider(IElevationProvider elevationProvider)
        {
            _elevationProvider = elevationProvider;
        }

        /// <inheritdoc />
        public HeightMap Get(Tile tile, int resolution)
        {
            // NOTE so far we do not expect resolution change without restarting app
            if (_map == null)
                _map = new float[resolution, resolution];

            var bbox = tile.BoundingBox;

            float maxElevation;
            float minElevation;

            if (!_isFlat) 
                BuildElevationMap(bbox, resolution, out minElevation, out maxElevation);
            else 
                BuildFlatMap(resolution, out minElevation, out maxElevation);

            return new HeightMap
            {
                LeftBottomCorner = tile.BottomLeft,
                RightUpperCorner = tile.TopRight,
                AxisOffset = tile.Size / resolution,
                IsFlat = _isFlat,
                Size = tile.Size,
                Data = _map,
                MinElevation = minElevation,
                MaxElevation = maxElevation,
                Resolution = resolution,
            };
        }

        /// <inheritdoc />
        public void Store(HeightMap heightMap)
        {
            Array.Clear(_map, 0, _map.Length);
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _isFlat = configSection.GetBool("flat", false);
        }

        #region Private members

        private void BuildElevationMap(BoundingBox bbox, int resolution, out float minElevation, out float maxElevation)
        {
            // NOTE Assume that [0,0] is bottom left corner
            var latStep = (bbox.MaxPoint.Latitude - bbox.MinPoint.Latitude) / resolution;
            var lonStep = (bbox.MaxPoint.Longitude - bbox.MinPoint.Longitude) / resolution;
            var startLat = bbox.MinPoint.Latitude + latStep / 2;
            var minPointLon = bbox.MinPoint.Longitude + lonStep / 2;
            var contexts = _map.Parallel((start, end) =>
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

                        _map[j, i] = elevation;
                        lon += lonStep;
                    }
                }
                return context;
            });
            minElevation = contexts.Min(c => c.MinElevation);
            maxElevation = contexts.Max(c => c.MaxElevation);
        }

        private void BuildFlatMap(int resolution, out float minElevation, out float maxElevation)
        {
            // TODO make them configurable
            maxElevation = 30;
            minElevation = 10;
            var middleValue = (maxElevation + minElevation) / 2;
            _map.Parallel((start, end) =>
            {
                for (int j = start; j < end; j++)
                    for (int i = 0; i < resolution; i++)
                        _map[j, i] = middleValue;
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
