using System;
using ActionStreetMap.Core.Scene.Models;
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
        private const int MaxHeight = 8000;

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
            // resolve height
            if (!_isFlat)
            {
                minElevation = MaxHeight;
                maxElevation = -MaxHeight;
                var latStep = (bbox.MaxPoint.Latitude - bbox.MinPoint.Latitude)/resolution;
                var lonStep = (bbox.MaxPoint.Longitude - bbox.MinPoint.Longitude)/resolution;

                // NOTE Assume that [0,0] is bottom left corner
                var lat = bbox.MinPoint.Latitude + latStep/2;
                for (int j = 0; j < resolution; j++)
                {
                    var lon = bbox.MinPoint.Longitude + lonStep/2;
                    for (int i = 0; i < resolution; i++)
                    {
                        var elevation = _elevationProvider.GetElevation(lat, lon);

                        if (elevation > maxElevation && elevation < MaxHeight)
                            maxElevation = elevation;
                        else if (elevation < minElevation)
                            minElevation = elevation;

                        _map[j, i] = elevation > MaxHeight ? maxElevation : elevation;

                        lon += lonStep;
                    }
                    lat += latStep;
                }
            }
            else
            {
                // NOTE values for non-flat mode
                // TODO make them configurable?
                maxElevation = 30;
                minElevation = 10;
                var middleValue = (maxElevation + minElevation)/2;
                for (int j = 0; j < resolution; j++)
                    for (int i = 0; i < resolution; i++)
                        _map[j, i] = middleValue;
            }

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
    }
}
