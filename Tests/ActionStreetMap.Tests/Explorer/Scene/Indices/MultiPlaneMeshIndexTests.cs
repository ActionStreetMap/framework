using System.Collections.Generic;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Indices;
using NUnit.Framework;
using UnityEngine;

namespace ActionStreetMap.Tests.Explorer.Scene.Indices
{
    [TestFixture]
    class MultiPlaneMeshIndexTests
    {
        [Test]
        public void CanBuild()
        {
            // ARRANGE
            Vector3[] vertices;
            var meshIndex = GetMeshIndex(out vertices);

            // ACT & ASSERT
            meshIndex.Build();
        }

        [Test]
        public void CanMakeQuery()
        {
            // ARRANGE
            Vector3[] vertices;
            var meshIndex = GetMeshIndex(out vertices);
            meshIndex.Build();

            // ACT
            var result = meshIndex.Modify(new MeshQuery()
            {
                Vertices = vertices,
                Epicenter = new Vector3(0, 0, 5),
                ForceDirection = new Vector3(0.5f, 0, 0.5f),
                GetForceChange = f => 1f,
                OffsetThreshold = 1,
                Radius = 6
            });

            // ASSERT
            Assert.IsNotNull(result);
            Assert.Greater(result.ModifiedVertices, 0);
            Assert.AreEqual(0, result.DestroyedVertices);
        }

        private MultiPlaneMeshIndex GetMeshIndex(out Vector3[] vertices)
        {
            var plane = new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 0, 10),
                new Vector3(0, 0, 10)
            };
            var center = new Vector3(5, 5, 5);
            vertices = new Vector3[plane.Count * 3];
            var meshIndex = new MultiPlaneMeshIndex(plane.Count, vertices.Length);
            for (int i = 0; i < plane.Count; i++)
            {
                var start = plane[i];
                var end = plane[i == plane.Count - 1 ? 0 : i + 1];
                meshIndex.AddPlane(start, end, center, i*3);
                vertices[i*3] = start;
                vertices[i*3 + 1] = end;
                vertices[i*3 + 2] = center;
            }
            return meshIndex;
        }
    }
}
