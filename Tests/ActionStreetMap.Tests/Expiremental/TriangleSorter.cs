using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ActionStreetMap.Tests.Expiremental
{
    class TriangleSorter
    {
        public static void Sort()
        {
            var vertices = GetVertices();
            var triangles = GetTriangles();


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

        private static List<int> GetTriangles()
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
    }
}
