using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Explorer.Scene.Terrain;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Scene.Indices
{
    [TestFixture]
    class TerrainMeshIndexTests
    {
        private TerrainMeshIndex _meshIndex;
        private List<TerrainMeshTriangle> _triangles;

        [SetUp]
        public void SetUp()
        {
            _triangles = new List<TerrainMeshTriangle>();
            _meshIndex = new TerrainMeshIndex(4, 4, new Rectangle2d(0, 0, 100, 100), _triangles);
            SetupTriangles();
        }

        [Test]
        public void CanBuildIndex()
        {
            // ARRANGE & ACT
            _meshIndex.Build();

            // ASSERT
            Assert.Greater(_triangles.Count, 0);
        }

        private void SetupTriangles()
        {
            _triangles = new List<TerrainMeshTriangle>();
            // TODO
        }
    }
}
