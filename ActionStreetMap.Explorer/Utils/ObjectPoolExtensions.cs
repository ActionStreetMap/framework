using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Infrastructure.Utilities;
using UnityEngine;

namespace ActionStreetMap.Explorer.Utils
{
    internal static class ObjectPoolExtensions
    {
        public static MeshData CreateMeshData(this IObjectPool objectPool, int vertexCount, int trisCount, int colorCount)
        {
            return new MeshData()
            {
                Vertices = objectPool.NewList<Vector3>(vertexCount),
                Triangles = objectPool.NewList<int>(trisCount),
                Colors = objectPool.NewList<Color>(colorCount)
            };
        }

        public static void RecycleMeshData(this IObjectPool objectPool, MeshData meshData)
        {
            objectPool.StoreList(meshData.Vertices);
            objectPool.StoreList(meshData.Triangles);
            objectPool.StoreList(meshData.Colors);
        }
    }
}
