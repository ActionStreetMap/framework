using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;

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

        [Dependency]
        public ITrace Trace { get; set; }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            var mesh = CreateMesh(building.Footprint);

            // NOTE the last floor plane will be flat roof
            var floorCount = building.Levels + 1;

            var vertexCount = mesh.Triangles.Count * 3 * 2 * floorCount;
            var meshIndex = new MultiPlaneMeshIndex(floorCount, vertexCount);
            var meshData = new MeshData(meshIndex, vertexCount);

            AttachFloors(new RoofContext()
            {
                Mesh = mesh,
                MeshData = meshData,
                MeshIndex = meshIndex,
                
                Bottom = building.Elevation + building.MinHeight,
                FloorCount = floorCount,
                FloorHeight = building.Height / (floorCount - 1),
                FloorFrontGradient = ResourceProvider.GetGradient(building.FloorFrontColor),
                FloorBackGradient = ResourceProvider.GetGradient(building.FloorBackColor),

                IsLastRoof = true,
                RoofFrontGradient = ResourceProvider.GetGradient(building.RoofColor),
                RoofBackGradient = ResourceProvider.GetGradient(building.RoofColor),
            });
                
            return new List<MeshData>(1) { meshData };
        }
    }
}