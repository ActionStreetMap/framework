using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Scene.Geometry;
using ActionStreetMap.Explorer.Scene.Geometry.Polygons;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Buildings.Roofs
{
    /// <summary>
    ///     Builds Pyramidal roof.
    ///     See http://wiki.openstreetmap.org/wiki/Key:roof:shape#Roof
    /// </summary>
    public class PyramidalRoofBuilder: IRoofBuilder
    {
        /// <inheritdoc />
        public string Name { get { return "pyramidal"; } }

        /// <inheritdoc />
        public bool CanBuild(Building building)
        {
            // TODO actually, we cannot use pyramidal for non-convex polygons
            return true;
        }

        /// <inheritdoc />
        public MeshData Build(Building building, BuildingStyle style)
        {
            var roofHeight = (building.RoofHeight > 0 ? building.RoofHeight : style.Roof.Height);
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            
            var center = PolygonUtils.GetCentroid(building.Footprint);

            return new MeshData()
            {
                Vertices = GetVerticies3D(center, building.Footprint, roofOffset, roofHeight),
                Triangles = GetTriangles(building.Footprint),
                UV = GetUV(building.Footprint, style),
                MaterialKey = style.Roof.Path,
            };
        }

        private int[] GetTriangles(List<MapPoint> footprint)
        {
            var length = footprint.Count * 3;
            var triangles = new int[length];

            for (int i = 0; i < length; i++)
                triangles[i] = i;

            return triangles;
        }

        private Vector3[] GetVerticies3D(MapPoint center, List<MapPoint> footprint, float roofOffset, float roofHeight)
        {
            var length = footprint.Count;
            var verticies = new Vector3[length*3];
            for (int i = 0; i < length; i++)
            {
                var vertIndex = i*3;
                var nextIndex = i == (length - 1) ? 0 : i + 1;

                verticies[vertIndex] = new Vector3(footprint[i].X, roofOffset, footprint[i].Y);
                verticies[vertIndex + 1] = new Vector3(footprint[nextIndex].X, roofOffset, footprint[nextIndex].Y);
                verticies[vertIndex + 2] = new Vector3(center.X, roofOffset + roofHeight, center.Y);
            }

            return verticies;
        }

        private Vector2[] GetUV(List<MapPoint> footprint, BuildingStyle style)
        {
            var uv = new Vector2[footprint.Count * 3];
            for (int i = 0; i < uv.Length; i++)
                uv[i] = style.Roof.FrontUvMap.RightUpper;

            return uv;
        }
    }
}
