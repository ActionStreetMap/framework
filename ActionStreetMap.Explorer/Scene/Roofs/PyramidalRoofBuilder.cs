using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
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
    public class PyramidalRoofBuilder: RoofBuilder
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
        public override MeshData Build(Building building)
        {
            var center = PolygonUtils.GetCentroid(building.Footprint);
            var meshData = new MeshData();
            var gradient = ResourceProvider.GetGradient(building.RoofColor);

            SetMeshData(building, meshData, gradient, center);
            return meshData;
        }

        private void SetMeshData(Building building, MeshData meshData, GradientWrapper gradient, MapPoint center)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;

            var footprint = building.Footprint;
            var roofHeight = building.RoofHeight;

            var length = footprint.Count;
            var verticies = new List<Vector3>(length*3);
            var colors = new List<Color>(length*3);
            for (int i = 0; i < length; i++)
            {
                var nextIndex = i == (length - 1) ? 0 : i + 1;

                var v0 = new Vector3(footprint[i].X, roofOffset, footprint[i].Y);
                verticies.Add(v0);
                colors.Add(GradientUtils.GetColor(gradient, v0, 0.2f));

                var v1 = new Vector3(footprint[nextIndex].X, roofOffset, footprint[nextIndex].Y);
                verticies.Add(v1);
                colors.Add(GradientUtils.GetColor(gradient, v1, 0.2f));

                var v2 = new Vector3(center.X, roofOffset + roofHeight, center.Y);
                verticies.Add(v2);
                colors.Add(GradientUtils.GetColor(gradient, v2, 0.2f));
            }

            meshData.Vertices = verticies;
            meshData.Triangles = GetTriangles(building.Footprint);
            meshData.Colors = colors;
            meshData.MaterialKey = building.RoofMaterial;
        }

        private List<int> GetTriangles(List<MapPoint> footprint)
        {
            var length = footprint.Count * 3;
            var triangles = new List<int>(length);

            for (int i = 0; i < length; i++)
                triangles.Add(i);

            return triangles;
        }
    }
}
