using System;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build info models. </summary>
    internal class InfoModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "info"; } }

        /// <inheritdoc />
        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var poinr = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            if (!tile.Contains(poinr, 0))
                return null;

            var uvRectStr = rule.Evaluate<string>("rect");
            var width = (int) rule.GetWidth();
            var height = (int)rule.GetHeight();
            Rectangle2d rect = GetUvRect(uvRectStr, new Size(width, height));

            var gameObjectWrapper = GameObjectFactory.CreateNew(GetName(node));

            var minHeight = rule.GetMinHeight();

            Observable.Start(() => BuildObject(tile, gameObjectWrapper, rule, rect, poinr, minHeight),
                Scheduler.MainThread);

            return gameObjectWrapper;
        }

        /// <summary> Process unity specific data. </summary>
        private void BuildObject(Tile tile, IGameObject gameObjectWrapper, Rule rule,
            Rectangle2d rect, Vector2d point, float minHeight)
        {
            var gameObject = gameObjectWrapper.AddComponent(GameObject.CreatePrimitive(PrimitiveType.Cube));
            var transform = gameObject.transform;
            var elevation = ElevationProvider.GetElevation(point);
            transform.position = new Vector3((float)point.X, elevation + minHeight, (float)point.Y);
            // TODO define size 
            transform.localScale = new Vector3(2, 2, 2);

            var p0 = new Vector2((float)rect.Left, (float)rect.Bottom);
            var p1 = new Vector2((float)rect.Right, (float)rect.Bottom);
            var p2 = new Vector2((float)rect.Left, (float)rect.Top);
            var p3 = new Vector2((float)rect.Right, (float)rect.Top);

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

            gameObject.GetComponent<MeshRenderer>().sharedMaterial = rule.GetMaterial(CustomizationService);
            
            gameObjectWrapper.Parent = tile.GameObject;
        }

        private Rectangle2d GetUvRect(string value, Size size)
        {
            var values = value.Split('_');
            if (values.Length != 4)
                throw new InvalidOperationException(String.Format(Strings.InvalidUvMappingDefinition, value));

            var width = (float)int.Parse(values[2]);
            var height = (float)int.Parse(values[3]);

            var offset = int.Parse(values[1]);
            var x = (float)int.Parse(values[0]);
            var y = Math.Abs((offset + height) - size.Height);

            var leftBottom = new Vector2d(x / size.Width, y / size.Height);
            var rightUpper = new Vector2d((x + width) / size.Width, (y + height) / size.Height);

            return new Rectangle2d(leftBottom, rightUpper);
        }

        private class Size
        {
            public int Width;
            public int Height;

            public Size(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }
    }
}
