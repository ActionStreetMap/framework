using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Indices
{
    /// <summary> Mesh index for sphere. </summary>
    internal class SphereMeshIndex : IMeshIndex
    {
        private readonly float _radius;
        private readonly Vector3 _center;

        /// <summary> Creates instance of <see cref="SphereMeshIndex"/>. </summary>
        public SphereMeshIndex(float radius, Vector3 center)
        {
            _radius = radius;
            _center = center;
        }

        /// <inheritdoc />
        public void Build()
        {
        }

        /// <inheritdoc />
        public MeshQuery.Result Modify(MeshQuery query)
        {
            return null;
        }
    }
}
