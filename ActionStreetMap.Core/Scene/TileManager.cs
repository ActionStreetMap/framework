using System;
using System.Linq;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Scene
{
    /// <summary>
    ///     This class listens to position changes and manages tile processing
    /// </summary>
    public class TileManager : IPositionListener, IConfigurable
    {
        /// <summary>
        ///     Maximum of loaded tiles including non-active
        /// </summary>
        private const int TileCacheSize = 4;

        /// <summary>
        ///     Max index distance in 2d space
        /// </summary>
        private const int ThresholdIndex = 4;

        private float _tileSize;
        private float _offset;
        private float _moveSensitivity;
        private int _heightmapsize;
        private bool _allowAutoRemoval;
        private MapPoint _lastUpdatePosition = new MapPoint(float.MinValue, float.MinValue);

        private readonly Tuple<int, int> _currentTileIndex = new Tuple<int, int>(0, 0);

        private readonly ITileLoader _tileLoader;
        private readonly IMessageBus _messageBus;
        private readonly IHeightMapProvider _heightMapProvider;
        private readonly ITileActivator _tileActivator;

        private readonly DoubleKeyDictionary<int, int, Tile> _allTiles = new DoubleKeyDictionary<int, int, Tile>();
        private readonly DoubleKeyDictionary<int, int, Tile> _activeTiles = new DoubleKeyDictionary<int, int, Tile>();

        /// <inheritdoc />
        public GeoCoordinate CurrentPosition { get; private set; }

        /// <inheritdoc />
        public MapPoint CurrentPoint { get; private set; }

        /// <summary>
        ///     Gets relative null point
        /// </summary>
        public GeoCoordinate RelativeNullPoint { get; set; }

        /// <summary>
        ///     Gets current tile.
        /// </summary>
        public Tile Current { get { return _allTiles[_currentTileIndex.Item1, _currentTileIndex.Item2]; } }

        /// <summary>
        ///     Gets all tile count.
        /// </summary>
        public int Count { get { return _allTiles.Count(); } }

        /// <summary>
        ///     Creats TileManager.
        /// </summary>
        /// <param name="tileLoader">Tile loeader.</param>
        /// <param name="heightMapProvider">Heightmap provider.</param>
        /// <param name="tileActivator">Tile activator.</param>
        /// <param name="messageBus">Message bus.</param>
        [Dependency]
        public TileManager(ITileLoader tileLoader, IHeightMapProvider heightMapProvider, 
            ITileActivator tileActivator, IMessageBus messageBus)
        {
            _tileLoader = tileLoader;
            _messageBus = messageBus;
            _heightMapProvider = heightMapProvider;
            _tileActivator = tileActivator;
        }

        /// <inheritdoc />
        public void OnMapPositionChanged(MapPoint position)
        {
            CurrentPoint = position;
            CurrentPosition = GeoProjection.ToGeoCoordinate(RelativeNullPoint, position);

            // call update logic only if threshold is reached
            if (Math.Abs(position.X - _lastUpdatePosition.X) > _moveSensitivity
                || Math.Abs(position.Y - _lastUpdatePosition.Y) > _moveSensitivity)
            {
                _lastUpdatePosition = position;

                int i = Convert.ToInt32(position.X / _tileSize);
                int j = Convert.ToInt32(position.Y / _tileSize);

                // TODO support setting of neighbors for Unity Terrain

                // NOTE it should be happened only once on start with (0,0)
                // however it's possible if we skip offset detection zone somehow
                if (!_allTiles.ContainsKey(i, j))
                    CreateTile(i, j);

                var tile = _allTiles[i, j];

                if (ShouldPreload(tile, position))
                    PreloadNextTile(tile, position, i, j);

                _currentTileIndex.Item1 = i;
                _currentTileIndex.Item2 = j;
            }
        }

        /// <inheritdoc />
        public void OnGeoPositionChanged(GeoCoordinate position)
        {
            CurrentPosition = position;
            OnMapPositionChanged(GeoProjection.ToMapCoordinate(RelativeNullPoint, position));
        }

        #region Activation

        private void Activate(int i, int j)
        {
            if (_activeTiles.ContainsKey(i, j))
                return;

            var tile = _allTiles[i, j];
            _tileActivator.Activate(tile);
            _activeTiles.Add(i, j, tile);
            _messageBus.Send(new TileActivateMessage(tile));
        }

        private void Deactivate(int i, int j)
        {
            if (!_activeTiles.ContainsKey(i, j))
                return;

            var tile = _activeTiles[i, j];
            _tileActivator.Deactivate(tile);
            _activeTiles.Remove(i, j);
            _messageBus.Send(new TileDeactivateMessage(tile));
        }

        #endregion

        #region Create/Destroy tile

        private void CreateTile(int i, int j)
        {
            var tileCenter = new MapPoint(i*_tileSize, j*_tileSize);

            _messageBus.Send(new TileLoadStartMessage(tileCenter));

            var tile = new Tile(RelativeNullPoint, tileCenter, _tileSize);
            tile.HeightMap = _heightMapProvider.Get(tile, _heightmapsize);
            _tileLoader.Load(tile);

            _messageBus.Send(new TileLoadFinishMessage(tile));

            _allTiles.Add(i, j, tile);

            Activate(i, j);          
        }

        private void Destroy(int i, int j)
        {
            var tile = _allTiles[i, j];
            _tileActivator.Destroy(tile);
            _allTiles.Remove(i, j);
            _messageBus.Send(new TileDestroyMessage(tile));
            if (_activeTiles.ContainsKey(i, j))
                throw new AlgorithmException(Strings.TileDeactivationBug);
        }

        #endregion

        #region Preload

        private bool ShouldPreload(Tile tile, MapPoint position)
        {
            return !tile.Contains(position, _offset);
        }

        private void PreloadNextTile(Tile tile, MapPoint position, int i, int j)
        {
            var index = GetNextTileIndex(tile, position, i, j);
            if (!_allTiles.ContainsKey(index.Item1, index.Item2))
                CreateTile(index.Item1, index.Item2);

            Activate(i, j);

            // NOTE We destroy tiles which are far away from us
            if (_allowAutoRemoval && _allTiles.Count() > TileCacheSize)
            {
                foreach (var doubleKeyPairValue in _allTiles.ToList())
                {
                    if(Math.Abs(doubleKeyPairValue.Key1 - i) + 
                        Math.Abs(doubleKeyPairValue.Key2 - j) > ThresholdIndex)
                        Destroy(doubleKeyPairValue.Key1, doubleKeyPairValue.Key2);
                }
            }
        }

        /// <summary>
        ///     Gets next tile index. Also calls deactivate for tile which is adjusted from opposite site
        /// </summary>
        private Tuple<int, int> GetNextTileIndex(Tile tile, MapPoint position, int i, int j)
        {
            // top
            if (GeometryUtils.IsPointInTreangle(position, tile.MapCenter, tile.TopLeft, tile.TopRight))
            {
                Deactivate(i, j - 1);
                Deactivate(i - 1, j - 1);
                Deactivate(i + 1, j - 1);
                return new Tuple<int, int>(i, j + 1);
            }

            // left
            if (GeometryUtils.IsPointInTreangle(position, tile.MapCenter, tile.TopLeft, tile.BottomLeft))
            {
                Deactivate(i + 1, j);
                Deactivate(i + 1, j + 1);
                Deactivate(i + 1, j - 1);
                return new Tuple<int, int>(i - 1, j);
            }

            // right
            if (GeometryUtils.IsPointInTreangle(position, tile.MapCenter, tile.TopRight, tile.BottomRight))
            {
                Deactivate(i - 1, j);
                Deactivate(i - 1, j + 1);
                Deactivate(i - 1, j - 1);
                return new Tuple<int, int>(i + 1, j);
            }

            // bottom
            Deactivate(i, j + 1);
            Deactivate(i - 1, j + 1);
            Deactivate(i + 1, j + 1);
            return new Tuple<int, int>(i, j - 1);
        }

        #endregion

        #region IConfigurable

        /// <summary>
        ///     Configures class
        /// </summary>
        public void Configure(IConfigSection configSection)
        {
            _tileSize = configSection.GetFloat("size");
            _offset = configSection.GetFloat("offset");
            _moveSensitivity = configSection.GetFloat("sensitivity", 10);
            _heightmapsize = configSection.GetInt("heightmap");

            RelativeNullPoint = new GeoCoordinate(
              configSection.GetFloat(@"position/latitude"),
              configSection.GetFloat(@"position/longitude"));

            _allowAutoRemoval = configSection.GetBool("autoclean", true);
        }

        #endregion
    }
}
