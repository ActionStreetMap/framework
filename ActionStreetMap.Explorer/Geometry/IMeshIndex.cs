using System;
using ActionStreetMap.Core;
using UnityEngine;

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
        /// <param name="radius">Radius of area. </param>
        /// <param name="vertices">Mesh vertices. </param>
        /// <param name="modifyAction">
        ///     Modify action: first parameter is vertex index, second - distance to center, third -
        ///     direction of force.
        /// </param>
        void Query(MapPoint center, float radius, Vector3[] vertices, Action<int, float, Vector2> modifyAction);
    }
}