using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Geometry.Primitives;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;
using Rect = ActionStreetMap.Explorer.Geometry.Primitives.Rect;

namespace ActionStreetMap.Explorer.Scene
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

            var uvRectStr = rule.Evaluate<string>("rect");
            var width = (int) rule.GetWidth();
            var height = (int)rule.GetHeight();
            Rect rect = GetUvRect(uvRectStr, new Size(width, height));

            var gameObjectWrapper = GameObjectFactory.CreateNew(GetName(node));

            var minHeight = rule.GetMinHeight();
            mapPoint.Elevation = ElevationProvider.GetElevation(mapPoint);

            Scheduler.MainThread.Schedule(() => BuildObject(tile, gameObjectWrapper, rule, rect, mapPoint, minHeight));

            return gameObjectWrapper;
        }

        /// <summary> Process unity specific data. </summary>
        private void BuildObject(Tile tile, IGameObject gameObjectWrapper, Rule rule, 
            Rect rect, MapPoint mapPoint, float minHeight)
        {
            var gameObject = gameObjectWrapper.AddComponent(GameObject.CreatePrimitive(PrimitiveType.Cube));
            var transform = gameObject.transform;
            transform.position = new Vector3(mapPoint.X, mapPoint.Elevation + minHeight, mapPoint.Y);
            // TODO define size 
            transform.localScale = new Vector3(2, 2, 2);

            var p0 = rect.LeftBottom;
            var p1 = new Vector2(rect.RightUpper.x, rect.LeftBottom.y);
            var p2 = new Vector2(rect.LeftBottom.x, rect.RightUpper.y);
            var p3 = rect.RightUpper;

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

            gameObject.GetComponent<MeshRenderer>().sharedMaterial = rule.GetMaterial(ResourceProvider);
            
            gameObjectWrapper.Parent = tile.GameObject;
        }

        private Rect GetUvRect(string value, Size size)
        {
            // expect x,y,width,height and (0,0) is left bottom corner
            if (value == null)
                return null;

            var values = value.Split('_');
            if (values.Length != 4)
                throw new InvalidOperationException(String.Format(Strings.InvalidUvMappingDefinition, value));

            var width = (float)int.Parse(values[2]);
            var height = (float)int.Parse(values[3]);

            var offset = int.Parse(values[1]);
            var x = (float)int.Parse(values[0]);
            var y = Math.Abs((offset + height) - size.Height);

            var leftBottom = new Vector2(x / size.Width, y / size.Height);
            var rightUpper = new Vector2((x + width) / size.Width, (y + height) / size.Height);

            return new Rect(leftBottom, rightUpper);
        }
    }
}
