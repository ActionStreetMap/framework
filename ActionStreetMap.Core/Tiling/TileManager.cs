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
    public interface ITilePositionObserver : IPositionObserver<MapPoint>, IPositionObserver<GeoCoordinate>
    {
    }

    /// <summary> This class listens to position changes and manages tile processing. </summary>
    public class TileManager : ITilePositionObserver, IConfigurable
    {
        private readonly object _lockObj = new object();

        private float _tileSize;
        private float _offset;
        private float _moveSensitivity;
        private float _thresholdDistance;
        private int _heightmapsize;
        private MapPoint _lastUpdatePosition = new MapPoint(float.MinValue, float.MinValue);

        private GeoCoordinate _currentPosition;
        private MapPoint _currentMapPoint;

        private readonly MutableTuple<int, int> _currentTileIndex = new MutableTuple<int, int>(0, 0);

        private readonly ITileLoader _tileLoader;
        private readonly IMessageBus _messageBus;
        private readonly IHeightMapProvider _heightMapProvider;
        private readonly ITileActivator _tileActivator;
        private readonly IObjectPool _objectPool;

        private readonly DoubleKeyDictionary<int, int, Tile> _allTiles = 
            new DoubleKeyDictionary<int, int, Tile>();

        /// <summary> Gets relative null point. </summary>
        public GeoCoordinate RelativeNullPoint { get; private set; }

        /// <summary> Gets current tile. </summary>
        public Tile Current { get { return _allTiles[_currentTileIndex.Item1, _currentTileIndex.Item2]; } }

        /// <summary> Gets all tile count. </summary>
        public int Count { get { return _allTiles.Count(); } }

        /// <summary> Creats <see cref="TileManager"/>. </summary>
        /// <param name="tileLoader">Tile loeader.</param>
        /// <param name="heightMapProvider">Heightmap provider.</param>
        /// <param name="tileActivator">Tile activator.</param>
        /// <param name="messageBus">Message bus.</param>
        /// <param name="objectPool">Object pool.</param>
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

        #region Create/Destroy tile

        private void Create(int i, int j)
        {
            var tileCenter = new MapPoint(i*_tileSize, j*_tileSize);

            var tile = new Tile(RelativeNullPoint, tileCenter, new Canvas(_objectPool), _tileSize, _tileSize);

            if (_allTiles.ContainsKey(i, j))
                return;
            _allTiles.Add(i, j, tile);
            _messageBus.Send(new TileLoadStartMessage(tileCenter));
            // Set activated before load to prevent concurrent issue in case of fast tile switching.
            tile.HeightMap = _heightMapProvider.Get(tile, _heightmapsize);
            _tileLoader.Load(tile).Subscribe(_ => {}, () => _messageBus.Send(new TileLoadFinishMessage(tile)));
        }

        private void Destroy(int i, int j)
        {
            var tile = _allTiles[i, j];
            _allTiles.Remove(i, j);
            _tileActivator.Destroy(tile);
            _messageBus.Send(new TileDestroyMessage(tile));
        }

        #endregion

        #region Preload

        private bool ShouldPreload(Tile tile, MapPoint position)
        {
            return !tile.Contains(position, _offset);
        }

        private void PreloadNextTile(Tile tile, MapPoint position, int i, int j)
        {
            // Let's cleanup old tile first to release memory.
            foreach (var doubleKeyPairValue in _allTiles.ToList())
            {
                var candidateToDie = doubleKeyPairValue.Value;
                if (candidateToDie.MapCenter.DistanceTo(position) >= _thresholdDistance)
                    Destroy(doubleKeyPairValue.Key1, doubleKeyPairValue.Key2);
            }

            var index = GetNextTileIndex(tile, position, i, j);
            if (_allTiles.ContainsKey(index.Item1, index.Item2))
                return;

            Create(index.Item1, index.Item2);
        }

        /// <summary>
        ///     Gets next tile index. Also calls deactivate for tile which is adjusted from opposite site
        /// </summary>
        private MutableTuple<int, int> GetNextTileIndex(Tile tile, MapPoint position, int i, int j)
        {
            // top
            if (GeometryUtils.IsPointInTriangle(position, tile.MapCenter, tile.TopLeft, tile.TopRight))
                return new MutableTuple<int, int>(i, j + 1);
      
            // left
            if (GeometryUtils.IsPointInTriangle(position, tile.MapCenter, tile.TopLeft, tile.BottomLeft))
                return new MutableTuple<int, int>(i - 1, j);

            // right
            if (GeometryUtils.IsPointInTriangle(position, tile.MapCenter, tile.TopRight, tile.BottomRight))
                return new MutableTuple<int, int>(i + 1, j);
 
            // bottom
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
                        Create(i, j);

                    var tile = _allTiles[i, j];
                    if (hasTile) 
                        _messageBus.Send(new TileFoundMessage(tile, _currentMapPoint));

                    if (ShouldPreload(tile, value))
                        PreloadNextTile(tile, value, i, j);

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

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _tileSize = configSection.GetFloat("size", 500);
            _offset = configSection.GetFloat("offset", 50);
            _moveSensitivity = configSection.GetFloat("sensitivity", 10);
            _heightmapsize = configSection.GetInt("heightmap", 1025);

            _thresholdDistance = (float) Math.Sqrt(2)*_tileSize;
        }

        #endregion
    }
}
