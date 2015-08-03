using System;
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
            var vertices = query.Vertices;
            int modified = 0;
            var destroyed = 0;
            for (int j = 0; j < vertices.Length; j += 3)
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
                        var forceChange = query.GetForceChange(distance);
                        var distanceToCenter = Vector3.Distance(v, _center);
                        if (Math.Abs(_radius - distanceToCenter + forceChange) > query.OffsetThreshold)
                        {
                            // collapse triangle into point
                            var firstVertIndex = i - i % 3;
                            vertices[firstVertIndex + 1] = vertices[firstVertIndex];
                            vertices[firstVertIndex + 2] = vertices[firstVertIndex];
                            destroyed += 3;
                            break;
                        }
                        var forceDirection = (_center - v).normalized;
                        vertices[i] = v + forceChange * forceDirection;
                        modified++;
                    }
                }
            }

            return new MeshQuery.Result(vertices)
            {
                DestroyedVertices = destroyed,
                ModifiedVertices = modified
            };
        }
    }
}
