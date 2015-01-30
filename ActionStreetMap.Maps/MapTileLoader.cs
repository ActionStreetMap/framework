
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Visitors;

namespace ActionStreetMap.Maps
{
    /// <summary>
    ///     Loads tile from given element source.
    /// </summary>
    public class MapTileLoader: ITileLoader
    {
        private readonly IElementSourceProvider _elementSourceProvider;
        private readonly IModelLoader _modelLoader;
        private readonly IObjectPool _objectPool;

        /// <summary>
        ///     Creates MapTileLoader.
        /// </summary>
        /// <param name="elementSourceProvider">Element source provider.</param>
        /// <param name="modelLoader">model visitor.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public MapTileLoader(IElementSourceProvider elementSourceProvider, 
            IModelLoader modelLoader, IObjectPool objectPool)
        {
            _elementSourceProvider = elementSourceProvider;
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
                new RelationVisitor(tile, _modelLoader, _objectPool)
            );

            // prepare tile
            tile.Accept(tile, _modelLoader);

            // NOTE Canvas.Accept should be called only once, but we call it for each element source
            return _elementSourceProvider
                .Get(tile.BoundingBox)
                .SelectMany(elementSource => elementSource.Get(tile.BoundingBox)
                    .ObserveOn(Scheduler.ThreadPool)
                    .Do(element => element.Accept(filterElementVisitor),
                        () => (tile.Canvas).Accept(tile, _modelLoader))
                    .AsCompletion());
        }
    }
}
