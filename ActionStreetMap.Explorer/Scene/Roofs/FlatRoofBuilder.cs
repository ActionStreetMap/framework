using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Indices;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds flat roof. </summary>
    internal class FlatRoofBuilder : RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "flat"; } }

        /// <summary> Flat builder can be used for every type of building. </summary>
        /// <param name="building"> Building. </param>
        /// <returns> Always true. </returns>
        public override bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            var mesh = CreateMesh(building.Footprint);

            var floorHeight = building.Height / building.Levels;
            var bottomOffset = building.Elevation + building.MinHeight;

            // NOTE the last floor plane will be flat roof
            var floorCount = building.Levels + 1;
            var vertexPerFloor = mesh.Triangles.Count * 3 * 2;
            int vertexCount = vertexPerFloor * floorCount;

            int meshCount = 1;
            int floorsPerIteration = floorCount;
            var twoSizedMeshCount = vertexCount*2;
            if (twoSizedMeshCount > Consts.MaxMeshSize)
            {
                Trace.Warn(LogCategory, Strings.MeshHasMaxVertexLimit, building.Id.ToString(),
                    twoSizedMeshCount.ToString());
                meshCount = (int)Math.Ceiling((double)twoSizedMeshCount / Consts.MaxMeshSize);
                floorsPerIteration = floorCount/meshCount;
            }

            var meshDataList = new List<MeshData>(meshCount);

            for (int i = 0; i < meshCount; i++)
            {
                var stepFloorCount = (i != meshCount - 1 || meshCount == 1)
                    ? floorsPerIteration
                    : floorsPerIteration + floorCount % meshCount;

                var stepVertexCount = vertexPerFloor*stepFloorCount;
                var stepBottomOffset = bottomOffset + i*(floorsPerIteration*floorHeight);

                var meshIndex = new MultiPlaneMeshIndex(stepFloorCount, stepVertexCount);
                var meshData = new MeshData(meshIndex, stepVertexCount);

                AttachFloors(new RoofContext()
                {
                    Mesh = mesh,
                    MeshData = meshData,
                    MeshIndex = meshIndex,

                    Bottom = stepBottomOffset,
                    FloorCount = stepFloorCount,
                    FloorHeight = floorHeight,
                    FloorFrontGradient = ResourceProvider.GetGradient(building.FloorFrontColor),
                    FloorBackGradient = ResourceProvider.GetGradient(building.FloorBackColor),

                    IsLastRoof = true,
                    RoofFrontGradient = ResourceProvider.GetGradient(building.RoofColor),
                    RoofBackGradient = ResourceProvider.GetGradient(building.RoofColor),
                });

                meshDataList.Add(meshData);
            }

            return meshDataList;
        }
    }
}