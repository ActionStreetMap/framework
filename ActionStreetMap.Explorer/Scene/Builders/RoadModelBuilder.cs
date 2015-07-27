using System;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Helpers;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides the way to process roads. </summary>
    public class RoadModelBuilder: ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "road"; } }

        /// <inheritdoc />
        public override IGameObject BuildWay(Tile tile, Rule rule, Way way)
        {
            var points = ObjectPool.NewList<Vector2d>(way.Points.Count);
            PointUtils.GetPolygonPoints(tile.RelativeNullPoint, way.Points, points);

            // road should be processed in one place: it's better to collect all 
            // roads and create connected road network
            tile.Canvas.AddRoad(new RoadElement
            {
                Id = way.Id,
                Width = (int) Math.Round(rule.GetWidth() / 2),
                Type = rule.GetRoadType(),
                Points = points
            });

            return null;
        }
    }
}
