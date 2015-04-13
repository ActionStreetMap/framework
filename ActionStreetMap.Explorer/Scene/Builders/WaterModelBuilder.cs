using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry.Utils;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>  Provides logic to build water. </summary>
    public class WaterModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "water"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            base.BuildArea(tile, rule, area);

            var verticies2D = ObjectPool.NewList<MapPoint>();

            // get polygon map points
            PointUtils.GetPolygonPoints(tile.RelativeNullPoint, area.Points, verticies2D);

            tile.Canvas.AddWater(new Surface()
            {
                Points = verticies2D,
            });

            return null;
        }
    }
}
