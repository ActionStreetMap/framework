using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.Utils;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Scene.Utils;
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
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var footprint = building.Footprint;
            var roofHeight = building.RoofHeight;
            var floorCount = building.Levels;

            var length = footprint.Count;
            var mesh = CreateMesh(footprint);
            var roofVertexCount = 12*length;
            var floorVertexCount = mesh.Triangles.Count * 3 * 2 * floorCount;

            var vertexCount = roofVertexCount + floorVertexCount;
            var planeCount = building.Footprint.Count + floorCount;

            bool limitIsReached = false;
            if (vertexCount * 2 > Consts.MaxMeshSize)
            {
                vertexCount = roofVertexCount;
                planeCount = building.Footprint.Count;
                limitIsReached = true;
            }

            var meshIndex = new MultiPlaneMeshIndex(planeCount, vertexCount);
            var meshData = new MeshData(meshIndex, vertexCount);

            Vector2 uv0, uv1, uv2;
            GetUV(building, out uv0, out uv1, out uv2);

            var roofGradient = CustomizationService.GetGradient(building.RoofColor);
            for (int i = 0; i < length; i++)
            {
                var nextIndex = i == (length - 1) ? 0 : i + 1;

                var v0 = new Vector3((float)footprint[i].X, roofOffset, (float)footprint[i].Y);
                var v1 = new Vector3((float)center.X, roofOffset + roofHeight, (float)center.Y);
                var v2 = new Vector3((float)footprint[nextIndex].X, roofOffset, (float)footprint[nextIndex].Y);

                var v01 = Vector3Utils.GetIntermediatePoint(v0, v1);
                var v12 = Vector3Utils.GetIntermediatePoint(v1, v2);
                var v02 = Vector3Utils.GetIntermediatePoint(v0, v2);

                meshIndex.AddPlane(v0, v1, v2, meshData.NextIndex);

                var color = GetColor(roofGradient, v0);
                meshData.AddTriangle(v0, v01, v02, color, color, uv0, uv1, uv2);

                color = GetColor(roofGradient, v01);
                meshData.AddTriangle(v02, v01, v12, color, color, uv0, uv1, uv2);

                color = GetColor(roofGradient, v02);
                meshData.AddTriangle(v2, v02, v12, color, color, uv0, uv1, uv2);

                color = GetColor(roofGradient, v01);
                meshData.AddTriangle(v01, v1, v12, color, color, uv0, uv1, uv2);
            }

            if (!limitIsReached)
            {
                AttachFloors(new RoofContext()
                {
                    Mesh = mesh,
                    MeshData = meshData,
                    MeshIndex = meshIndex,

                    Bottom = building.Elevation + building.MinHeight,
                    FloorCount = floorCount,
                    FloorHeight = building.Height/floorCount,
                    FloorFrontGradient = CustomizationService.GetGradient(building.FloorFrontColor),
                    FloorBackGradient = CustomizationService.GetGradient(building.FloorBackColor),

                    IsLastRoof = false,
                });

                return new List<MeshData>(1) {meshData};
            }

            var meshDataList = BuildFloors(building, building.Levels, false);
            meshDataList.Add(meshData);
            return meshDataList;
        }
    }
}
