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
            // NOTE the last floor plane will be flat roof
            return BuildFloors(building, building.Levels + 1, true);
        }
    }
}