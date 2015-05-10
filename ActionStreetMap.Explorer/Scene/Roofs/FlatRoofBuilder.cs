using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Utils;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds flat roof. </summary>
    public class FlatRoofBuilder : RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "flat"; } }

        /// <summary> Flat builder can be used for every type of building. </summary>
        /// <param name="building"> Building. </param>
        /// <returns> Always true. </returns>
        public override bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public override MeshData Build(Building building)
        {
            var gradient = ResourceProvider.GetGradient(building.RoofColor);
            var footprint = building.Footprint;
            var roofBottomOffset = building.Elevation + building.MinHeight;
            var roofTopOffset = roofBottomOffset + building.Height;

            var meshData = ObjectPool.CreateMeshData();
            meshData.MaterialKey = building.RoofMaterial;

            var triangles = ObjectPool.NewList<int>();
            Triangulator.Triangulate(footprint, triangles);

            // top
            BuildFootprint(meshData, gradient, footprint, triangles, roofTopOffset);
            // bottom
            BuildFootprint(meshData, gradient, footprint, triangles, roofBottomOffset, true);

            return meshData;
        }
    }
}