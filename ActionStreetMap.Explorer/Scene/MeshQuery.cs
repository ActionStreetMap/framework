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

        /// <summary> DTO for mesh query operation results. </summary>
        public class Result
        {
            /// <summary> Amound of modifide vertices. </summary>
            public int ModifiedVertices;

            /// <summary> Amount of scanned triangles. </summary>
            public int ScannedTriangles;

            /// <summary> Result vertices. </summary>
            public readonly Vector3[] Vertices;

            /// <summary> True if any of vertices is modified. </summary>
            public bool IsModified { get { return ModifiedVertices > 0; } }

            /// <summary> True if mesh is marked as destroyed. </summary>
            public bool IsDestroyed { get; set; }

            /// <summary> Creates instance of <see cref="Result"/>. </summary>
            public Result(Vector3[] vertices)
            {
                Vertices = vertices;
            }
        }
    }
}
