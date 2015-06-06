using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Utils
{
    internal static class ObjectPoolExtensions
    {
        public static MeshData CreateMeshData(this IObjectPool objectPool, int capacity = 256)
        {
            return new MeshData(objectPool)
            {
                Triangles = objectPool.NewList<MeshTriangle>(capacity)
            };
        }

        public static void RecycleMeshData(this IObjectPool objectPool, MeshData meshData)
        {
            objectPool.StoreList(meshData.Triangles);
        }
    }
}
