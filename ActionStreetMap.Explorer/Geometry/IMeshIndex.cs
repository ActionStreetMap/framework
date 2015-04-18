using System.Collections.Generic;
using ActionStreetMap.Core;

namespace ActionStreetMap.Explorer.Geometry
{
    /// <summary> Represents index of mesh's triangles. </summary>
    public interface IMeshIndex
    {
        /// <summary> Adds triangle to index. </summary>
        void AddTriangle(MeshTriangle triangle);

        /// <summary> Builds index. </summary>
        void Build();

        /// <summary> Performs query which represented by circle with given center and radius. </summary>
        /// <param name="center">Center of affected area. </param>
        /// <param name="radius">Radius of area.</param>
        /// <param name="direction">Direction of force. </param>
        /// <returns> List of affected triangles. </returns>
        List<int> Query(MapPoint center, float radius, out MapPoint direction);
    }
}
