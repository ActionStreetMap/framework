using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Utils;
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

        private List<int> GetTriangles(List<MapPoint> footprint)
        {
            var length = footprint.Count * 3;
            var triangles = new List<int>(length);

            for (int i = 0; i < length; i++)
                triangles.Add(i);

            return triangles;
        }

        private List<Vector3> GetVerticies3D(MapPoint center, List<MapPoint> footprint, float roofOffset, float roofHeight)
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

        private List<Vector2> GetUV(List<MapPoint> footprint, BuildingStyle style)
        {
            var length = footprint.Count*3;
            var uv = new List<Vector2>(length);
            for (int i = 0; i < length; i++)
                uv.Add(style.Roof.FrontUvMap.RightUpper);

            return uv;
        }
    }
}
