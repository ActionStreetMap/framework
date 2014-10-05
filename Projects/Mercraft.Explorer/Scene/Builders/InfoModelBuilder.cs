﻿using System;
using Mercraft.Core;
using Mercraft.Core.Algorithms;
using Mercraft.Core.MapCss.Domain;
using Mercraft.Core.Scene.Models;
using Mercraft.Core.Unity;
using Mercraft.Core.World;
using Mercraft.Core.World.Infos;
using Mercraft.Explorer.Helpers;
using Mercraft.Explorer.Themes;
using Mercraft.Infrastructure.Dependencies;
using Mercraft.Infrastructure.Utilities;
using Mercraft.Models.Infos;
using Mercraft.Models.Utils;
using UnityEngine;

namespace Mercraft.Explorer.Scene.Builders
{
    public class InfoModelBuilder: ModelBuilder
    {
        private readonly IThemeProvider _themeProvider;
        private readonly IResourceProvider _resourceProvider;

        public override string Name
        {
            get { return "info"; }
        }

        [Dependency]
        public InfoModelBuilder(WorldManager worldManager, IGameObjectFactory gameObjectFactory,
            IThemeProvider themeProvider, IResourceProvider resourceProvider, IObjectPool objectPool) :
                base(worldManager, gameObjectFactory, objectPool)
        {
            _themeProvider = themeProvider;
            _resourceProvider = resourceProvider;
        }

        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var mapPoint = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            if (!tile.Contains(mapPoint, 0))
                return null;

            var key = rule.GetKey();
            var info = new Info()
            {
                Key = key,
                Coordinate = node.Point,
            };
            var style = _themeProvider.Get().GetInfoStyle(info);

            var gameObjectWrapper = GameObjectFactory
              .CreatePrimitive(String.Format("info {0}", node), UnityPrimitiveType.Cube);

            var zIndex = rule.GetZIndex();
            mapPoint.Elevation = tile.HeightMap.LookupHeight(mapPoint);

            BuildObject(tile, gameObjectWrapper, info, style, mapPoint, zIndex);

            return gameObjectWrapper;
        }

        protected virtual void BuildObject(Tile tile, IGameObject gameObjectWrapper, Info info, 
            InfoStyle style, MapPoint mapPoint, float zIndex)
        {
            var gameObject = gameObjectWrapper.GetComponent<GameObject>();
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
            mesh.uv = new Vector2[]
            {
                p0, p1, p2, p3,
                p2, p3, p2, p3,
                p0, p1, p0, p1,
                p0, p3, p1, p2,
                p0, p3, p1, p2,
                p0, p3, p1, p2
            };

            gameObject.GetComponent<MeshRenderer>().sharedMaterial = _resourceProvider
                .GetMatertial(style.Path);
            
            gameObjectWrapper.Parent = tile.GameObject;
        }
    }
}
