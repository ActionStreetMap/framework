using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry.ThickLine;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary>  Provides logic to build various barriers. </summary>
    public class BarrierModelBuilder: ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "barrier"; } }

        /// <inheritdoc />
        public override IGameObject BuildWay(Tile tile, Rule rule, Way way)
        {
            if (way.Points.Count < 2)
            {
                Trace.Warn("model.barrier", Strings.InvalidPolyline);
                return null;
            }

            var gameObjectWrapper = GameObjectFactory.CreateNew(GetName(way));

            var points = ObjectPool.NewList<MapPoint>();
            PointUtils.FillHeight(ElevationProvider, tile.RelativeNullPoint, way.Points, points);

            var lines = ObjectPool.NewList<LineElement>(1);
            lines.Add(new LineElement(points, rule.GetWidth()));

            var dimenLineBuilder = new DimenLineBuilder(2, ElevationProvider, ObjectPool);
            dimenLineBuilder.Height = rule.GetHeight();
            dimenLineBuilder.Build(tile.Rectangle, lines,
                (p, t, u) =>
                {
                    var vertices = p.ToArray();
                    var triangles = t.ToArray();
                    var uvs = u.ToArray();
                    Scheduler.MainThread.Schedule(() =>
                    {
                        BuildObject(gameObjectWrapper, rule, vertices, triangles, uvs);
                        dimenLineBuilder.Dispose();
                    });
                });

            ObjectPool.StoreList(lines);
            ObjectPool.StoreList(points);

            return gameObjectWrapper;
        }

        /// <summary> Process unity specific data. </summary>
        protected virtual void BuildObject(IGameObject gameObjectWrapper, Rule rule,
            Vector3[] vertices, int[] triangles, Vector2[] uvs)
        {
            var gameObject = gameObjectWrapper.AddComponent(new GameObject());

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = rule.GetMaterial(ResourceProvider);
            renderer.material.mainTexture = rule.GetTexture(ResourceProvider);
        }
    }
}
