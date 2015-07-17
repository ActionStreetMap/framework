using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.StraightSkeleton;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds hipped roof. </summary>
    public class HippedRoofBuilder: RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "hipped"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public override MeshData Build(Building building)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var meshData = ObjectPool.CreateMeshData();
            meshData.MaterialKey = building.RoofMaterial;
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            // TODO Use common primitive type here
            var skeleton = Skeleton.Create(building.Footprint
                .Select(p => new Vector2d(p.X, p.Y)).ToList());

            var distances = skeleton.Distances;
            foreach (var edgeResult in skeleton.Edges)
            {
                var polygon = edgeResult.Polygon;
                if (polygon.Count == 3)
                    AddPolygonAsTriangle(skeleton, polygon, building.RoofHeight, roofOffset, 
                        meshData, gradient);
                // TODO
            }

            return meshData;
        }

        private void AddPolygonAsTriangle(SkeletonResult skeleton, List<Vector2d> polygon, 
            float roofHeight, float roofOffset, MeshData meshData, GradientWrapper gradient)
        {
            var distances = skeleton.Distances;

            var p0 = polygon[0];
            var v0 = new MapPoint((float)p0.X, (float)p0.Y,
                distances[p0] > 0 ? roofHeight + roofOffset : roofOffset);

            var p1 = polygon[1];
            var v1 = new MapPoint((float)p1.X, (float)p1.Y,
                distances[p1] > 0 ? roofHeight + roofOffset : roofOffset);

            var p2 = polygon[2];
            var v2 = new MapPoint((float)p2.X, (float)p2.Y,
                distances[p2] > 0 ? roofHeight + roofOffset : roofOffset);

            meshData.AddTriangle(v0, v2, v1, GradientUtils.GetColor(gradient, v0, 0.2f));
        }
    }
}
