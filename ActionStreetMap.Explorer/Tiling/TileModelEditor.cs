using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
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

namespace ActionStreetMap.Explorer.Tiling
{
    /// <summary> Provides the way to edit tile models. </summary>
    public interface ITileModelEditor
    {
        /// <summary> Sets start id. </summary>
        long StartId { get; set; }

        /// <summary> Save all changes. </summary>
        void Commit();

        /// <summary> Adds building to current scene. </summary>
        void AddBuilding(Building building);

        /// <summary> Delets building with given id from element source covered by given map rectangle. </summary>
        void DeleteBuilding(long id, MapRectangle rectangle);
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

        private long _currentModelId = 0;
        private IElementSource _currentElementSource;

        /// <summary> Gets or sets trace. </summary>
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
            EnsureElementSource(building.Footprint.First());

            building.Id = _currentModelId++;
            var way = CreateWayFromPoints(building.Id , building.Footprint);
            way.Tags = new TagCollection()
                .Add("building", "yes") // TODO add correct tags
                .AsReadOnly();
            _elementSourceEditor.Add(way);
            LoadWay(way);
        }

        /// <inheritdoc />
        public void DeleteBuilding(long id, MapRectangle rectangle)
        {
            EnsureElementSource(rectangle.BottomLeft);
            
            var nullPoint = _tileController.CurrentTile.RelativeNullPoint;
            var boundingBox = new BoundingBox(
                GeoProjection.ToGeoCoordinate(nullPoint, rectangle.BottomLeft),
                GeoProjection.ToGeoCoordinate(nullPoint, rectangle.TopRight));
            
            _elementSourceEditor.Delete<Way>(id, boundingBox);
        }

        /// <summary> Ensures that the corresponding element source is loaded. </summary>
        private void EnsureElementSource(MapPoint point)
        {
            var boundingBox = _tileController.GetTile(point).BoundingBox;
            var elementSource = _elementSourceProvider.Get(boundingBox)
                .SingleOrDefault(e => !e.IsReadOnly).Wait();

            // create in memory element source
            if (elementSource == null)
            {
                var indexBuilder = new InMemoryIndexBuilder(boundingBox, IndexSettings.CreateDefault(), 
                    _objectPool, Trace);
                indexBuilder.Build();

                elementSource = new ElementSource(indexBuilder) {IsReadOnly = false};
                _elementSourceProvider.Add(elementSource);
            }

            CommitIfNecessary(elementSource);

            _currentElementSource = elementSource;
            _elementSourceEditor.ElementSource = _currentElementSource;
        }

        /// <summary> Visualizes way in scene. </summary>
        private void LoadWay(Way way)
        {
            way.Accept(new WayVisitor(_tileController.CurrentTile, _modelLoader, _objectPool));
        }

        /// <summary> Commits changes for old element source. </summary>
        private void CommitIfNecessary(IElementSource elementSource)
        {
            if (_currentElementSource != null && _currentElementSource != elementSource)
                _elementSourceEditor.Commit();
        }

        #region Helper methods
        
        /// <summary> Creates way from points plus adds id. </summary>
        private Way CreateWayFromPoints(long id, List<MapPoint> points)
        {
            var nullPoint = _tileController.CurrentTile.RelativeNullPoint;
            return new Way()
            {
                Id = id,
                Coordinates = points.Select(p => GeoProjection.ToGeoCoordinate(nullPoint, p)).ToList()
            };
        }

        #endregion
    }
}
