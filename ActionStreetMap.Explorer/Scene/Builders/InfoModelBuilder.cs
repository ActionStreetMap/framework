using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Scene.Infos;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utilities;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Explorer.Scene.Infos;
using ActionStreetMap.Explorer.Helpers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build info models. </summary>
    public class InfoModelBuilder: ModelBuilder
    {       
        /// <inheritdoc />
        public override string Name { get { return "info"; } }

        /// <inheritdoc />
        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var mapPoint = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            if (!tile.Contains(mapPoint, 0))
                return null;

            var key = rule.GetKey();
            var info = new Info
            {
                Key = key,
                Coordinate = node.Point,
            };
            var style = ThemeProvider.Get().GetInfoStyle(info);

            var gameObjectWrapper = GameObjectFactory.CreateNew(GetName(node));

            var zIndex = rule.GetZIndex();
            mapPoint.Elevation = tile.HeightMap.LookupHeight(mapPoint);

            Scheduler.MainThread.Schedule(() => BuildObject(tile, gameObjectWrapper, info, style, mapPoint, zIndex));

            return gameObjectWrapper;
        }

        /// <summary> Process unity specific data. </summary>
        protected virtual void BuildObject(Tile tile, IGameObject gameObjectWrapper, Info info, 
            InfoStyle style, MapPoint mapPoint, float zIndex)
        {
            var gameObject = gameObjectWrapper.AddComponent(GameObject.CreatePrimitive(PrimitiveType.Cube));
            var transform = gameObject.transform;
            transform.position = new Vector3(mapPoint.X, mapPoint.Elevation + zIndex, mapPoint.Y);
            // TODO define size 
            transform.localScale = new Vector3(2, 2, 2);

            var p0 = style.UvMap.LeftBottom;
            var p1 = new Vector2(style.UvMap.RightUpper.x, style.UvMap.LeftBottom.y);
            var p2 = new Vector2(style.UvMap.LeftBottom.x, style.UvMap.RightUpper.y);
            var p3 = style.UvMap.RightUpper;

            var mesh = gameObject.GetComponent<MeshFilter>().mesh;

            // Imagine looking at the front of the cube, the first 4 vertices are arranged like so
            //   2 --- 3
            //   |     |
            //   |     |
            //   0 --- 1
            // then the UV's are mapped as follows
            //    2    3    0    1   Front
            //    6    7   10   11   Back
            //   19   17   16   18   Left
            //   23   21   20   22   Right
            //    4    5    8    9   Top
            //   15   13   12   14   Bottom
            mesh.uv = new[]
            {
                p0, p1, p2, p3,
                p2, p3, p2, p3,
                p0, p1, p0, p1,
                p0, p3, p1, p2,
                p0, p3, p1, p2,
                p0, p3, p1, p2
            };

            gameObject.GetComponent<MeshRenderer>().sharedMaterial = ResourceProvider.GetMatertial(style.Path);
            
            gameObjectWrapper.Parent = tile.GameObject;
        }
    }
}
