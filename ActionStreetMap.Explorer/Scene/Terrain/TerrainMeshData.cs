using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    internal sealed class TerrainMeshData: IDisposable
    {
        private readonly IObjectPool _objectPool;

        /// <summary> Material key. </summary>
        public string MaterialKey;

        /// <summary> Built game object. </summary>
        public IGameObject GameObject;

        /// <summary> Mesh index provides way to find affected vertices of given area quickly. </summary>
        internal IMeshIndex Index;

        /// <summary> Triangles. </summary>
        internal List<TerrainMeshTriangle> Triangles;

        /// <summary> Creates instance of <see cref="TerrainMeshData"/>. </summary>
        /// <param name="objectPool">Object pool.</param>
        public TerrainMeshData(IObjectPool objectPool)
        {
            _objectPool = objectPool;
            Triangles = _objectPool.NewList<TerrainMeshTriangle>(2048);
        }

        public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color)
        {
            var triangle = _objectPool.NewObject<TerrainMeshTriangle>();
            triangle.Vertex0 = v0;
            triangle.Vertex1 = v1;
            triangle.Vertex2 = v2;
            triangle.Color0 = color;
            triangle.Color1 = color;
            triangle.Color2 = color;
            triangle.Region = 0;

            Triangles.Add(triangle);
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

                vertices[first] = v0;
                vertices[second] = v1;
                vertices[third] = v2;

                colors[first] = triangle.Color0;
                colors[second] = triangle.Color1;
                colors[third] = triangle.Color2;

                triangles[first] = third;
                triangles[second] = second;
                triangles[third] = first;

                _objectPool.StoreObject<TerrainMeshTriangle>(triangle);
            }
        }

        public void Dispose()
        {
            _objectPool.StoreList(Triangles);
            Triangles = null;
        }
    }
}
