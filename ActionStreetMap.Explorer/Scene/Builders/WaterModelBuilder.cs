using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Models.Geometry;
using ActionStreetMap.Models.Terrain;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Core.Scene.Details;

using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides logic to build water.
    /// </summary>
    public class WaterModelBuilder : ModelBuilder
    {
        private const int NoLayer = -1;

        /// <inheritdoc />
        public override string Name { get { return "water"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            base.BuildArea(tile, rule, area);

            var verticies2D = ObjectPool.NewList<MapPoint>();

            // get polygon map points
            PointUtils.GetPolygonPoints(tile.HeightMap, tile.RelativeNullPoint, area.Points, verticies2D);

            // detect minimal elevation for water
            var elevation = verticies2D.Min(v => v.Elevation);

            // cut polygon by current tile
            PolygonUtils.ClipPolygonByTile(tile.BottomLeft, tile.TopRight, verticies2D);

            // get offset points to prevent gaps between water polygon and terrain due to issues 
            // on low terrain heightmap resolutions
            // NOTE current polygon cut algorithm may produce self-intersection results
            // TODO have to test current offset alhorithm
            // TODO determine better offset constant or make it configurable
            //var offsetPoints = ObjectPool.NewList<MapPoint>(verticies2D.Count);
            //PolygonUtils.MakeOffset(verticies2D, offsetPoints, -2f);
     
            // add elevation
            tile.Canvas.AddElevation(new Surface
            {
                ZIndex = rule.GetZIndex(),
                AverageElevation = elevation - 10,
                Points = verticies2D
            });

            var vector3Ds = verticies2D.GetVerticies(elevation - 2.5f);
            var triangles = PolygonUtils.Triangulate(verticies2D, ObjectPool);

            //ObjectPool.Store(verticies2D);

            IGameObject gameObjectWrapper = GameObjectFactory.CreateNew(String.Format("{0} {1}", Name, area));
            Scheduler.MainThread.Schedule(() => BuildObject(gameObjectWrapper, rule, vector3Ds, triangles));

            return gameObjectWrapper;
        }

        /// <summary>
        ///     Process unity specific data.
        /// </summary>
        protected virtual void BuildObject(IGameObject gameObjectWrapper, Rule rule, Vector3[] points, int[] triangles)
        {
            var gameObject = gameObjectWrapper.AddComponent(new GameObject());
            var mesh = new Mesh();
            mesh.vertices = points;
            //mesh.uv = verticies.GetUV();
            mesh.triangles = triangles;

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh.Clear();
            meshFilter.mesh = mesh;
            meshFilter.mesh.RecalculateNormals();

            gameObject.AddComponent<MeshRenderer>();
            gameObject.renderer.material = rule.GetMaterial(ResourceProvider);
            gameObject.renderer.material.color = rule.GetFillUnityColor();

            var layerIndex = rule.GetLayerIndex(NoLayer);
            if (layerIndex != NoLayer)
                gameObject.layer = layerIndex;
        }
    }
}
