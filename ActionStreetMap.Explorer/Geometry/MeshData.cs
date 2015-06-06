using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry
{
    public class MeshData
    {
        private readonly IObjectPool _objectPool;

        /// <summary> Triangles. </summary>
        public List<MeshTriangle> Triangles;

        /// <summary> Material key. </summary>
        public string MaterialKey;

        /// <summary> Built game object. </summary>
        public IGameObject GameObject;

        /// <summary> Mesh index provides way to find affected vertices of given area quickly. </summary>
        public IMeshIndex Index;

        /// <summary> Creates instance of <see cref="MeshData"/>. </summary>
        /// <param name="objectPool">Object pool.</param>
        public MeshData(IObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        public void AddTriangle(MapPoint v0, MapPoint v1, MapPoint v2, Color color)
        {
            var triangle = _objectPool.NewObject<MeshTriangle>();
            triangle.Vertex0 = v0;
            triangle.Vertex1 = v1;
            triangle.Vertex2 = v2;
            triangle.Color0 = color;
            triangle.Color1 = color;
            triangle.Color2 = color;

            Triangles.Add(triangle);

            if(Index != null)
                Index.AddTriangle(triangle);
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

                _objectPool.StoreObject<MeshTriangle>(triangle);
            }
        }
    }
}
