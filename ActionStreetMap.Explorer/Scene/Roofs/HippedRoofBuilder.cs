using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.StraightSkeleton;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds hipped roof. </summary>
    internal class HippedRoofBuilder: RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "hipped"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            var roofHeight = building.RoofHeight;
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            var skeleton = SkeletonBuilder.Build(building.Footprint);
            var vertexCount = 0;
            foreach (var edgeResult in skeleton.Edges)
                vertexCount += (edgeResult.Polygon.Count - 2)*12;
            
            var meshIndex = new MultiPlaneMeshIndex(skeleton.Edges.Count, vertexCount);
            var meshData = new MeshData();
            meshData.Initialize(vertexCount, true);
            meshData.Index = meshIndex;

            var distances = skeleton.Distances;
            foreach (var edgeResult in skeleton.Edges)
            {
                var polygon = edgeResult.Polygon;
                var triCount = polygon.Count - 2;
                for (int i = 0; i < triCount; i++)
                {
                    var p0 = polygon[i];
                    var p1 = polygon[i + 1];
                    var p2 = polygon[i + 2 == polygon.Count ? 0 : i + 2];

                    var v0 = new Vector3((float) p0.X, distances[p0] > 0 ? roofHeight + roofOffset : roofOffset,
                        (float) p0.Y);
                    var v1 = new Vector3((float) p1.X, distances[p1] > 0 ? roofHeight + roofOffset : roofOffset,
                        (float) p1.Y);
                    var v2 = new Vector3((float) p2.X, distances[p2] > 0 ? roofHeight + roofOffset : roofOffset,
                        (float) p2.Y);

                    if (i == 0)
                        meshIndex.AddPlane(v0, v1, v2, meshData.NextIndex);
                    AddTriangle(meshData, gradient, v0, v1, v2);
                }
            }

            return new List<MeshData>()
            {
                meshData,
                BuildFloor(gradient, building.Footprint, building.Elevation + building.MinHeight)
            };
        }

        private void AddTriangle(MeshData meshData, GradientWrapper gradient, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var v01 = Vector3Utils.GetIntermediatePoint(v0, v1);
            var v12 = Vector3Utils.GetIntermediatePoint(v1, v2);
            var v02 = Vector3Utils.GetIntermediatePoint(v0, v2);

            meshData.AddTriangle(v0, v01, v02, GetColor(gradient, v0));
            meshData.AddTriangle(v02, v01, v12, GetColor(gradient, v02));
            meshData.AddTriangle(v2, v02, v12, GetColor(gradient, v2));
            meshData.AddTriangle(v01, v1, v12, GetColor(gradient, v01));
        }
    }
}
