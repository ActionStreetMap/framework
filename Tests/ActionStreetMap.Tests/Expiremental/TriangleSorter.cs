using System;
using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Topology;
using ActionStreetMap.Explorer.Geometry;
using UnityEngine;

namespace ActionStreetMap.Tests.Expiremental
{
    class TriangleSorter
    {
        public static void Sort()
        {
            const int columnCount = 4;
            const int rowCount = 4;

            var index = new TriangleIndex(columnCount, rowCount, new MapRectangle(-15, -15, 30, 30));
            var triangles = GetTriangles(index);
         
            index.BuiltIndex(triangles);

            var indices = index.GetAfectedIndecies(new MapPoint(15, 0), 2);

            PrintTriangles(triangles, indices);
        }

        private static void PrintTriangles(List<MeshTriangle> triangles, List<int> indices)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                var index = indices[i];
                var v0 = triangles[index].Vertex0;
                var v1 = triangles[index].Vertex1;
                var v2 = triangles[index].Vertex2;
                Console.WriteLine("[({0:.00}, {1:.00}),({2:.00}, {3:.00}),({4:.00}, {5:.00})]",
                    v0.X, v0.Y, v1.X, v1.Y, v2.X, v2.Y);
            }
        }

        #region Read input data

        private static List<MeshTriangle> GetTriangles(TriangleIndex rangeIndex)
        {
            var vertices = GetVertices();
            var indices = GetTrianglesIndices();
            var triangles = new List<MeshTriangle>(vertices.Count / 3);
            for (int i = 0; i < vertices.Count;)
            {
                var v0 = vertices[indices[i++]];
                var v1 = vertices[indices[i++]];
                var v2 = vertices[indices[i++]];              

                var meshTriangle = new MeshTriangle()
                {
                    Vertex0 = new MapPoint(v0.x, v0.z),
                    Vertex1 = new MapPoint(v1.x, v1.z),
                    Vertex2 = new MapPoint(v2.x, v2.z),
                };

                rangeIndex.AddToIndex(meshTriangle);
            }
            return triangles;
        }

        private static List<Vector3> GetVertices()
        {
            var vertices = new List<Vector3>();
            using (var reader = new StreamReader(File.OpenRead(@"..\..\..\..\Tests\TestAssets\vertices.txt")))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(' ');
                    vertices.Add(new Vector3(float.Parse(line[0]), float.Parse(line[1]), float.Parse(line[2])));
                }
            }
            return vertices;
        }

        private static List<int> GetTrianglesIndices()
        {
            var triangles = new List<int>();
            using (var reader = new StreamReader(File.OpenRead(@"..\..\..\..\Tests\TestAssets\triangles.txt")))
            {
                while (!reader.EndOfStream)
                {
                    triangles.Add(int.Parse(reader.ReadLine()));
                }
            }
            return triangles;
        }
        
        #endregion
    }
}
