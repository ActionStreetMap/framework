using System;
using System.Linq;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Tiling
{
    /// <summary>
    ///     This is workaround for DI container which doesn't support multi interface registrations for 
    ///     one object instance.
    /// </summary>
    internal interface ITilePositionObserver : IPositionObserver<MapPoint>, IPositionObserver<GeoCoordinate>
    {
    }

    /// <summary>
    ///     This class listens to position changes and manages tile processing
    /// </summary>
    public class TileManager : ITilePositionObserver, IConfigurable
    {
        /// <summary>
        ///     Maximum of loaded tiles including non-active
        /// </summary>
        private const int TileCacheSize = 4;

        /// <summary>
        ///     Max index distance in 2d space
        /// </summary>
        private const int ThresholdIndex = 4;

        private readonly object _lockObj = new object();

        private float _tileSize;
        private float _offset;
        private float _moveSensitivity;
        private int _heightmapsize;
        private bool _allowAutoRemoval;
        private MapPoint _lastUpdatePosition = new MapPoint(float.MinValue, float.MinValue);

        private GeoCoordinate _currentPosition;
        private MapPoint _currentMapPoint;

        private readonly MutableTuple<int, int> _currentTileIndex = new MutableTuple<int, int>(0, 0);

        private readonly ITileLoader _tileLoader;
        private readonly IMessageBus _messageBus;
        private readonly IHeightMapProvider _heightMapProvider;
        private readonly ITileActivator _tileActivator;
        private readonly IObjectPool _objectPool;

        private readonly DoubleKeyDictionary<int, int, MutableTuple<Tile, TileState>> _allTiles = 
            new DoubleKeyDictionary<int, int, MutableTuple<Tile, TileState>>();

        /// <summary>
        ///     Gets relative null point
        /// </summary>
        public GeoCoordinate RelativeNullPoint { get; private set; }

        /// <summary>
        ///     Gets current tile.
        /// </summary>
        public Tile Current { get { return _allTiles[_currentTileIndex.Item1, _currentTileIndex.Item2].Item1; } }

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
            ITileActivator tileActivator, IMessageBus messageBus, IObjectPool objectPool)
        {
            _tileLoader = tileLoader;
            _messageBus = messageBus;
            _heightMapProvider = heightMapProvider;
            _tileActivator = tileActivator;
            _objectPool = objectPool;
        }

        #region Activation

        private void Activate(int i, int j)
        {
            var entry = _allTiles[i, j];
            var tile = entry.Item1;
            if (entry.Item2 == TileState.Activated)
                return;

            _tileActivator.Activate(tile);
            entry.Item2 = TileState.Activated;
            _messageBus.Send(new TileActivateMessage(tile));
        }

        private void Deactivate(int i, int j)
        {
            if (!_allTiles.ContainsKey(i, j))
                return;

            var entry = _allTiles[i, j];
            var tile = entry.Item1;

            // NOTE or != Activated ?
            if (entry.Item2 == TileState.Deactivated)
                return;
            _tileActivator.Deactivate(tile);
            entry.Item2 = TileState.Deactivated;
            _messageBus.Send(new TileDeactivateMessage(tile));
        }

        #endregion

        #region Create/Destroy tile

        private void CreateTile(int i, int j)
        {
            var tileCenter = new MapPoint(i*_tileSize, j*_tileSize);

            var tile = new Tile(RelativeNullPoint, tileCenter, new Canvas(_objectPool), _tileSize);
            var entry = new MutableTuple<Tile, TileState>(tile, TileState.IsLoading);

            if (_allTiles.ContainsKey(i, j))
                return;
            _allTiles.Add(i, j, entry);
            _messageBus.Send(new TileLoadStartMessage(tileCenter));
            tile.HeightMap = _heightMapProvider.Get(tile, _heightmapsize);
            _tileLoader.Load(tile).Subscribe(_ => {}, () => 
            {
                lock (_lockObj) { entry.Item2 = TileState.Activated; }
                _messageBus.Send(new TileLoadFinishMessage(tile));
            });
        }

        private void Destroy(int i, int j)
        {
            var entry = _allTiles[i, j];
            if (entry.Item2 != TileState.Deactivated)
                throw new AlgorithmException(Strings.TileDeactivationBug);
            _allTiles.Remove(i, j);
            _tileActivator.Destroy(entry.Item1);
            _messageBus.Send(new TileDestroyMessage(entry.Item1));
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
            else
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
        private MutableTuple<int, int> GetNextTileIndex(Tile tile, MapPoint position, int i, int j)
        {
            // top
            if (GeometryUtils.IsPointInTreangle(position, tile.MapCenter, tile.TopLeft, tile.TopRight))
            {
                Deactivate(i, j - 1);
                Deactivate(i - 1, j - 1);
                Deactivate(i + 1, j - 1);
                return new MutableTuple<int, int>(i, j + 1);
            }

            // left
            if (GeometryUtils.IsPointInTreangle(position, tile.MapCenter, tile.TopLeft, tile.BottomLeft))
            {
                Deactivate(i + 1, j);
                Deactivate(i + 1, j + 1);
                Deactivate(i + 1, j - 1);
                return new MutableTuple<int, int>(i - 1, j);
            }

            // right
            if (GeometryUtils.IsPointInTreangle(position, tile.MapCenter, tile.TopRight, tile.BottomRight))
            {
                Deactivate(i - 1, j);
                Deactivate(i - 1, j + 1);
                Deactivate(i - 1, j - 1);
                return new MutableTuple<int, int>(i + 1, j);
            }

            // bottom
            Deactivate(i, j + 1);
            Deactivate(i - 1, j + 1);
            Deactivate(i + 1, j + 1);
            return new MutableTuple<int, int>(i, j - 1);
        }

        #endregion

        #region IObserver<MapPoint> implementation

        MapPoint IPositionObserver<MapPoint>.Current { get { return _currentMapPoint; } }

        void IObserver<MapPoint>.OnNext(MapPoint value)
        {
            var geoPosition = GeoProjection.ToGeoCoordinate(RelativeNullPoint, value);
            lock (_lockObj)
            {
                _currentMapPoint = value;
                _currentPosition = geoPosition;

                // call update logic only if threshold is reached
                if (Math.Abs(value.X - _lastUpdatePosition.X) > _moveSensitivity
                    || Math.Abs(value.Y - _lastUpdatePosition.Y) > _moveSensitivity)
                {
                    _lastUpdatePosition = value;

                    int i = Convert.ToInt32(value.X / _tileSize);
                    int j = Convert.ToInt32(value.Y / _tileSize);

                    // TODO support setting of neighbors for Unity Terrain

                    bool hasTile = _allTiles.ContainsKey(i, j);
                    if (!hasTile)
                        CreateTile(i, j);

                    var tileEntry = _allTiles[i, j];
                    if (hasTile) 
                        _messageBus.Send(new TileFoundMessage(tileEntry.Item1, _currentMapPoint));

                    if (ShouldPreload(tileEntry.Item1, value))
                        PreloadNextTile(tileEntry.Item1, value, i, j);

                    _currentTileIndex.Item1 = i;
                    _currentTileIndex.Item2 = j;
                }
            }
        }

        void IObserver<MapPoint>.OnError(Exception error) { }
        void IObserver<MapPoint>.OnCompleted() { }

        #endregion

        #region IObserver<GeoCoordinate> implementation

        GeoCoordinate IPositionObserver<GeoCoordinate>.Current { get { return _currentPosition; } }

        void IObserver<GeoCoordinate>.OnNext(GeoCoordinate value)
        {
            if (RelativeNullPoint == default(GeoCoordinate))
                RelativeNullPoint = value;
            _currentPosition = value;

            (this as IPositionObserver<MapPoint>).OnNext(GeoProjection.ToMapCoordinate(RelativeNullPoint, value));
        }

        void IObserver<GeoCoordinate>.OnError(Exception error) { }
        void IObserver<GeoCoordinate>.OnCompleted() { }

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

            _allowAutoRemoval = configSection.GetBool("autoclean", true);
        }

        #endregion

        #region Nested classes

        private enum TileState
        {
            IsLoading,
            Activated,
            Deactivated
        }

        #endregion
    }
}
