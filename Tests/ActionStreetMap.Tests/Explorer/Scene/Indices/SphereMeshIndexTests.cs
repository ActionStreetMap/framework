using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Generators;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Utils;
using NUnit.Framework;
using UnityEngine;

namespace ActionStreetMap.Tests.Explorer.Scene.Indices
{
    [TestFixture]
    class SphereMeshIndexTests
    {
        private const float Radius = 5;
        private readonly Vector3 Center = new Vector3(0, 0, 0);

        [Test]
        public void CanMakeQuery()
        {
            // ARRANGE
            var vertices = GetVertices();
            var query = new MeshQuery()
            {
                Epicenter = new Vector3(0, Radius, 0),
                ForceDirection = new Vector3(0, -1, 0),
                GetForceChange = f => 1,
                Vertices = vertices,
                Radius = Radius,
                OffsetThreshold = 1
            };

            // ACT
            var meshIndex = new SphereMeshIndex(Radius, Center);
            meshIndex.Build();
            var result = meshIndex.Modify(query);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.Greater(result.ModifiedVertices, 0);
            Assert.Greater(result.DestroyedVertices, 0);
        }

        private Vector3[] GetVertices()
        {
            var meshData = new MeshData(DummyMeshIndex.Default);
            var sphereGen = new IcoSphereGenerator(meshData)
                .SetCenter(Center)
                .SetRadius(Radius)
                .SetRecursionLevel(2)
                .SetGradient(GradientUtils.ParseGradient("gradient(#808080, #606060 50%, #505050)"));

            meshData.Initialize(sphereGen.CalculateVertexCount());
            sphereGen.Build();

            return meshData.Vertices;
        }
    }
}
