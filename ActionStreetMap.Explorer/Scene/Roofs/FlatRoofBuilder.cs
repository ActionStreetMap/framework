using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
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
            var gradient = ResourceProvider.GetGradient(building.RoofColor);
            var footprint = building.Footprint;
            var roofBottomOffset = building.Elevation + building.MinHeight;
            var roofTopOffset = roofBottomOffset + building.Height;

            var top = BuildFloor(gradient, footprint, roofTopOffset);
            var groundFloor = ReuseMeshData(gradient, footprint, top, roofBottomOffset);

            return new List<MeshData>(2)
            {
                groundFloor,
                top
            };
        }
    }
}