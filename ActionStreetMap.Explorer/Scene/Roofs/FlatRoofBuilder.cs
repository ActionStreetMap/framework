using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Utils;
using UnityEngine;

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
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            var triangles = ObjectPool.NewList<int>();
            Triangulator.Triangulate(building.Footprint, triangles);

            var footprint = building.Footprint;
            var count = building.Footprint.Count;
            var vertices = new List<Vector3>(count);
            var colors = new List<Color>(count);
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            for (int i = 0; i < count; i++)
            {
                var vertex = new Vector3(footprint[i].X, roofOffset, footprint[i].Y);
                vertices.Add(vertex);
                colors.Add(GradientUtils.GetColor(gradient, vertex, 0.2f));
            }

            return new MeshData
            {
                Vertices = vertices,
                Triangles = triangles,
                Colors = colors,
                MaterialKey = building.RoofMaterial
            };
        }
    }
}