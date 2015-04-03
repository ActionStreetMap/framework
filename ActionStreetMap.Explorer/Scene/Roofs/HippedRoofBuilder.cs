using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Utils;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary> Builds hipped roof. </summary>
    public class HippedRoofBuilder: RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "hipped"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public override MeshData Build(Building building)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var meshData = ObjectPool.CreateMeshData();
            meshData.MaterialKey = building.RoofMaterial;
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            var skeleton = StraightSkeleton.Calculate(building.Footprint);
            
            var skeletVertices = skeleton.Item1;
            skeletVertices.Reverse();

            for (int i = 0; i < skeletVertices.Count; i+=3)
            {
                var p0 = skeletVertices[i];
                var v0 =  new MapPoint(p0.x, p0.y, skeleton.Item2.Any(t => p0 == t) ? building.RoofHeight + roofOffset : roofOffset);

                var p1 = skeletVertices[i + 1];
                var v1 = new MapPoint(p1.x, p1.y, skeleton.Item2.Any(t => p1 == t) ? building.RoofHeight + roofOffset : roofOffset);

                var p2 = skeletVertices[i + 2];
                var v2 = new MapPoint(p2.x, p2.y, skeleton.Item2.Any(t => p2 == t) ? building.RoofHeight + roofOffset : roofOffset);

                meshData.AddTriangle(v0, v2, v1, GradientUtils.GetColor(gradient, v0, 0.2f));
            }

            return meshData;
        }
    }
}
