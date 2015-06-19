using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data;
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
    }

    /// <summary> Default implementation of <see cref="ITileModelEditor"/>. </summary>
    internal sealed class TileModelEditor : ITileModelEditor
    {
        private readonly ITileController _tileController;
        private readonly IElementSourceEditor _elementSourceEditor;
        private readonly IModelLoader _modelLoader;
        private readonly IObjectPool _objectPool;

        private long _lastId = 0;

        /// <summary> Creates instance of <see cref="TileModelEditor"/>. </summary>
        /// <param name="tileController">Tile controller. </param>
        /// <param name="elementSourceEditor">Element source editor.</param>
        /// <param name="modelLoader">Model loader.</param>
        /// <param name="objectPool">Object pool.</param>
        public TileModelEditor(ITileController tileController, 
                               IElementSourceEditor elementSourceEditor,
                               IModelLoader modelLoader,
                               IObjectPool objectPool)
        {
            _tileController = tileController;
            _elementSourceEditor = elementSourceEditor;
            _modelLoader = modelLoader;
            _objectPool = objectPool;
        }

        /// <inheritdoc />
        public long StartId
        {
            get { return _lastId; }
            set { _lastId = value; }
        }

        /// <inheritdoc />
        public void Commit()
        {
            _elementSourceEditor.Commit();
        }

        /// <inheritdoc />
        public void AddBuilding(Building building)
        {
            var way = CreateWayFromPoints(building.Footprint);
            // TODO add correct tags
            way.Tags = new TagCollection()
                .AsReadOnly();
            // this add it to underlying store
            _elementSourceEditor.Add(way);
            LoadWay(way);
        }

        /// <summary> Visualizes way in scene. </summary>
        private void LoadWay(Way way)
        {
            way.Accept(new WayVisitor(_tileController.CurrentTile, _modelLoader, _objectPool));
        }

        #region Helper methods
        
        /// <summary> Creates way from points plus adds id. </summary>
        private Way CreateWayFromPoints(List<MapPoint> points)
        {
            var nullPoint = _tileController.CurrentTile.RelativeNullPoint;
            return new Way()
            {
                Id = _lastId++,
                Coordinates = points.Select(p => GeoProjection.ToGeoCoordinate(nullPoint, p)).ToList()
            };
        }

        #endregion
    }
}
