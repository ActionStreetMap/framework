using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary>  Provides logic to build water. </summary>
    internal class WaterModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "water"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            var verticies2D = ObjectPool.NewList<Vector2d>();

            // get polygon map points
            PointUtils.SetPolygonPoints(tile.RelativeNullPoint, area.Points, verticies2D);

            tile.Canvas.AddWater(new Surface { Points = verticies2D });

            return null;
        }
    }
}
