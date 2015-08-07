using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Generators;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using NUnit.Framework;
using UnityEngine;

namespace ActionStreetMap.Tests.Explorer.Scene.Generators
{
    [TestFixture]
    class TreeGeneratorTests
    {
        private readonly GradientWrapper _foliageGradient = 
            GradientUtils.ParseGradient("gradient(#80c34c, #406126 50%, #101809)");

        private readonly GradientWrapper _trunkGradient =
            GradientUtils.ParseGradient("gradient(#f4a460, #614126 50%, #302013)");

        [Test]
        public void CanGenerateSimpleTree()
        {
            // ARRANGE
            var position = new Vector3(0, 0, 0);

            // ACT
            var treeGen = new TreeGenerator()
                .SetTrunkGradient(_trunkGradient)
                .SetFoliageGradient(_foliageGradient)
                .SetPosition(position);
            
            var vertCount = treeGen.CalculateVertexCount();
            var meshData = new MeshData(DummyMeshIndex.Default, vertCount);
            treeGen.Build(meshData);

            // ASSERT
            Assert.Greater(meshData.Vertices.Length, 0);
            Assert.Greater(meshData.Triangles.Length, 0);
            Assert.Greater(meshData.Colors.Length, 0);
        }
    }
}
