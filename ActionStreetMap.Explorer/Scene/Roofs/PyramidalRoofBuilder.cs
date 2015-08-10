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

            var mesh = CreateMesh(footprint);
            var floorVertexCount = mesh.Triangles.Count * 3 * 2 * floorCount;

            var length = footprint.Count;
            var vertexCount = 12 * length + floorVertexCount;

            var meshIndex = new MultiPlaneMeshIndex(length + floorCount, vertexCount);
            var meshData = new MeshData(meshIndex, vertexCount);

            var roofGradient = ResourceProvider.GetGradient(building.RoofColor);
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
                meshData.AddTriangle(v0, v01, v02, color, color);

                color = GetColor(roofGradient, v01);
                meshData.AddTriangle(v02, v01, v12, color, color);

                color = GetColor(roofGradient, v02);
                meshData.AddTriangle(v2, v02, v12, color, color);

                color = GetColor(roofGradient, v01);
                meshData.AddTriangle(v01, v1, v12, color, color);
            }

            AttachFloors(new RoofContext()
            {
                Mesh = mesh,
                MeshData = meshData,
                MeshIndex = meshIndex,

                Bottom = building.Elevation + building.MinHeight,
                FloorCount = floorCount,
                FloorHeight = building.Height / floorCount,
                FloorFrontGradient = ResourceProvider.GetGradient(building.FloorFrontColor),
                FloorBackGradient = ResourceProvider.GetGradient(building.FloorBackColor),

                IsLastRoof = false,
            });

            return new List<MeshData>(1) { meshData };
        }
    }
}
