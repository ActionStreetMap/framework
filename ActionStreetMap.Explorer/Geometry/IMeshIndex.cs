using System.Collections.Generic;
using ActionStreetMap.Core;

namespace ActionStreetMap.Explorer.Geometry
{
    public interface IMeshIndex
    {
        void AddTriangle(MeshTriangle triangle);
        void Build();

        List<int> GetAfectedIndices(MapPoint center, float radius, out MapPoint direction);
    }
}
