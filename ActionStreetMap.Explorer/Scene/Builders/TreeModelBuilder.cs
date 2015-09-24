using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Scene.Generators;
using ActionStreetMap.Explorer.Scene.Indices;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides the way to process trees. </summary>
    internal class TreeModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "tree"; } }

        /// <inheritdoc />
        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var mapPoint = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            var elevation = ElevationProvider.GetElevation(mapPoint);

            var treeGen = new TreeGenerator()
                .SetTrunkGradient(rule.GetTrunkGradient(CustomizationService))
                .SetFoliageGradient(rule.GetFoliageGradient(CustomizationService))
                .SetTrunkTexture(rule.GetTrunkTexture((int) node.Id, CustomizationService))
                .SetFoliageTexture(rule.GetFoliageTexture((int) node.Id, CustomizationService))
                .SetPosition(new Vector3((float) mapPoint.X, elevation, (float) mapPoint.Y));

            var meshData = new MeshData(MeshDestroyIndex.Default, treeGen.CalculateVertexCount())
            {
                GameObject = GameObjectFactory.CreateNew("tree " + node.Id),
                MaterialKey = rule.GetMaterialKey()
            };

            treeGen.Build(meshData);

            BuildObject(tile.GameObject, meshData, rule, node);

            return meshData.GameObject;
        }
    }
}