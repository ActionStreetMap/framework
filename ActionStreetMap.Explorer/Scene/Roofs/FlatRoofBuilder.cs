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
            var gradient = ResourceProvider.GetGradient(building.RoofColor);
            var footprint = building.Footprint;
            var roofBottomOffset = building.Elevation + building.MinHeight;
            var roofTopOffset = roofBottomOffset + building.Height;

            var groundFloor = BuildFloor(gradient, footprint, roofBottomOffset);
            var top = CopyMeshData(groundFloor, roofTopOffset);

            groundFloor.Index = new PlaneMeshIndex(
                groundFloor.Vertices[0],
                groundFloor.Vertices[1],
                groundFloor.Vertices[2]);

            top.Index = new PlaneMeshIndex(
               top.Vertices[0],
               top.Vertices[1],
               top.Vertices[2]);

            return new List<MeshData>(2)
            {
                groundFloor,
                top
            };
        }
    }
}