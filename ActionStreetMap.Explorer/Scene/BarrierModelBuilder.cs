using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.ThickLine;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
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
            var materialKey = rule.GetMaterialKey();

            var points = ObjectPool.NewList<MapPoint>();
            PointUtils.FillHeight(ElevationProvider, tile.RelativeNullPoint, way.Points, points);

            var lines = ObjectPool.NewList<LineElement>(1);
            lines.Add(new LineElement(points, rule.GetWidth()));

            var dimenLineBuilder = new DimenLineBuilder(2, ElevationProvider, ObjectPool);
            dimenLineBuilder.Height = rule.GetHeight();
            dimenLineBuilder.Build(tile.Rectangle, lines,
                (vertices, triangles, u) =>
                {
                    BuildObject(tile.GameObject, new MeshData()
                    {
                        GameObject = gameObjectWrapper,
                        MaterialKey = materialKey,
                        Vertices = vertices,
                        Triangles = triangles,
                        // TODO process colors
                        Colors = new List<Color>(vertices.Count)
                    });
                });

            ObjectPool.StoreList(lines);
            ObjectPool.StoreList(points);

            return gameObjectWrapper;
        }
    }
}
