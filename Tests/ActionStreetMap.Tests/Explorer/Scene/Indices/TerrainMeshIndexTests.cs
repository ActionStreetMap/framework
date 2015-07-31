using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Terrain;
using NUnit.Framework;
using UnityEngine;

namespace ActionStreetMap.Tests.Explorer.Scene.Indices
{
    [TestFixture]
    class TerrainMeshIndexTests
    {
        private TerrainMeshIndex _meshIndex;
        private List<TerrainMeshTriangle> _triangles;
        private Vector3[] _vertices;

        #region Simple terrain cell

        private const int CellCount = 5;
        private const int GridSize = 100;
        private const int TriangleCount = CellCount * CellCount * 2;
        private const int RangeCount = CellCount * CellCount;

        private const int Step = GridSize / CellCount;

        public void SetUpSimpleCell()
        {
            SetupSimpleTestData();
            _meshIndex = new TerrainMeshIndex(CellCount, CellCount,
                new Rectangle2d(0, 0, GridSize, GridSize), _triangles);
        }

        [Test]
        public void CanBuildIndexSimple()
        {
            // ARRANGE
            SetUpSimpleCell(); 

            // ACT
            _meshIndex.Build();

            // ASSERT
            Assert.AreEqual(TriangleCount, _triangles.Count);

            var ranges = GetRanges();
            Assert.AreEqual(RangeCount, ranges.Length);
            Assert.AreEqual(RangeCount, ranges.Distinct().Count(), "All ranges are different");
            var assertMsg = "Each range should contain two triagles, e.g. 0-1, 2-3, 4-5";
            for (int i = 0; i < RangeCount; i++)
            {
                Assert.AreEqual(i*2, ranges[i].Start, assertMsg);
                Assert.AreEqual(i * 2 + 1, ranges[i].End, assertMsg);
            }
        }

        [Test]
        public void CanPerformQueryInCenterSimple()
        {
            // ARRANGE 
            SetUpSimpleCell();
            var query = GetQuery(new Vector3(50, 0, 50), new Vector3(50, 0, 50), 5);
            _meshIndex.Build();

            // ACT
            var modifiedCount = _meshIndex.Modify(query);

            // ASSERT
            Assert.AreEqual(6, modifiedCount);
        }

        private void SetupSimpleTestData()
        {
            _triangles = new List<TerrainMeshTriangle>(TriangleCount);
            _vertices = new Vector3[TriangleCount * 3];
            var count = 0;
            for (int j = 0; j < CellCount; j++)
            {
                var y = Step*j;
                for (int i = 0; i < CellCount; i++)
                {
                    var x = Step*i;
                    var startIndex = count;

                    _vertices[count++] = new Vector3(x, 0, y);
                    _vertices[count++] = new Vector3(x, 0, y + Step);
                    _vertices[count++] = new Vector3(x + Step, 0, y + Step);
                    _vertices[count++] = new Vector3(x, 0, y);
                    _vertices[count++] = new Vector3(x + Step, 0, y);
                    _vertices[count++] = new Vector3(x + Step, 0, y + Step);

                    _triangles.Add(new TerrainMeshTriangle()
                    {
                        Vertex0 = _vertices[startIndex++],
                        Vertex1 = _vertices[startIndex++],
                        Vertex2 = _vertices[startIndex++]
                    });
                    _triangles.Add(new TerrainMeshTriangle()
                    {
                        Vertex0 = _vertices[startIndex++],
                        Vertex1 = _vertices[startIndex++],
                        Vertex2 = _vertices[startIndex]
                    });
                }
            }
            Assert.AreEqual(TriangleCount, _triangles.Count);
        }

        #endregion

        #region Original terrain cell

        [Test]
        public void CanBuildIndexOriginal()
        {
            // ARRANGE
            SetUpOriginalCell();

            // ACT
            _meshIndex.Build();

            // ASSERT
            Assert.AreEqual(4738, _triangles.Count);

            var ranges = GetRanges();
            Assert.AreEqual(16 * 16, ranges.Length);
            Assert.AreEqual(16 * 16, ranges.Distinct().Count(), "All ranges are different");
        }

        [Test]
        public void CanPerformQueryInCenterOriginal()
        {
            // ARRANGE 
            SetUpOriginalCell();
            var query = GetQuery(new Vector3(50, 0, 50), new Vector3(50, 0, 50), 5);
            _meshIndex.Build();

            // ACT
            var modifiedCount = _meshIndex.Modify(query);

            // ASSERT
            Assert.Greater(modifiedCount, 0);
        }

        public void SetUpOriginalCell()
        {
            SetUpOriginalTestData();
            _meshIndex = new TerrainMeshIndex(16, 16, new Rectangle2d(0, 0, 100, 100), _triangles);
        }

        private void SetUpOriginalTestData()
        {
            var testMeshFilePath = TestHelper.TestAssetsFolder + @"\Mesh\terrain_cell_triangles.txt";
            using (var reader = new StreamReader(File.Open(testMeshFilePath, FileMode.Open)))
            {
                var count = int.Parse(reader.ReadLine());
                _triangles = new List<TerrainMeshTriangle>(count);
                _vertices = new Vector3[count * 3];
                for (int i = 0; i < count; i++)
                {
                    var triangle = new TerrainMeshTriangle()
                    {
                        Vertex0 = ReadVector3FromString(reader.ReadLine()),
                        Vertex1 = ReadVector3FromString(reader.ReadLine()),
                        Vertex2 = ReadVector3FromString(reader.ReadLine())
                    };
                    _vertices[i * 3] = triangle.Vertex0;
                    _vertices[i * 3 + 1] = triangle.Vertex1;
                    _vertices[i * 3 + 2] = triangle.Vertex2;
                    _triangles.Add(triangle);
                }
            }
        }

        private Vector3 ReadVector3FromString(string str)
        {
            var data = str.Split(' ');
            return new Vector3(
                float.Parse(data[0]), 
                float.Parse(data[1]), 
                float.Parse(data[2]));
        }

        #endregion

        private MeshQuery GetQuery(Vector3 epicenter, Vector3 collidePoint, float radius)
        {
            return new MeshQuery()
            {
                Epicenter = epicenter,
                ForceDirection = new Vector3(0, 1, 0),
                ForcePower = 1,
                Vertices = _vertices,
                CollidePoint = collidePoint,
                OffsetThreshold = 1,
                Radius = radius
            };
        }

        private TerrainMeshIndex.Range[] GetRanges()
        {
            return ReflectionUtils.GetFieldValue<TerrainMeshIndex.Range[]>(_meshIndex, "_ranges");
        }
    }
}
