using System;
using ActionStreetMap.Core;
using ActionStreetMap.Explorer.Geometry;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Terrain
{
    /// <summary> Dummy mesh index. </summary>
    internal class DummyMeshIndex: IMeshIndex
    {
        /// <inheritdoc />
        public void AddTriangle(MeshTriangle triangle)
        {
        }

        /// <inheritdoc />
        public void Build()
        {
        }

        /// <inheritdoc />
        public void Query(MapPoint center, float radius, Vector3[] vertices, Action<int, float, Vector2> modifyAction)
        {
            throw new NotSupportedException();
        }
    }
}
