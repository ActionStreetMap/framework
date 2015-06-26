using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry.ThickLine;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides logic to build various barriers. </summary>
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

            if (tile.Registry.Contains(way.Id)) return null;

            var gameObjectWrapper = GameObjectFactory.CreateNew(GetName(way));
            var materialKey = rule.GetMaterialKey();
            var gradientKey = rule.GetFillColor();
            var height = rule.GetHeight();
            var width = rule.GetWidth();

            var points = ObjectPool.NewList<MapPoint>();
            PointUtils.FillHeight(ElevationProvider, tile.RelativeNullPoint, way.Points, points);

            var lines = ObjectPool.NewList<LineElement>(1);
            lines.Add(new LineElement(points, width));

            new DimenLineBuilder(ElevationProvider, ObjectPool)
                .SetHeight(height)
                .SetColorNoiseFreq(0.2f)
                .SetGradient(ResourceProvider.GetGradient(gradientKey))
                .SetMaxDistance(4f)
                .Build(tile.Rectangle, lines, (data) =>
                {
                    data.GameObject = gameObjectWrapper;
                    data.MaterialKey = materialKey;
                    BuildObject(tile.GameObject, data);
                });

            ObjectPool.StoreList(lines);
            ObjectPool.StoreList(points);

            tile.Registry.RegisterGlobal(way.Id);

            return gameObjectWrapper;
        }
    }
}
