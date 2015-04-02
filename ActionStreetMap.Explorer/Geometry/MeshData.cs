using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Unity;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry
{
    public class MeshData
    {
        /// <summary> Triangles. </summary>
        public List<MeshTriangle> Triangles;

        /// <summary> Material key. </summary>
        public string MaterialKey;

        /// <summary> Built game object. </summary>
        public IGameObject GameObject;

        public void AddTriangle(MapPoint v0, MapPoint v1, MapPoint v2, Color color)
        {
            Triangles.Add(new MeshTriangle()
            {
                Vertex0 = v0,
                Vertex1 = v1,
                Vertex2 = v2,
                Color0 = color,
                Color1 = color,
                Color2 = color,
            });
        }

        public void GenerateObjectData(out Vector3[] vertices, out int[] triangles, out Color[] colors)
        {
            var trisCount = Triangles.Count;
            var vertextCount = trisCount * 3;

            vertices = new Vector3[vertextCount];
            triangles = new int[vertextCount];
            colors = new Color[vertextCount];
            for (int i = 0; i < trisCount; i++)
            {
                var first = i * 3;
                var second = first + 1;
                var third = first + 2;
                var triangle = Triangles[i];
                var v0 = triangle.Vertex0;
                var v1 = triangle.Vertex1;
                var v2 = triangle.Vertex2;

                vertices[first] = new Vector3(v0.X, v0.Elevation, v0.Y);
                vertices[second] = new Vector3(v1.X, v1.Elevation, v1.Y);
                vertices[third] = new Vector3(v2.X, v2.Elevation, v2.Y);

                colors[first] = triangle.Color0;
                colors[second] = triangle.Color1;
                colors[third] = triangle.Color2;

                triangles[first] = third;
                triangles[second] = second;
                triangles[third] = first;
            }
        }
    }
}
