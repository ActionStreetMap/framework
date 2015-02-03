using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Explorer.Scene.Geometry;
using ActionStreetMap.Explorer.Scene.Geometry.ThickLine;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Explorer.Scene.Utils;

using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>
    ///     Provides logic to build various barriers.
    /// </summary>
    public class BarrierModelBuilder: ModelBuilder
    {
        private readonly HeightMapProcessor _heightMapProcessor;

        /// <inheritdoc />
        public override string Name { get { return "barrier"; } }

        /// <summary>
        ///     Creates instance of <see cref="BarrierModelBuilder"/>.
        /// </summary>
        /// <param name="heightMapProcessor">Heightmap processor.</param>
        [Dependency]
        public BarrierModelBuilder(HeightMapProcessor heightMapProcessor)
        {
            _heightMapProcessor = heightMapProcessor;
        }

        /// <inheritdoc />
        public override IGameObject BuildWay(Tile tile, Rule rule, Way way)
        {
            if (way.Points.Count < 2)
            {
                Trace.Warn(Strings.InvalidPolyline);
                return null;
            }

            var gameObjectWrapper = GameObjectFactory.CreateNew(GetName(way));

            var points = ObjectPool.NewList<MapPoint>();
            PointUtils.FillHeight(tile.HeightMap, tile.RelativeNullPoint, way.Points, points);

            var lines = ObjectPool.NewList<LineElement>(1);
            lines.Add(new LineElement(points, rule.GetWidth()));
            
            var dimenLineBuilder = new DimenLineBuilder(2, ObjectPool, _heightMapProcessor);
            dimenLineBuilder.Height = rule.GetHeight();
            dimenLineBuilder.Build(tile.HeightMap, lines,
                (p, t, u) => Scheduler.MainThread.Schedule(() =>
                {
                    BuildObject(gameObjectWrapper, rule, p, t, u);
                    dimenLineBuilder.Dispose();
                }));

            ObjectPool.StoreList(lines);
            ObjectPool.StoreList(points);

            return gameObjectWrapper;
        }

        /// <summary>
        ///     Process unity specific data.
        /// </summary>
        protected virtual void BuildObject(IGameObject gameObjectWrapper, Rule rule,
            List<Vector3> p, List<int> t, List<Vector2> u)
        {
            var gameObject = gameObjectWrapper.AddComponent(new GameObject());

            Mesh mesh = new Mesh();
            mesh.vertices = p.ToArray();
            mesh.triangles = t.ToArray();
            mesh.uv = u.ToArray();
            mesh.RecalculateNormals();

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = rule.GetMaterial(ResourceProvider);
            renderer.material.mainTexture = rule.GetTexture(ResourceProvider);
        }
    }
}
