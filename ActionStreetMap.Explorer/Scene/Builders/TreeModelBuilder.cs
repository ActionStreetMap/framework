using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Scene.Generators;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides the way to process trees. </summary>
    public class TreeModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "tree"; } }

        /// <inheritdoc />
        public override IGameObject BuildNode(Tile tile, Rule rule, Node node)
        {
            var mapPoint = GeoProjection.ToMapCoordinate(tile.RelativeNullPoint, node.Point);
            var elevation = ElevationProvider.GetElevation(mapPoint);

            var trunkGradientKey = rule.Evaluate<string>("trunk-color");
            var foliageGradientKey = rule.Evaluate<string>("foliage-color");

            var meshData = new MeshData();
            meshData.GameObject = GameObjectFactory.CreateNew("tree " + node.Id);
            meshData.MaterialKey = rule.GetMaterialKey();
            var treeGen = new TreeGenerator(meshData)
                .SetTrunkGradient(ResourceProvider.GetGradient(trunkGradientKey))
                .SetFoliageGradient(ResourceProvider.GetGradient(foliageGradientKey))
                .SetPosition(new Vector3((float)mapPoint.X, elevation, (float)mapPoint.Y));
             meshData.Initialize(treeGen.CalculateVertexCount());
            treeGen.Build();

            BuildObject(tile.GameObject, meshData, rule, node);

            return meshData.GameObject;
        }
    }
}