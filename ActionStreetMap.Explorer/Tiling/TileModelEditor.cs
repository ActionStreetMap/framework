using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Import;
using ActionStreetMap.Maps.Visitors;
using Way = ActionStreetMap.Maps.Entities.Way;
using Node = ActionStreetMap.Maps.Entities.Node;

namespace ActionStreetMap.Explorer.Tiling
{
    /// <summary> Provides the way to edit tile models. </summary>
    public interface ITileModelEditor
    {
        /// <summary> Sets start id. </summary>
        long StartId { get; set; }

        /// <summary> Save all changes. </summary>
        void Commit();

        #region Model manipulations

        /// <summary> Adds building to current scene. </summary>
        void AddBuilding(Building building);
        /// <summary> Deletes building with given id from element source covered by given map rectangle. </summary>
        void DeleteBuilding(long id, Rectangle2d rectangle);

        /// <summary> Adds barrier to current scene. </summary>
        void AddBarrier(Barrier barrier);
        /// <summary> Deletes barrier with given id from element source covered by given map rectangle. </summary>
        void DeleteBarrier(long id, Rectangle2d rectangle);

        /// <summary> Adds building to current scene. </summary>
        void AddTree(Tree tree);
        /// <summary> Deletes tree with given id from element source. </summary>
        void DeleteTree(long id, Vector2d point);

        #endregion
    }

    /// <summary> Default implementation of <see cref="ITileModelEditor"/>. </summary>
    /// <remarks> Not thread safe. </remarks>
    internal sealed class TileModelEditor : ITileModelEditor
    {
        private readonly ITileController _tileController;
        private readonly IElementSourceProvider _elementSourceProvider;
        private readonly IElementSourceEditor _elementSourceEditor;
        private readonly IModelLoader _modelLoader;
        private readonly IObjectPool _objectPool;

        private long _currentModelId;
        private IElementSource _currentElementSource;

        /// <summary> Gets or sets trace. </summary>
        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates instance of <see cref="TileModelEditor"/>. </summary>
        /// <param name="tileController">Tile controller. </param>
        /// <param name="elementSourceProvider">Element source provider.</param>
        /// <param name="elementSourceEditor">Element source editor.</param>
        /// <param name="modelLoader">Model loader.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public TileModelEditor(ITileController tileController, 
                               IElementSourceProvider elementSourceProvider,
                               IElementSourceEditor elementSourceEditor,
                               IModelLoader modelLoader,
                               IObjectPool objectPool)
        {
            _tileController = tileController;
            _elementSourceProvider = elementSourceProvider;
            _elementSourceEditor = elementSourceEditor;
            _modelLoader = modelLoader;
            _objectPool = objectPool;
        }

        #region ITileModelEditor implementation

        /// <inheritdoc />
        public long StartId
        {
            get { return _currentModelId; }
            set { _currentModelId = value; }
        }

        /// <inheritdoc />
        public void Commit()
        {
            _elementSourceEditor.Commit();
        }

        /// <inheritdoc />
        public void AddBuilding(Building building)
        {
            building.Id = _currentModelId++;
            AddWayModel(building.Id, building.Footprint, new TagCollection()
                .Add("building", "yes")
                .AsReadOnly());
        }

        /// <inheritdoc />
        public void DeleteBuilding(long id, Rectangle2d rectangle)
        {
            DeleteElement(id, rectangle);
        }

        /// <inheritdoc />
        public void AddBarrier(Barrier barrier)
        {
            barrier.Id = _currentModelId++;
            AddWayModel(barrier.Id, barrier.Footprint, new TagCollection()
                .Add("barrier", "yes")
                .AsReadOnly());
        }

        /// <inheritdoc />
        public void DeleteBarrier(long id, Rectangle2d rectangle)
        {
            DeleteElement(id, rectangle);
        }

        /// <inheritdoc />
        public void AddTree(Tree tree)
        {
            tree.Id = _currentModelId++;
            AddNodeModel(tree.Id, tree.Point, new TagCollection()
                .Add("natural", "tree")
                .AsReadOnly());
        }

        /// <inheritdoc />
        public void DeleteTree(long id, Vector2d point)
        {
            DeleteElement(id, new Rectangle2d(point.X, point.Y, 0, 0));
        }

        #endregion

        /// <summary> Ensures that the corresponding element source is loaded. </summary>
        private void EnsureElementSource(Vector2d point)
        {
            var boundingBox = _tileController.GetTile(point).BoundingBox;
            var elementSource = _elementSourceProvider.Get(boundingBox)
                .SingleOrDefault(e => !e.IsReadOnly).Wait();

            // create in memory element source
            if (elementSource == null)
            {
                // NOTE use bounding box which fits whole world
                var indexBuilder = new InMemoryIndexBuilder(new BoundingBox(
                    new GeoCoordinate(-90, -180), new GeoCoordinate(90, 180)), 
                        IndexSettings.CreateDefault(), _objectPool, Trace);
                indexBuilder.Build();

                elementSource = new ElementSource(indexBuilder) {IsReadOnly = false};
                _elementSourceProvider.Add(elementSource);
            }

            CommitIfNecessary(elementSource);

            _currentElementSource = elementSource;
            _elementSourceEditor.ElementSource = _currentElementSource;
        }

        /// <summary> Adds way model to element source and scene. </summary>
        private void AddWayModel(long id, List<Vector2d> footprint, TagCollection tags)
        {
            EnsureElementSource(footprint.First());
            var nullPoint = _tileController.CurrentTile.RelativeNullPoint;

            var way = new Way()
            {
                Id = id,
                Tags = tags,
                Coordinates = footprint.Select(p => GeoProjection.ToGeoCoordinate(nullPoint, p)).ToList()
            };
            
            _elementSourceEditor.Add(way);
            way.Accept(new WayVisitor(_tileController.CurrentTile, _modelLoader, _objectPool));
        }

        /// <summary> Adds node model to to element source and scene. </summary>
        private void AddNodeModel(long id, Vector2d point, TagCollection tags)
        {
            EnsureElementSource(point);
            var nullPoint = _tileController.CurrentTile.RelativeNullPoint;

            var node = new Node()
            {
                Id = id,
                Tags = tags,
                Coordinate = GeoProjection.ToGeoCoordinate(nullPoint, point)
            };

            _elementSourceEditor.Add(node);
            node.Accept(new NodeVisitor(_tileController.CurrentTile, _modelLoader, _objectPool));
        }

        /// <summary> Deletes way from element source. </summary>
        private void DeleteElement(long id, Rectangle2d rectangle)
        {
            EnsureElementSource(rectangle.BottomLeft);

            var nullPoint = _tileController.CurrentTile.RelativeNullPoint;
            var boundingBox = new BoundingBox(
                GeoProjection.ToGeoCoordinate(nullPoint, rectangle.BottomLeft),
                GeoProjection.ToGeoCoordinate(nullPoint, rectangle.TopRight));

            _elementSourceEditor.Delete<Way>(id, boundingBox);
        }

        /// <summary> Commits changes for old element source. </summary>
        private void CommitIfNecessary(IElementSource elementSource)
        {
            if (_currentElementSource != null && _currentElementSource != elementSource)
                _elementSourceEditor.Commit();
        }
    }
}
