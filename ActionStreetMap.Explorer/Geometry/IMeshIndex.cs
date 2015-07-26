using System;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry
{
    /// <summary> Represents index of mesh vertices for quick search. </summary>
    public interface IMeshIndex
    {
        /// <summary> Builds index. </summary>
        void Build();

        /// <summary> Performs query which represented by circle with given center and radius. </summary>
        /// <param name="center">Center of affected area. </param>
        /// <param name="radius">Radius of affected area. </param>
        /// <param name="vertices">Mesh vertices. </param>
        /// <param name="modifyAction">
        ///     Modify action: first parameter is vertex index, second - distance to center, third -
        ///     direction of force.
        /// </param>
        void Query(Vector3 center, float radius, Vector3[] vertices, Action<int, float, Vector3> modifyAction);
    }

    /// <summary> Represents mesh index which does nothing. </summary>
    internal sealed class DummyMeshIndex : IMeshIndex
    {
        public static readonly  DummyMeshIndex Default = new DummyMeshIndex();
        private DummyMeshIndex() { }

        /// <inheritdoc />
        public void Build()
        {
        }

        /// <inheritdoc />
        public void Query(Vector3 center, float radius, Vector3[] vertices, Action<int, float, Vector3> modifyAction)
        {
        }
    }
}
