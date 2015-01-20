
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Osm.Index;
using ActionStreetMap.Osm.Visitors;

namespace ActionStreetMap.Osm
{
    /// <summary>
    ///     Loads tile from given element source.
    /// </summary>
    public class MapTileLoader: ITileLoader
    {
        private readonly IElementSourceProvider _elementSourceProvider;
        private readonly IModelVisitor _modelVisitor;
        private readonly FilterElementVisitor _filterElementVisitor;

        /// <summary>
        ///     Creates MapTileLoader.
        /// </summary>
        /// <param name="elementSourceProvider">Element source provider.</param>
        /// <param name="modelVisitor">model visitor.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public MapTileLoader(IElementSourceProvider elementSourceProvider, 
            IModelVisitor modelVisitor, IObjectPool objectPool)
        {
            _elementSourceProvider = elementSourceProvider;
            _modelVisitor = modelVisitor;

            _filterElementVisitor = new FilterElementVisitor(new NodeVisitor(modelVisitor, objectPool),
                new WayVisitor(modelVisitor, objectPool),
                new RelationVisitor(modelVisitor, objectPool)
            );
        }

        /// <inheritdoc />
        public void Load(Tile tile)
        {
            // get element source for given bounding box
            var elementSource = _elementSourceProvider.Get(tile.BoundingBox);

            // prepare tile
            tile.Accept(_modelVisitor);

            _filterElementVisitor.BoundingBox = tile.BoundingBox;
            var source = elementSource.Get(tile.BoundingBox).ObserveOn(Scheduler.ThreadPool);
            source.Subscribe(element =>
            {
                System.Console.WriteLine("accept:{0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                element.Accept(_filterElementVisitor);
            },
            () => (new Canvas()).Accept(_modelVisitor));
            //source.Wait();
            System.Console.ReadKey();
        }
    }
}
