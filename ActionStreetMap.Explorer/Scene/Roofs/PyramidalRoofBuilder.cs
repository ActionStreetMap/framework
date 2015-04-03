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
            var meshData = ObjectPool.CreateMeshData();
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
            for (int i = 0; i < length; i++)
            {
                var nextIndex = i == (length - 1) ? 0 : i + 1;

                var v0 = new MapPoint(footprint[i].X, footprint[i].Y, roofOffset);
                var v1 = new MapPoint(center.X, center.Y, roofOffset + roofHeight);
                var v2 = new MapPoint(footprint[nextIndex].X, footprint[nextIndex].Y, roofOffset);

                var color = GradientUtils.GetColor(gradient, v0, 0.2f);

                meshData.AddTriangle(v0, v1, v2, color);
            }
            meshData.MaterialKey = building.RoofMaterial;
        }
    }
}
