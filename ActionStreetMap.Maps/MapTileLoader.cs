using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps
{
    /// <summary> Loads tile from given element source. </summary>
    internal class MapTileLoader: ITileLoader
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
            var filterElementVisitor = new FilterElementVisitor(
                tile.BoundingBox,
                new NodeVisitor(tile, _modelLoader, _objectPool),
                new WayVisitor(tile, _modelLoader, _objectPool),
                new RelationVisitor(tile, _modelLoader, _objectPool));

            // download elevation data if necessary
            if (!_elevationProvider.HasElevation(tile.BoundingBox))
                _elevationProvider.Download(tile.BoundingBox);

            // prepare tile
            tile.Accept(tile, _modelLoader);

            var result = new Subject<Unit>();
            _elementSourceProvider.Get(tile.BoundingBox).Subscribe(elementSource => 
                elementSource.Get(tile.BoundingBox)
                .ObserveOn(Scheduler.ThreadPool)
                .Do(element => element.Accept(filterElementVisitor),
                    () =>
                    {
                        // NOTE so far, we call this for every element source
                        // However, it will break multiply element sources case
                        tile.Canvas.Accept(tile, _modelLoader);
                        result.OnCompleted();
                    })
                .Subscribe());

            return result;
        }
    }
}
