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
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            var triangles = ObjectPool.NewList<int>();
            Triangulator.Triangulate(building.Footprint, triangles);

            var meshData = ObjectPool.CreateMeshData();
            meshData.MaterialKey = building.RoofMaterial;

            for (int i = 0; i < triangles.Count; i += 3)
            {
                var p0 = footprint[triangles[i]];
                var v0 = new MapPoint(p0.X, p0.Y, roofOffset);

                var p1 = footprint[triangles[i + 2]];
                var v1 = new MapPoint(p1.X, p1.Y, roofOffset);

                var p2 = footprint[triangles[i + 1]];
                var v2 = new MapPoint(p2.X, p2.Y, roofOffset);

                meshData.AddTriangle(v0, v1, v2, GradientUtils.GetColor(gradient, v0, 0.2f));
            }

            return meshData;
        }
    }
}