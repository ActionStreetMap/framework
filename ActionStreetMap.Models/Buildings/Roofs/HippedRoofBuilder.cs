using System.Linq;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Models.Geometry.Polygons;
using UnityEngine;

namespace ActionStreetMap.Models.Buildings.Roofs
{
    /// <summary>
    ///     Builds hipped roof.
    /// </summary>
    public class HippedRoofBuilder: IRoofBuilder
    {
        /// <inheritdoc />
        public virtual string Name { get { return "hipped"; } }

        /// <inheritdoc />
        public virtual bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public virtual MeshData Build(Building building, BuildingStyle style)
        {
            var roofHeight = building.RoofHeight > 0 ? building.RoofHeight : style.Roof.Height;
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            var skeleton = StraightSkeleton.Calculate(building.Footprint);
            
            var skeletVertices = skeleton.Item1;
            skeletVertices.Reverse();

            var vertices = new Vector3[skeletVertices.Count];
            var triangles = new int[skeletVertices.Count];
            var uv = new Vector2[skeletVertices.Count];

            for (int i = 0; i < skeletVertices.Count; i++)
            {
                var vertex = skeletVertices[i];
                var y = skeleton.Item2.Any(t => vertex == t) ? roofHeight + roofOffset : roofOffset;
                vertices[i].Set(vertex.x, y, vertex.y);
                triangles[i] = i;
                uv[i] = style.Roof.FrontUvMap.RightUpper;
            }
           
            return new MeshData()
            {
                Vertices = vertices,
                Triangles = triangles,
                UV = uv,
                MaterialKey = style.Roof.Path,
            };
        }
    }
}
