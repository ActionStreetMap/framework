
using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Infrastructure.Dependencies;
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
        private readonly CompositeVisitor _compositeVisitor;

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

            _compositeVisitor = new CompositeVisitor(new List<IElementVisitor>
            {
                new WayVisitor(modelVisitor, objectPool),
                new NodeVisitor(modelVisitor, objectPool),
                new RelationVisitor(modelVisitor, objectPool)
            });
        }

        /// <inheritdoc />
        public void Load(Tile tile)
        {
            // get element source for given bounding box
            var elementSource = _elementSourceProvider.Get(tile.BoundingBox);

            // prepare tile
            tile.Accept(_modelVisitor);

            foreach (var element in elementSource.Get(tile.BoundingBox))
                element.Accept(_compositeVisitor);

            // finalize by canvas visiting
            (new Canvas()).Accept(_modelVisitor);
        }
    }
}
