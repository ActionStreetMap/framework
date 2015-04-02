using System.Collections.Generic;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Utils
{
    internal static class ObjectPoolExtensions
    {
        public static MeshData CreateMeshData(this IObjectPool objectPool, int vertexCount = 128, int trisCount = 256, int colorCount = 128)
        {
            return new MeshData()
            {
                Vertices = objectPool.NewList<Vector3>(vertexCount),
                Triangles = objectPool.NewList<int>(trisCount),
                Colors = objectPool.NewList<Color>(colorCount)
            };
        }

        public static MeshDataEx CreateMeshDataEx(this IObjectPool objectPool, int capacity = 256)
        {
            return new MeshDataEx()
            {
                Triangles = new List<MeshTriangle>(capacity)
            };
        }

        public static void RecycleMeshData(this IObjectPool objectPool, MeshData meshData)
        {
            objectPool.StoreList(meshData.Vertices);
            objectPool.StoreList(meshData.Triangles);
            objectPool.StoreList(meshData.Colors);
        }

        public static void RecycleMeshDataEx(this IObjectPool objectPool, MeshDataEx meshData)
        {
            objectPool.StoreList(meshData.Triangles);
        }
    }
}
