using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.Polygons;
using UnityEngine;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Scene.Buildings.Facades
{
    /// <summary>
    ///     Builds flat facade.
    /// </summary>
    public class FlatFacadeBuilder : IFacadeBuilder
    {
        /// <inheritdoc />
        public string Name { get { return "flat"; } }

        /// <inheritdoc />
        [Dependency]
        public IObjectPool ObjectPool { get; set; }

        /// <inheritdoc />
        public MeshData Build(Building building, BuildingStyle style)
        {
            var vertices2D = building.Footprint;

            var meshData = new MeshData
            {
                Vertices = GetVerticies3D(vertices2D, building.Elevation + building.MinHeight, building.Height),
                Triangles = GetTriangles3D(vertices2D),
                UV = GetUV(style, vertices2D),
                MaterialKey = style.Facade.Path
            };

            AttachFloor(building, meshData);

            return meshData;
        }

        private List<Vector3> GetVerticies3D(List<MapPoint> mapPoints, float elevation, float height)
        {
            var length = mapPoints.Count;
            var verticies3D = new List<Vector3>(length * 4 + length);
            for (int i = 0; i < length; i++)
            {
                var v2DIndex = i == (length - 1) ? 0 : i + 1;

                verticies3D.Add(new Vector3(mapPoints[i].X, elevation, mapPoints[i].Y));
                verticies3D.Add(new Vector3(mapPoints[v2DIndex].X, elevation, mapPoints[v2DIndex].Y));
                verticies3D.Add(new Vector3(mapPoints[v2DIndex].X, elevation + height, mapPoints[v2DIndex].Y));
                verticies3D.Add(new Vector3(mapPoints[i].X, elevation + height, mapPoints[i].Y));
            }

            return verticies3D;
        }

        private List<int> GetTriangles3D(List<MapPoint> verticies2D)
        {
            var length = verticies2D.Count;
            var triangles = new List<int>((length) * 2 * 3 + (length - 2) * 3);

            for (int i = 0; i < length; i++)
            {
                var vIndex = i * 4;
                triangles.Add(vIndex);
                triangles.Add(vIndex + 1);
                triangles.Add(vIndex + 2);

                triangles.Add(vIndex + 3);
                triangles.Add(vIndex + 0);
                triangles.Add(vIndex + 2);
            }

            return triangles;
        }

        private List<Vector2> GetUV(BuildingStyle style, List<MapPoint> verticies2D)
        {
            var leftBottom = style.Facade.FrontUvMap.LeftBottom;
            var rightUpper = style.Facade.FrontUvMap.RightUpper;

            var uv = new List<Vector2>(verticies2D.Count * 4 + verticies2D.Count);

            for (int i = 0; i < verticies2D.Count; i++)
            {
                uv.Add(new Vector2(rightUpper.x, leftBottom.y));
                uv.Add(leftBottom);
                uv.Add(new Vector2(leftBottom.x, rightUpper.y));
                uv.Add(rightUpper);
            }

            return uv;
        }

        private void AttachFloor(Building building, MeshData meshData)
        {
            var points = building.Footprint;
            var length = points.Count;
            var elevation = meshData.Vertices[0].y;
            var startVertexIndex = length * 4;

            // attach vertices
            for (int i = 0; i < length; i++)
                meshData.Vertices.Add(new Vector3(points[i].X, elevation, points[i].Y));

            for (int i = 0; i < length; i++)
                meshData.UV.Add(new Vector2());

            var triangles = ObjectPool.NewList<int>();
            Triangulator.Triangulate(building.Footprint, triangles);
            
            for (int i = 0; i < triangles.Count; i++)
                meshData.Triangles.Add(triangles[i] + startVertexIndex);
        }
    }
}
