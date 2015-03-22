using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Polygons;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Buildings.Roofs
{
    /// <summary> Builds flat roof. </summary>
    public class FlatRoofBuilder : IRoofBuilder
    {
        /// <inheritdoc />
        public string Name { get { return "flat"; } }

        /// <inheritdoc />
        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        /// <summary> Flat builder can be used for every type of building. </summary>
        /// <param name="building"> Building. </param>
        /// <returns> Always true. </returns>
        public bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public MeshData Build(Building building)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            var triangles = ObjectPool.NewList<int>();
            Triangulator.Triangulate(building.Footprint, triangles);

            return new MeshData
            {
                Vertices = GetVertices(building.Footprint, roofOffset),
                Triangles = triangles,
                Colors = GetColors(building),
                MaterialKey = building.RoofMaterial
            };
        }

        private List<Vector3> GetVertices(List<MapPoint> footprint, float roofOffset)
        {
            var length = footprint.Count;
            var vertices3D = new List<Vector3>(length);

            for (int i = 0; i < length; i++)
                vertices3D.Add(new Vector3(footprint[i].X, roofOffset, footprint[i].Y));

            return vertices3D;
        }

        private List<Color> GetColors(Building building)
        {
            var count = building.Footprint.Count;
            var colors = new List<Color>(count);
            var color = building.RoofColor.ToUnityColor();
            for (int i = 0; i < count; i++)
                colors.Add(color);
            return colors;
        }
    }
}