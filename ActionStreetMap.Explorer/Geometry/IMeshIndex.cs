using System.Collections.Generic;
using ActionStreetMap.Core;

namespace ActionStreetMap.Explorer.Geometry
{
    public interface IMeshIndex
    {
        void AddTriangle(MeshTriangle triangle);
        void Build(MeshData meshData);
        List<int> GetAfectedIndices(MapPoint center, float radius);
    }
}
