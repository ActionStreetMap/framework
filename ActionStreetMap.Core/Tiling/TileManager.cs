using System;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utils;
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
        /// <summary> Gets current scene tile. </summary>
        Tile CurrentTile { get; }
    }

    /// <summary> This class listens to position changes and manages tile processing. </summary>
    public class TileManager : ITilePositionObserver, IConfigurable
    {
        private const int MixedMode = 0;
        private const int SceneMode = 1;
        private const int OverviewMode = 2;

        private readonly object _lockObj = new object();

        private float _tileSize;
        private float _offset;
        private float _moveSensitivity;
        private int _renderModeEx;
        private int _overviewBuffer;
        private float _thresholdDistance;
        private MapPoint _lastUpdatePosition = new MapPoint(float.MinValue, float.MinValue);

        private GeoCoordinate _currentPosition;
        private MapPoint _currentMapPoint;

        private readonly MutableTuple<int, int> _currentSceneTileIndex = new MutableTuple<int, int>(0, 0);

        private readonly ITileLoader _tileLoader;
        private readonly IMessageBus _messageBus;
        private readonly ITileActivator _tileActivator;
        private readonly IObjectPool _objectPool;

        private readonly DoubleKeyDictionary<int, int, Tile> _allSceneTiles = new DoubleKeyDictionary<int, int, Tile>();
        private readonly DoubleKeyDictionary<int, int, Tile> _allOverviewTiles = new DoubleKeyDictionary<int, int, Tile>();

        /// <summary> Gets relative null point. </summary>
        public GeoCoordinate RelativeNullPoint { get; private set; }

        /// <inheritdoc />
        public Tile CurrentTile { get { return _allSceneTiles[_currentSceneTileIndex.Item1, _currentSceneTileIndex.Item2]; } }

        /// <summary> Gets all scene tile count. </summary>
        public int Count { get { return _allSceneTiles.Count(); } }

        /// <summary> Creates <see cref="TileManager"/>. </summary>
        /// <param name="tileLoader">Tile loeader.</param>
        /// <param name="tileActivator">Tile activator.</param>
        /// <param name="messageBus">Message bus.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public TileManager(ITileLoader tileLoader, ITileActivator tileActivator, 
            IMessageBus messageBus, IObjectPool objectPool)
        {
            _tileLoader = tileLoader;
            _messageBus = messageBus;
            _tileActivator = tileActivator;
            _objectPool = objectPool;
        }

        #region Create/Destroy tile

        private void Create(int i, int j)
        {
            if (_renderModeEx != OverviewMode)
                LoadTile(i, j, RenderMode.Scene);

            if (_renderModeEx != SceneMode)
                for (int z = j - _overviewBuffer; z <= j + _overviewBuffer; z++)
                    for (int k = i - _overviewBuffer; k <= i + _overviewBuffer; k++)
                    {
                        if (!_allOverviewTiles.ContainsKey(k, z)) 
                            LoadTile(k, z, RenderMode.Overview);
                   }
        }

        private void LoadTile(int i, int j, RenderMode renderMode)
        {
            if (_allSceneTiles.ContainsKey(i, j))
                return;

            var tileCenter = new MapPoint(i * _tileSize, j * _tileSize);
            var tile = new Tile(RelativeNullPoint, tileCenter, renderMode, new Canvas(_objectPool), _tileSize, _tileSize);

            (renderMode == RenderMode.Overview ? _allOverviewTiles : _allSceneTiles).Add(i, j, tile);

            _messageBus.Send(new TileLoadStartMessage(tileCenter));
            _tileLoader.Load(tile)
                .Subscribe(_ => { }, () =>
                {
                    // should destroy overview tile if we're rendering scene tile on it
                    if (renderMode == RenderMode.Scene && _allOverviewTiles.ContainsKey(i, j))
                        Destroy(i, j, RenderMode.Overview);

                    _messageBus.Send(new TileLoadFinishMessage(tile));
                });
        }

        private void Destroy(int i, int j, RenderMode renderMode)
        {
            Tile tile;
            if (renderMode == RenderMode.Scene)
            {
                tile = _allSceneTiles[i, j];
                _allSceneTiles.Remove(i, j);
            }
            else
            {
                tile = _allOverviewTiles[i, j];
                _allOverviewTiles.Remove(i, j);
            }

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
            // Let's cleanup old tiles first to release memory.
            DestroyRemoteTiles(position, RenderMode.Scene);
            DestroyRemoteTiles(position, RenderMode.Overview);

            var index = GetNextTileIndex(tile, position, i, j);
            if (_allSceneTiles.ContainsKey(index.Item1, index.Item2))
                return;

            Create(index.Item1, index.Item2);
        }

        private void DestroyRemoteTiles(MapPoint position, RenderMode renderMode)
        {
            var collection = renderMode == RenderMode.Scene ? _allSceneTiles : _allOverviewTiles;
            var threshold = renderMode == RenderMode.Scene ? _thresholdDistance : _thresholdDistance* (_overviewBuffer + 1);
            foreach (var doubleKeyPairValue in collection.ToList())
            {
                var candidateToDie = doubleKeyPairValue.Value;
                if (candidateToDie.MapCenter.DistanceTo(position) >= threshold)
                    Destroy(doubleKeyPairValue.Key1, doubleKeyPairValue.Key2, renderMode);
            }
        }

        /// <summary> Gets next tile index. </summary>
        private MutableTuple<int, int> GetNextTileIndex(Tile tile, MapPoint position, int i, int j)
        {
            var rectangle = tile.Rectangle;
            // top
            if (GeometryUtils.IsPointInTriangle(position, tile.MapCenter, rectangle.TopLeft, rectangle.TopRight))
                return new MutableTuple<int, int>(i, j + 1);
      
            // left
            if (GeometryUtils.IsPointInTriangle(position, tile.MapCenter, rectangle.TopLeft, rectangle.BottomLeft))
                return new MutableTuple<int, int>(i - 1, j);

            // right
            if (GeometryUtils.IsPointInTriangle(position, tile.MapCenter, rectangle.TopRight, rectangle.BottomRight))
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

                    var tileCollection = _renderModeEx != OverviewMode ? _allSceneTiles : _allOverviewTiles;

                    bool hasTile = tileCollection.ContainsKey(i, j);
                    if (!hasTile)
                        Create(i, j);

                    var tile = tileCollection[i, j];
                    if (hasTile) 
                        _messageBus.Send(new TileFoundMessage(tile, _currentMapPoint));

                    if (ShouldPreload(tile, value))
                        PreloadNextTile(tile, value, i, j);

                    _currentSceneTileIndex.Item1 = i;
                    _currentSceneTileIndex.Item2 = j;
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

            // NOTE don't want to extend RenderMode enum with Mixed field
            var renderModeString = configSection.GetString("render_mode", "mixed").ToLower();
            switch (renderModeString)
            {
                case "overview":
                    _renderModeEx = OverviewMode;
                    break;
                case "scene":
                    _renderModeEx = SceneMode;
                    break;
                default:
                    _renderModeEx = MixedMode;
                    break;
            }
            _overviewBuffer = configSection.GetInt("overview_buffer", 1);

            _thresholdDistance = (float) Math.Sqrt(2)*_tileSize;
        }

        #endregion
    }
}
