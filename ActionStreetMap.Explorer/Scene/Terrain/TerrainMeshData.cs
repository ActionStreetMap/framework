using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Customization;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> Provides the way to construct terrain mesh from triangles. </summary>
    internal sealed class TerrainMeshData: IDisposable
    {
        private readonly IObjectPool _objectPool;

        /// <summary> Material key. </summary>
        public string MaterialKey;

        /// <summary> Built game object. </summary>
        public IGameObject GameObject;

        /// <summary> Mesh index provides way to find affected vertices of given area quickly. </summary>
        public IMeshIndex Index;

        /// <summary> Triangles. </summary>
        public List<TerrainMeshTriangle> Triangles;

        /// <summary> Creates instance of <see cref="TerrainMeshData"/>. </summary>
        public TerrainMeshData(IObjectPool objectPool)
        {
            _objectPool = objectPool;
            Triangles = _objectPool.NewList<TerrainMeshTriangle>(2048);
        }

        /// <summary> Adds triangle with given parameters. </summary>
        public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color, 
            TextureGroup.Texture texture)
        {
            var triangle = _objectPool.NewObject<TerrainMeshTriangle>();
            triangle.Vertex0 = v0;
            triangle.Vertex1 = v1;
            triangle.Vertex2 = v2;
            triangle.Color = color;
            triangle.Texture = texture;

            triangle.Region = TerrainMeshTriangle.InvalidRegionIndex;

            Triangles.Add(triangle);
        }

        /// <summary> Generates arrays for mesh. </summary>
        public void GenerateObjectData(out Vector3[] vertices, out int[] triangles, 
            out Color[] colors, out Vector2[] uvs)
        {
            var trisCount = Triangles.Count;
            var vertextCount = trisCount * 3;

            vertices = new Vector3[vertextCount];
            triangles = new int[vertextCount];
            colors = new Color[vertextCount];
            uvs = new Vector2[vertextCount];
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

                colors[first] = triangle.Color;
                colors[second] = triangle.Color;
                colors[third] = triangle.Color;

                triangles[first] = third;
                triangles[second] = second;
                triangles[third] = first;

                uvs[first] = triangle.Texture.Map(new Vector2(0, 0));
                uvs[second] = triangle.Texture.Map(new Vector2(.5f, .5f));
                uvs[third] = triangle.Texture.Map(new Vector2(1, 0));

                _objectPool.StoreObject(triangle);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _objectPool.StoreList(Triangles);
            Triangles = null;
        }
    }
}
