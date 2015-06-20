using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Helpers;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps
{
    /// <summary> Loads tile from given element source. </summary>
    internal class MapTileLoader : ITileLoader
    {
        private readonly IElementSourceProvider _elementSourceProvider;
        private readonly IElevationProvider _elevationProvider;
        private readonly IModelLoader _modelLoader;
        private readonly IObjectPool _objectPool;

        /// <summary> Creates <see cref="MapTileLoader"/>. </summary>
        /// <param name="elementSourceProvider">Element source provider.</param>
        /// <param name="elevationProvider">Elevation provider.</param>
        /// <param name="modelLoader">model visitor.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public MapTileLoader(IElementSourceProvider elementSourceProvider,
            IElevationProvider elevationProvider,
            IModelLoader modelLoader, IObjectPool objectPool)
        {
            _elementSourceProvider = elementSourceProvider;
            _elevationProvider = elevationProvider;
            _modelLoader = modelLoader;
            _objectPool = objectPool;
        }

        /// <inheritdoc />
        public IObservable<Unit> Load(Tile tile)
        {
            var boundingBox = tile.BoundingBox;
            var zoomLevel = ZoomHelper.GetZoomLevel(tile.RenderMode);

            var filterElementVisitor = new CompositeElementVisitor(
                new NodeVisitor(tile, _modelLoader, _objectPool),
                new WayVisitor(tile, _modelLoader, _objectPool),
                new RelationVisitor(tile, _modelLoader, _objectPool));

            _elevationProvider.SetNullPoint(tile.RelativeNullPoint);

            // download elevation data if necessary
            if (!_elevationProvider.HasElevation(tile.BoundingBox))
                _elevationProvider.Download(tile.BoundingBox).Wait();

            // prepare tile
            tile.Accept(tile, _modelLoader);

            var subject = new Subject<Unit>();
            _elementSourceProvider
                .Get(tile.BoundingBox)
                .SelectMany(e => e.Get(boundingBox, zoomLevel))
                .ObserveOn(Scheduler.ThreadPool)
                .Subscribe(element => element.Accept(filterElementVisitor),
                    () =>
                    {
                        tile.Canvas.Accept(tile, _modelLoader);
                        subject.OnCompleted();
                    });

             return subject;
        }
    }
}
