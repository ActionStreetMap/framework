using System;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Explorer.Scene.Generators;
using ActionStreetMap.Explorer.Scene.Indices;
using UnityEngine;
using Mesh = ActionStreetMap.Core.Geometry.Triangle.Mesh;
using RenderMode = ActionStreetMap.Core.RenderMode;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides the way to process surfaces. </summary>
    internal class SurfaceModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "surface"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            var points = ObjectPool.NewList<Vector2d>();
            PointUtils.SetPolygonPoints(tile.RelativeNullPoint, area.Points, points);

            var parent = tile.GameObject;
            Action<Mesh> fillAction = null;
            if (rule.IsForest() && tile.RenderMode == RenderMode.Scene)
                fillAction = mesh => CreateForest(parent, rule, mesh);

            tile.Canvas.AddSurface(new Surface()
            {
                GradientKey = rule.GetColor(),
                TextureAtlas = rule.GetTextureAtlas(),
                TextureKey = rule.GetTextureKey(),
                ElevationNoise = rule.GetEleNoiseFreq(),
                ColorNoise = rule.GetColorNoiseFreq(),
                Points = points,
            }, fillAction);

            return null;
        }

        private void CreateForest(IGameObject parent, Rule rule, Mesh mesh)
        {
            var trunkGradientKey = rule.Evaluate<string>("trunk-color");
            var foliageGradientKey = rule.Evaluate<string>("foliage-color");
            int treeFreq = (int) (1 / rule.EvaluateDefault<float>("tree-freq", 0.1f));
            // TODO reuse tree builder?
            // TODO behaviour should be set somehow
            var node = new Node();
            foreach (var triangle in mesh.Triangles)
            {
                // TODO reuse mesh and/or generator?
                if (triangle.Id % treeFreq != 0) continue;

                var v0 = triangle.GetVertex(0);
                var v1 = triangle.GetVertex(1);
                var v2 = triangle.GetVertex(2);

                var center = new Vector2d((v0.X + v1.X + v2.X) / 3, (v0.Y + v1.Y + v2.Y) / 3);
                var elevation = ElevationProvider.GetElevation(center);

                var treeGen = new TreeGenerator()
                   .SetTrunkGradient(CustomizationService.GetGradient(trunkGradientKey))
                   .SetFoliageGradient(CustomizationService.GetGradient(foliageGradientKey))
                   .SetTrunkTexture(rule.GetTrunkTexture(triangle.Id, CustomizationService))
                   .SetFoliageTexture(rule.GetFoliageTexture(triangle.Id, CustomizationService))
                   .SetPosition(new Vector3((float)center.X, elevation, (float)center.Y));
                
                var meshData = new MeshData(MeshDestroyIndex.Default, treeGen.CalculateVertexCount());
                meshData.GameObject = GameObjectFactory.CreateNew("tree");
                meshData.MaterialKey = rule.GetMaterialKey();

                treeGen.Build(meshData);

                BuildObject(parent, meshData, rule, node);
            }
        }
    }
}
