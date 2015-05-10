using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Geometry.Generators;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Helpers;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;
using Mesh = ActionStreetMap.Core.Geometry.Triangle.Mesh;

namespace ActionStreetMap.Explorer.Scene.Builders
{
    /// <summary> Provides the way to process surfaces. </summary>
    public class SurfaceModelBuilder : ModelBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "surface"; } }

        /// <inheritdoc />
        public override IGameObject BuildArea(Tile tile, Rule rule, Area area)
        {
            var points = ObjectPool.NewList<MapPoint>();
            PointUtils.GetPolygonPoints(tile.RelativeNullPoint, area.Points, points);

            var parent = tile.GameObject;
            Action<Mesh> fillAction = null;
            if (rule.IsForest())
                fillAction = mesh => CreateForest(parent, rule, mesh);

            tile.Canvas.AddSurface(new Surface()
            {
                GradientKey = rule.GetFillColor(),
                Points = points,
            }, fillAction);

            return null;
        }

        private void CreateForest(IGameObject parent, Rule rule, Mesh mesh)
        {
            var trunkGradientKey = rule.Evaluate<string>("trunk-color");
            var foliageGradientKey = rule.Evaluate<string>("foliage-color");
            int treeFreq = (int) (1 / rule.EvaluateDefault<float>("tree-freq", 0.1f));

            foreach (var triangle in mesh.Triangles)
            {
                // TODO reuse mesh and/or generator?
                if (triangle.ID % treeFreq != 0) continue;

                var v0 = triangle.GetVertex(0);
                var v1 = triangle.GetVertex(1);
                var v2 = triangle.GetVertex(2);

                var center = new MapPoint((float)(v0.x + v1.x + v2.x) / 3, (float)(v0.y + v1.y + v2.y) / 3);
                var elevation = ElevationProvider.GetElevation(center);
                var meshData = ObjectPool.CreateMeshData();
                meshData.GameObject = GameObjectFactory.CreateNew("tree");
                meshData.MaterialKey = rule.GetMaterialKey();
                new TreeGenerator(meshData)
                    .SetTrunkGradient(ResourceProvider.GetGradient(trunkGradientKey))
                    .SetFoliageGradient(ResourceProvider.GetGradient(foliageGradientKey))
                    .SetPosition(new Vector3(center.X, elevation, center.Y))
                    .Build();

                BuildObject(parent, meshData);
            }
        }
    }
}
