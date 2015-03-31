using System;
using System.Collections.Generic;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Topology;
using UnityEngine;

namespace ActionStreetMap.Tests.Expiremental
{
    class TriangleSorter
    {
        public static void Sort()
        {
            const int columnCount = 4;
            const int rowCount = 4;

            var index = new TriangleRangeIndex(columnCount, rowCount, new MapRectangle(-15, -15, 30, 30));
            var triangles = GetTriangles(index);
         
            index.FillRanges(triangles);

            var range = index.FindRange(new MapPoint(0, 0));

            var triRange = triangles.GetRange(range.Start, range.End - range.Start + 1);

            PrintTriangles(triRange);
        }

        private static void PrintTriangles(List<Triangle> triangles)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                var v0 = triangles[i].GetVertex(0);
                var v1 = triangles[i].GetVertex(1);
                var v2 = triangles[i].GetVertex(2);
                Console.WriteLine("[({0:.00}, {1:.00}),({2:.00}, {3:.00}),({4:.00}, {5:.00})]",
                    v0.x, v0.y, v1.x, v1.y, v2.x, v2.y);
            }
        }

        #region Read input data

        private static List<Triangle> GetTriangles(TriangleRangeIndex rangeIndex)
        {
            var vertices = GetVertices();
            var indices = GetTrianglesIndices();
            var triangles = new List<Triangle>(vertices.Count / 3);
            for (int i = 0; i < vertices.Count;)
            {
                var v0 = vertices[indices[i++]];
                var v1 = vertices[indices[i++]];
                var v2 = vertices[indices[i++]];

                var centroid = new MapPoint((v0.x + v1.x + v2.x)/3f, (v0.z + v1.z + v2.z)/3f);

                triangles.Add(new Triangle()
                {
                    vertices = new Vertex[]
                    {
                        new Vertex(v0.x, v0.z), 
                        new Vertex(v1.x, v1.z), 
                        new Vertex(v2.x, v2.z), 
                    },
                    region = rangeIndex.GetIndex(centroid),
                });
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

        internal class TriangleRangeIndex
        {
            private static readonly TriangleComparer Comparer = new TriangleComparer();

            private readonly int _columnCount;
            private readonly float _xAxisStep;
            private readonly float _yAxisStep;
            private readonly float _x;
            private readonly float _y;

            private readonly Range[] _ranges;

            public TriangleRangeIndex(int columnCount, int rowCount, MapRectangle rectangle)
            {
                _columnCount = columnCount;
                _x = rectangle.BottomLeft.X;
                _y = rectangle.BottomLeft.Y;

                _xAxisStep = rectangle.Width/columnCount;
                _yAxisStep = rectangle.Height/rowCount;

                _ranges = new Range[rowCount * columnCount];
            }

            public int GetIndex(MapPoint point)
            {
                var i = (int) Math.Floor((point.X - _x) / _xAxisStep);
                var j = (int) Math.Floor((point.Y - _y) / _yAxisStep);

                return _columnCount * j + i;
            }

            public Range FindRange(MapPoint point)
            {
                var index = GetIndex(point);
                return _ranges[index];
            }

            public void FillRanges(List<Triangle> triangles)
            {
                triangles.Sort(Comparer);

                var rangeIndex = -1;
                for (int i = 0; i < triangles.Count; i++)
                {
                    var triangle = triangles[i];
                    if (triangle.region != rangeIndex)
                    {
                        if (i != 0)
                            _ranges[rangeIndex].End = i - 1;

                        rangeIndex = triangle.region;
                        _ranges[rangeIndex].Start = i;
                    }
                }
                _ranges[rangeIndex].End = triangles.Count - 1;
            }

            public struct Range
            {
                public int Start;
                public int End;
            }

            private class TriangleComparer : IComparer<Triangle>
            {
                public int Compare(Triangle x, Triangle y)
                {
                    return x.Region.CompareTo(y.Region);
                }
            }
        }
    }
}
