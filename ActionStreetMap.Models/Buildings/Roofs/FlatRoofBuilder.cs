using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Models.Geometry;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Infrastructure.Dependencies;

using UnityEngine;

namespace ActionStreetMap.Models.Buildings.Roofs
{
    /// <summary>
    ///     Builds flat roof.
    /// </summary>
    public class FlatRoofBuilder: IRoofBuilder
    {
        /// <inheritdoc />
        public string Name { get { return "flat"; } }

        /// <inheritdoc />
        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        /// <summary>
        ///     Flat builder can be used for every type of building.
        /// </summary>
        /// <param name="building">Building.</param>
        /// <returns>Always true.</returns>
        public bool CanBuild(Building building) { return true; }

        /// <inheritdoc />
        public MeshData Build(Building building, BuildingStyle style)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            var buffer = ObjectPool.NewList<int>();
            var triangles = Triangulator.Triangulate(building.Footprint, buffer);
            ObjectPool.Store(buffer);

            return new MeshData
            {
                Vertices = GetVerticies3D(building.Footprint, roofOffset),
                Triangles = triangles,
                UV = GetUV(building.Footprint, style),
                MaterialKey = style.Roof.Path,
            };
        }

        private Vector3[] GetVerticies3D(List<MapPoint> footprint, float roofOffset)
        {
            var length = footprint.Count;
            var vertices3D = new Vector3[length];
            
            for (int i = 0; i < length; i++)
                vertices3D[i] = new Vector3(footprint[i].X, roofOffset, footprint[i].Y);

            return vertices3D;
        }

        private Vector2[] GetUV(List<MapPoint> footprint, BuildingStyle style)
        {
            var uv = new Vector2[footprint.Count];
            for (int i = 0; i < uv.Length; i++)
                uv[i] = style.Roof.FrontUvMap.RightUpper;

            return uv;
        }
    }
}
