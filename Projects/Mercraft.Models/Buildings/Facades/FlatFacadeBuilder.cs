﻿using System.Linq;
using Mercraft.Core;
using Mercraft.Core.Algorithms;
using Mercraft.Core.World.Buildings;
using Mercraft.Models.Utils;
using UnityEngine;

namespace Mercraft.Models.Buildings.Facades
{
    public class FlatFacadeBuilder : IFacadeBuilder
    {
        public string Name { get { return "flat"; } }

        public MeshData Build(Building building, BuildingStyle style)
        {
            var vertices2D = building.Footprint;

            var meshData = new MeshData()
            {
                Vertices = GetVerticies3D(vertices2D, building.Elevation + building.MinHeight, building.Height),
                Triangles = GetTriangles3D(vertices2D),
                UV = GetUV(vertices2D),
                MaterialKey = style.Facade.Path
            };

            AttachFloor(building, meshData);

            return meshData;
        }

        private Vector3[] GetVerticies3D(MapPoint[] mapPoints, float elevation, float height)
        {
            var length = mapPoints.Length;
            var verticies3D = new Vector3[length * 4 + length];
            for (int i = 0; i < length; i++)
            {
                var v3DIndex = i * 4;
                var v2DIndex = i == (length - 1) ? 0 : i + 1;

                verticies3D[v3DIndex] = new Vector3(mapPoints[i].X, elevation, mapPoints[i].Y);

                verticies3D[v3DIndex + 1] = new Vector3(mapPoints[v2DIndex].X, elevation, mapPoints[v2DIndex].Y);
                verticies3D[v3DIndex + 2] = new Vector3(mapPoints[v2DIndex].X, elevation + height, mapPoints[v2DIndex].Y);

                verticies3D[v3DIndex + 3] = new Vector3(mapPoints[i].X, elevation + height, mapPoints[i].Y);
            }

            return verticies3D;
        }

        private int[] GetTriangles3D(MapPoint[] verticies2D)
        {
            var length = verticies2D.Count();
            var triangles = new int[(length) * 2 * 3 + (length - 2) * 3];

            for (int i = 0; i < length; i++)
            {
                var tIndex = i * 6;
                var vIndex = i * 4;
                triangles[tIndex] = vIndex;
                triangles[tIndex + 1] = vIndex + 1;
                triangles[tIndex + 2] = vIndex + 2;

                triangles[tIndex + 3] = vIndex + 3;
                triangles[tIndex + 4] = vIndex + 0;
                triangles[tIndex + 5] = vIndex + 2;
            }

            return triangles;
        }

        private Vector2[] GetUV(MapPoint[] verticies2D)
        {
            var length = verticies2D.Length;
            var uv = new Vector2[length * 4 + length];

            for (int i = 0; i < length; i++)
            {
                var vIndex = i * 4;
                uv[vIndex] = new Vector2(1, 0);
                uv[vIndex + 1] = new Vector2(0, 0);
                uv[vIndex + 2] = new Vector2(0, 1);
                uv[vIndex + 3] = new Vector2(1, 1);
            }

            return uv;
        }

        private void AttachFloor(Building building, MeshData meshData)
        {
            var points = building.Footprint;
            var length = points.Length;
            var elevation = meshData.Vertices[0].y;
            var startVertexIndex = length * 4;

            // attach vertices
            for (int i = 0; i < length; i++)
                meshData.Vertices[startVertexIndex + i] = new Vector3(points[i].X, elevation, points[i].Y);

            // attach triangles
            var startTriangleIndex = length * 2 * 3;
            var triangles = Triangulator.Triangulate(points, false);
            for (int i = 0; i < triangles.Length; i++)
                meshData.Triangles[startTriangleIndex + i] = triangles[i] + startVertexIndex;
        }
    }
}
