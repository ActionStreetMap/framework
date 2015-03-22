using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene.Roads;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Maps.Helpers;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Provides the way to process roads. </summary>
    public class RoadModelBuilder: ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "road"; } }

        /// <inheritdoc />
        public override IGameObject BuildWay(Tile tile, Rule rule, Way way)
        {
            var points = ObjectPool.NewList<MapPoint>();
            PointUtils.FillHeight(ElevationProvider, tile.RelativeNullPoint, way.Points, points);

            // road should be processed in one place: it's better to collect all 
            // roads and create connected road network
            tile.Canvas.AddRoad(new RoadElement
            {
                Id = way.Id,
                Address = AddressExtractor.Extract(way.Tags),
                Width = (int) Math.Round(rule.GetWidth() / 2),
                Type = rule.GetRoadType(),
                Points = points
            });

            return null;
        }
    }
}
