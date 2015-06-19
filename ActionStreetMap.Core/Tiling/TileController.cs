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
    /// <summary> Controls flow of loading/unloading tiles. </summary>
    public interface ITileController : IPositionObserver<MapPoint>, IPositionObserver<GeoCoordinate>
    {
        /// <summary> Gets current tile. </summary>
        Tile CurrentTile { get; }

        /// <summary> Gets tile for given point. Null if tile is not loaded. </summary>
        Tile GetTile(MapPoint point);
        
        /// <summary> Gets or sets current render mode. </summary>
        RenderMode Mode { get; set; }

        /// <summary> Gets or sets current viewport. </summary>
        MapRectangle Viewport { get; set; }

        /// <summary> Gets tile size. </summary>
        float TileSize { get; }
    }

    /// <summary> This class listens to position changes and manages tile processing. </summary>
    public class TileController : ITileController, IConfigurable
    {
        private readonly object _lockObj = new object();

        private float _tileSize;
        private float _offset;
        private float _moveSensitivity;
        private RenderMode _renderMode;

        private int _horizontalOverviewTileCount;
        private int _verticalOverviewTileCount;

        private float _thresholdDistance;
        private MapPoint _lastUpdatePosition;
        private MapRectangle _viewport;

        private GeoCoordinate _currentPosition;
        private MapPoint _currentMapPoint;

        private readonly MutableTuple<int, int> _currentTileIndex = new MutableTuple<int, int>(0, 0);

        private readonly ITileLoader _tileLoader;
        private readonly IMessageBus _messageBus;
        private readonly ITileActivator _tileActivator;
        private readonly IObjectPool _objectPool;

        private readonly DoubleKeyDictionary<int, int, Tile> _allSceneTiles = new DoubleKeyDictionary<int, int, Tile>();
        private readonly DoubleKeyDictionary<int, int, Tile> _allOverviewTiles = new DoubleKeyDictionary<int, int, Tile>();

        // TODO used only inside this class. Should be private?
        /// <summary> Gets relative null point. </summary>
        public GeoCoordinate RelativeNullPoint;

        #region ITileController implementation

        /// <inheritdoc />
        public Tile CurrentTile
        {
            get
            {
                return (_renderMode == RenderMode.Scene ? _allSceneTiles : _allOverviewTiles)
                    [_currentTileIndex.Item1, _currentTileIndex.Item2];
            }
        }

        /// <inheritdoc />
        public Tile GetTile(MapPoint point)
        {
            int i = Convert.ToInt32(point.X / _tileSize);
            int j = Convert.ToInt32(point.Y / _tileSize);

            if (_allSceneTiles.ContainsKey(i, j))
                return _allSceneTiles[i, j];

            if (_allOverviewTiles.ContainsKey(i, j))
                return _allOverviewTiles[i, j];

            return null;
        }

        /// <inheritdoc />
        public RenderMode Mode
        {
            get { return _renderMode; }
            set
            {
                lock (_lockObj)
                {
                    _renderMode = value;
                    InvalidateLastKnownPosition();
                }
            }
        }

        /// <inheritdoc />
        public MapRectangle Viewport
        {
            get { return _viewport; }
            set
            {
                lock (_lockObj)
                {
                    _viewport = value;
                    RecalculateOverviewTileCount();
                    InvalidateLastKnownPosition();
                }
            }
        }

        /// <inheritdoc />
        public float TileSize { get { return _tileSize; } }

        #endregion

        /// <summary> Creates <see cref="TileController"/>. </summary>
        /// <param name="tileLoader">Tile loader.</param>
        /// <param name="tileActivator">Tile activator.</param>
        /// <param name="messageBus">Message bus.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public TileController(ITileLoader tileLoader, ITileActivator tileActivator, 
            IMessageBus messageBus, IObjectPool objectPool)
        {
            _tileLoader = tileLoader;
            _messageBus = messageBus;
            _tileActivator = tileActivator;
            _objectPool = objectPool;
            InvalidateLastKnownPosition();
        }

        #region Create/Destroy tile

        /// <summary> Creates tile if necessary. </summary>
        private void Create(int i, int j)
        {
            if (_renderMode != RenderMode.Overview)
                Load(i, j, RenderMode.Scene);

            for (int z = j - _verticalOverviewTileCount; z <= j + _verticalOverviewTileCount; z++)
                for (int k = i - _horizontalOverviewTileCount; k <= i + _horizontalOverviewTileCount; k++)
                {
                    if (!_allOverviewTiles.ContainsKey(k, z))
                        Load(k, z, RenderMode.Overview);
                }
        }

        /// <summary> Loads tile if necessary. </summary>
        private void Load(int i, int j, RenderMode renderMode)
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
            var index = GetNextTileIndex(tile, position, i, j);
            if (_allSceneTiles.ContainsKey(index.Item1, index.Item2))
                return;

            Create(index.Item1, index.Item2);
        }

        private void DestroyRemoteTiles(MapPoint position, RenderMode renderMode)
        {
            var collection = renderMode == RenderMode.Scene ? _allSceneTiles : _allOverviewTiles;
            var threshold = renderMode == RenderMode.Scene ? 
                _thresholdDistance : 
                _thresholdDistance * (Math.Max(_horizontalOverviewTileCount, _verticalOverviewTileCount) + 1);
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

        MapPoint IPositionObserver<MapPoint>.CurrentPosition { get { return _currentMapPoint; } }

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

                    // Cleanup old overview tiles first to release memory.
                    // NOTE scene tiles are kept untouched in overview mode
                    DestroyRemoteTiles(value, RenderMode.Overview);

                    // NOTE call always to enforce applying of changed settings when
                    // tile is already created.
                    Create(i, j);

                    // NOTE preload feature is used only by scene mode
                    if (_renderMode == RenderMode.Scene)
                    {
                        DestroyRemoteTiles(value, RenderMode.Scene);
                        var tile = _allSceneTiles[i, j];
                        if (ShouldPreload(tile, value))
                            PreloadNextTile(tile, value, i, j);
                    }

                    _currentTileIndex.Item1 = i;
                    _currentTileIndex.Item2 = j;
                }
            }
        }

        void IObserver<MapPoint>.OnError(Exception error) { }
        void IObserver<MapPoint>.OnCompleted() { }

        #endregion

        #region IObserver<GeoCoordinate> implementation

        GeoCoordinate IPositionObserver<GeoCoordinate>.CurrentPosition { get { return _currentPosition; } }

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

        #region Helpers

        /// <summary> Recalculates value which is used to detect grid size built from overview tiles. </summary>
        private void RecalculateOverviewTileCount()
        {
            _horizontalOverviewTileCount = GetTileCountFromSide(_viewport.Width);
            _verticalOverviewTileCount = GetTileCountFromSide(_viewport.Height);
        }

        private int GetTileCountFromSide(float size)
        {
            if (Math.Abs(size) < 0.00001f || size < _tileSize) size = 3 * _tileSize;
            return (int) Math.Ceiling((Math.Ceiling(size / _tileSize) - 1) / 2);
        }

        /// <summary> Makes last known position invalid to force execution of tile loading logic. </summary>
        private void InvalidateLastKnownPosition()
        {
            _lastUpdatePosition = new MapPoint(float.MinValue, float.MinValue);
        }

        #endregion

        #region IConfigurable implementation

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _tileSize = configSection.GetFloat("size", 500);
            _offset = configSection.GetFloat("offset", 50);
            _moveSensitivity = configSection.GetFloat("sensitivity", 10);

            var renderModeString = configSection.GetString("render_mode", "scene").ToLower();
            _renderMode = renderModeString == "scene" ? RenderMode.Scene : RenderMode.Overview;

            var viewportConfig = configSection.GetSection("viewport");
            var width = viewportConfig != null ? viewportConfig.GetFloat("w", 0) : 0;
            var height =  viewportConfig != null ? viewportConfig.GetFloat("h", 0) : 0;
            _viewport = new MapRectangle(0, 0, width, height);

            RecalculateOverviewTileCount();

            _thresholdDistance = (float) Math.Sqrt(2)*_tileSize;
        }

        #endregion
    }
}
