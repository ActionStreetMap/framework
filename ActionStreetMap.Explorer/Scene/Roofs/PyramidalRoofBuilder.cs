using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary>
    ///     Builds Pyramidal roof.
    ///     See http://wiki.openstreetmap.org/wiki/Key:roof:shape#Roof
    /// </summary>
    internal class PyramidalRoofBuilder : RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "pyramidal"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building)
        {
            // TODO actually, we cannot use pyramidal for non-convex polygons
            return true;
        }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            var center = PolygonUtils.GetCentroid(building.Footprint);
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            var meshData = CreateMeshData(building, gradient, center);
            return new List<MeshData>(1)
            {
                meshData,
                BuildFloor(gradient, building.Footprint, building.Elevation + building.MinHeight)
            };
        }

        private MeshData CreateMeshData(Building building, GradientWrapper gradient, Vector2d center)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var footprint = building.Footprint;
            var roofHeight = building.RoofHeight;

            var length = footprint.Count;

            var meshData = new MeshData();
            meshData.Initialize(12 * length, true);

            var meshIndex = new MultiplyPlaneMeshIndex();
            meshIndex.Init(length, meshData.Vertices.Length);
            meshData.Index = meshIndex;
            for (int i = 0; i < length; i++)
            {
                var nextIndex = i == (length - 1) ? 0 : i + 1;

                var v0 = new Vector3((float)footprint[i].X, roofOffset, (float)footprint[i].Y);
                var v1 = new Vector3((float)center.X, roofOffset + roofHeight, (float)center.Y);
                var v2 = new Vector3((float)footprint[nextIndex].X, roofOffset, (float)footprint[nextIndex].Y);

                var v01 = GetIntermediatePoint(v0, v1);
                var v12 = GetIntermediatePoint(v1, v2);
                var v02 = GetIntermediatePoint(v0, v2);

                meshData.AddTriangle(v0, v01, v02, GetColor(gradient, v0));
                meshData.AddTriangle(v02, v01, v12, GetColor(gradient, v02));
                meshData.AddTriangle(v2, v02, v12, GetColor(gradient, v2));
                meshData.AddTriangle(v01, v1, v12, GetColor(gradient, v01));

                meshIndex.AddPlane(v0, v1, v2, i * 12);
            }

            return meshData;
        }

        private Vector3 GetIntermediatePoint(Vector3 v0, Vector3 v1)
        {
            var distance01 = Vector3.Distance(v0, v1);
           return v0 + (v1 - v0).normalized * distance01 / 2;
        }
    }
}
