using System;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Indices
{
    /// <summary> Mesh index for plane. </summary>
    internal class PlaneMeshIndex : IMeshIndex
    {
        /// <summary> Normal vector to plane. </summary>
        private readonly Vector3 _n;

        /// <summary> Magnitude of normal vector. </summary>
        private readonly float _normalMagnitude;

        /// <summary> Coefficient from plane equation. </summary>
        private readonly float _d;

        /// <summary> Creates instance of <see cref="PlaneMeshIndex"/>. </summary>
        protected PlaneMeshIndex()
        {
        }

        /// <summary> Initializes index using three points on the plane. </summary>
        public PlaneMeshIndex(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            CalculateParams(p1, p2, p3, out _n, out _normalMagnitude, out _d);
        }

        /// <inheritdoc />
        public virtual void Build()
        {
            // TODO try to create some index internally to avoid iteration
            // over whole collection of vertices
        }

        /// <inheritdoc />
        public virtual MeshQuery.Result Modify(MeshQuery query)
        {
            return Modify(query, 0, query.Vertices.Length / 2, _n, _normalMagnitude, _d);
        }

        /// <summary> Calcualtes plane equation parameter based on three points. </summary>
        protected void CalculateParams(Vector3 p1, Vector3 p2, Vector3 p3,
            out Vector3 n, out float magnitude, out float d)
        {
            n = Vector3.Cross(p2 - p1, p3 - p1);
            magnitude = n.magnitude;
            d = p1.x * n.x + p1.y * n.y + p1.z * n.z;
        }

        /// <summary> Modifies mesh plane based on it's equation parameters. </summary>
        protected MeshQuery.Result Modify(MeshQuery query, int startIndex, int endIndex,
            Vector3 n, float magnitude, float d)
        {
            var halfVertexCount = query.Vertices.Length / 2;
            var vertices = query.Vertices;
            int modified = 0;
            var destroyed = 0;
            for (int j = startIndex; j < endIndex; j += 3)
            {
                // triangle is already collapsed
                if (vertices[j] == vertices[j + 1])
                    continue;

                for (int i = j; i < j + 3; i++)
                {
                    var v = vertices[i];
                    var distance = Vector3.Distance(v, query.Epicenter);
                    if (distance < query.Radius)
                    {
                        var distanceToWall = (v.x * n.x + v.y * n.y + v.z * n.z - d) / magnitude;
                        var forceChange = query.GetForceChange(distance);
                        // NOTE whole traingle should be removed as one of the vertices is 
                        // moved more than threshold allows
                        if (Math.Abs(distanceToWall + forceChange) > query.OffsetThreshold)
                        {
                            // collapse triangle into point
                            var firstVertIndex = i - i % 3;
                            vertices[firstVertIndex + 1] = vertices[firstVertIndex];
                            vertices[firstVertIndex + 2] = vertices[firstVertIndex];

                            var backSideIndex = halfVertexCount + firstVertIndex;
                            vertices[backSideIndex + 1] = vertices[backSideIndex];
                            vertices[backSideIndex + 2] = vertices[backSideIndex];
                            destroyed += 3;
                            break;
                        }
                        vertices[i] = v + forceChange * query.ForceDirection;
                        vertices[halfVertexCount + i] = vertices[i];
                        modified++;
                    }
                }
            }
            return new MeshQuery.Result(query.Vertices)
            {
                DestroyedVertices = destroyed,
                ModifiedVertices = modified,
                ScannedTriangles = -1
            };
        }
    }
}