using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Represents DTO which is used to modify mesh. </summary>
    public class MeshQuery
    {
        /// <summary> Epicenter of modification. </summary>
        public Vector3 Epicenter;

        /// <summary> Radious of modification. </summary>
        public float Radius;

        /// <summary> Offset threshold used to destroy triangles. </summary>
        public float OffsetThreshold;

        /// <summary> Force power. </summary>
        public float ForcePower;

        /// <summary> Force direction. </summary>
        public Vector3 ForceDirection;

        /// <summary> Collide point on mesh </summary>
        public Vector3 CollidePoint;

        /// <summary> Mesh vertices. </summary>
        public Vector3[] Vertices;
    }
}
