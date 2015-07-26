using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Roofs
{
    /// <summary>
    ///     Builds Pyramidal roof.
    ///     See http://wiki.openstreetmap.org/wiki/Key:roof:shape#Roof
    /// </summary>
    internal class PyramidalRoofBuilder: RoofBuilder
    {
        /// <inheritdoc />
        public override string Name { get { return "pyramidal"; } }

        /// <inheritdoc />
        public override bool CanBuild(Building building)
        {
            // TODO actually, we cannot use pyramidal for non-convex polygons
            return true;
        }

        /// <inheritdoc />
        public override List<MeshData> Build(Building building)
        {
            var center = PolygonUtils.GetCentroid(building.Footprint);
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            var meshData = CreateMeshData(building, gradient, center);
            return new List<MeshData>(1)
            {
                meshData
            };
        }

        private MeshData CreateMeshData(Building building, GradientWrapper gradient, MapPoint center)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            var footprint = building.Footprint;
            var roofHeight = building.RoofHeight;

            var length = footprint.Count;
            var meshData = new MeshData()
            {
                Vertices = new Vector3[length*3],
                Triangles = new int[length*3],
                Colors = new Color[length*3],
            };

            for (int i = 0; i < length; i++)
            {
                var nextIndex = i == (length - 1) ? 0 : i + 1;

                var v0 = new Vector3(footprint[i].X, roofOffset, footprint[i].Y);
                var v1 = new Vector3(center.X, roofOffset + roofHeight, center.Y);
                var v2 = new Vector3(footprint[nextIndex].X, roofOffset, footprint[nextIndex].Y);

                var color = GradientUtils.GetColor(gradient, v0, 0.2f);

                meshData.AddTriangle(v0, v1, v2, color);
            }

            return meshData;
        }
    }
}
