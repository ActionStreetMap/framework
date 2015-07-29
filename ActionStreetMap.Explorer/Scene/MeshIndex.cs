using System;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Represents index of mesh vertices for quick search. </summary>
    public interface IMeshIndex
    {
        /// <summary> Builds index. </summary>
        void Build();

        /// <summary> Modifies mesh using query provided. </summary>
        bool Modify(MeshQuery query);
    }

    /// <summary> Mesh index for plane. </summary>
    internal class PlaneMeshIndex : IMeshIndex
    {
        /// <summary> Normal vector to plane. </summary>
        protected readonly Vector3 N;

        /// <summary> Magnitude of normal vector. </summary>
        protected readonly float NormalMagnitude;

        /// <summary> Coefficient from plane equation. </summary>
        protected readonly float D;

        /// <summary> Initializes index using three points on the plane. </summary>
        public PlaneMeshIndex(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            N = Vector3.Cross(p2 - p1, p3 - p1);
            NormalMagnitude = N.magnitude;
            D = p1.x*N.x + p1.y*N.y + p1.z*N.z;
        }

        /// <inheritdoc />
        public void Build()
        {
            // TODO try to create some index internally to avoid iteration
            // over whole collection of vertices
        }

        /// <inheritdoc />
        public bool Modify(MeshQuery query)
        {
            var vertices = query.Vertices;
            bool isModified = false;
            for (int j = 0; j < vertices.Length; j += 3)
            {
                // triangle is already collapsed
                if (vertices[j] == vertices[j + 1])
                    continue;

                for (int i = j; i < j + 3; i++)
                {
                    var v = vertices[i];
                    var distanceToCollidePoint = Vector3.Distance(v, query.CollidePoint);
                    if (distanceToCollidePoint < query.Radius)
                    {
                        isModified = true;
                        var distanceToWall = (v.x * N.x + v.y * N.y + v.z * N.z - D) / NormalMagnitude;
                        var forceChange = query.ForcePower * (query.Radius - distanceToCollidePoint) / 2;

                        // NOTE whole traingle should be removed as one of the vertices is 
                        // moved more than threshold allows
                        if (Math.Abs(distanceToWall + forceChange) > query.OffsetThreshold)
                        {
                            // collapse triangle into point
                            var firstVertIndex = i - i % 3;
                            vertices[firstVertIndex + 1] = vertices[firstVertIndex];
                            vertices[firstVertIndex + 2] = vertices[firstVertIndex];
                            break;
                        }
                        vertices[i] += forceChange * query.ForceDirection;
                    }
                }
            }
            return isModified;
        }
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
        public bool Modify(MeshQuery query)
        {
            return false;
        }
    }
}
