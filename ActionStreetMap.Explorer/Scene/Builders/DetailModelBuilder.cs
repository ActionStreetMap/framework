using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Explorer.Scene.Roads;
using ActionStreetMap.Explorer.Helpers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides logic to build details.
    /// </summary>
    public class DetailModelBuilder: ModelBuilder
    {
        /// <inheritdoc />
        public override string Name
        {
            get { return "detail"; }
        }

        /// <inheritdoc />
        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var mapPoint = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            if (!tile.Contains(mapPoint, 0))
                return null;

            var detail = rule.GetDetail();
            var zIndex = rule.GetZIndex();
            mapPoint.Elevation = tile.HeightMap.LookupHeight(mapPoint);

            // TODO check this
            //WorldManager.AddModel(node.Id);
            var gameObjectWrapper = GameObjectFactory.CreateNew("detail " + node);

            Scheduler.MainThread.Schedule(() => BuildObject(gameObjectWrapper, tile, rule, node, mapPoint, zIndex, detail));

            return gameObjectWrapper;
        }

        /// <summary>
        ///     Process unity specific data.
        /// </summary>
        protected virtual void BuildObject(IGameObject gameObjectWrapper, Tile tile, Rule rule, Node node, MapPoint mapPoint, 
            float zIndex, string detail)
        {
            var prefab = ResourceProvider.GetGameObject(detail);
            var gameObject = (GameObject)Object.Instantiate(prefab);
            
            // TODO do we need this workarounf?
            if (rule.IsRoadFix())
                gameObject.AddComponent<RoadFixBehavior>().RotationOffset = rule.GetDetailRotation();

            gameObject.transform.position = new Vector3(mapPoint.X, mapPoint.Elevation + zIndex, mapPoint.Y);           
            gameObjectWrapper.Parent = tile.GameObject;
        }
    }
}
