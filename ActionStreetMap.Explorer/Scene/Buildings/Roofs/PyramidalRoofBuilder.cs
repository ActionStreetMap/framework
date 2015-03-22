using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
using ActionStreetMap.Explorer.Infrastructure;
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
        public MeshData Build(Building building)
        {
            var roofOffset = building.Elevation + building.MinHeight + building.Height;
            
            var center = PolygonUtils.GetCentroid(building.Footprint);

            return new MeshData()
            {
                Vertices = GetVertices(center, building.Footprint, roofOffset, building.RoofHeight),
                Triangles = GetTriangles(building.Footprint),
                Colors = GetColors(building),
                MaterialKey = building.RoofMaterial,
            };
        }

        private List<int> GetTriangles(List<MapPoint> footprint)
        {
            var length = footprint.Count * 3;
            var triangles = new List<int>(length);

            for (int i = 0; i < length; i++)
                triangles.Add(i);

            return triangles;
        }

        private List<Vector3> GetVertices(MapPoint center, List<MapPoint> footprint, float roofOffset, float roofHeight)
        {
            var length = footprint.Count;
            var verticies = new List<Vector3>(length*3);
            for (int i = 0; i < length; i++)
            {
                var nextIndex = i == (length - 1) ? 0 : i + 1;

                verticies.Add(new Vector3(footprint[i].X, roofOffset, footprint[i].Y));
                verticies.Add(new Vector3(footprint[nextIndex].X, roofOffset, footprint[nextIndex].Y));
                verticies.Add(new Vector3(center.X, roofOffset + roofHeight, center.Y));
            }

            return verticies;
        }

        private List<Color> GetColors(Building building)
        {
            var length = building.Footprint.Count * 3;
            var colors = new List<Color>(length);
            var color = building.RoofColor.ToUnityColor();
            for (int i = 0; i < length; i++)
                colors.Add(color);

            return colors;
        }
    }
}
