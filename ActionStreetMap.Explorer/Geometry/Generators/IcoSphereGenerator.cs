using System.Collections.Generic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Geometry.Generators
{
    /// <summary>
    ///     Builds icosphere.
    ///     See http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
    /// </summary>
    internal class IcoSphereGenerator: AbstractGenerator
    {
        private Vector3 _center;
        private float _radius;
        private float _recursionLevel;

        public IcoSphereGenerator(MeshData meshData) : base(meshData)
        {
        }

        public IcoSphereGenerator SetCenter(Vector3 center)
        {
            _center = center;
            return this;
        }

        public IcoSphereGenerator SetRadius(float radius)
        {
            _radius = radius;
            return this;
        }

        public IcoSphereGenerator SetRecursionLevel(int recursionLevel)
        {
            _recursionLevel = recursionLevel;
            return this;
        }

        public override void Build()
        {
            var vertList = new List<Vector3>();
            var middlePointIndexCache = new Dictionary<long, int>();

            // create 12 vertices of a icosahedron
            float t = (1f + Mathf.Sqrt(5f)) / 2f;

            vertList.Add(new Vector3(-1f, t, 0f).normalized * _radius);
            vertList.Add(new Vector3(1f, t, 0f).normalized * _radius);
            vertList.Add(new Vector3(-1f, -t, 0f).normalized * _radius);
            vertList.Add(new Vector3(1f, -t, 0f).normalized * _radius);

            vertList.Add(new Vector3(0f, -1f, t).normalized * _radius);
            vertList.Add(new Vector3(0f, 1f, t).normalized * _radius);
            vertList.Add(new Vector3(0f, -1f, -t).normalized * _radius);
            vertList.Add(new Vector3(0f, 1f, -t).normalized * _radius);

            vertList.Add(new Vector3(t, 0f, -1f).normalized * _radius);
            vertList.Add(new Vector3(t, 0f, 1f).normalized * _radius);
            vertList.Add(new Vector3(-t, 0f, -1f).normalized * _radius);
            vertList.Add(new Vector3(-t, 0f, 1f).normalized * _radius);

            // create 20 triangles of the icosahedron
            var faces = new List<TriangleIndices>();

            // 5 faces around point 0
            faces.Add(new TriangleIndices(0, 11, 5));
            faces.Add(new TriangleIndices(0, 5, 1));
            faces.Add(new TriangleIndices(0, 1, 7));
            faces.Add(new TriangleIndices(0, 7, 10));
            faces.Add(new TriangleIndices(0, 10, 11));

            // 5 adjacent faces 
            faces.Add(new TriangleIndices(1, 5, 9));
            faces.Add(new TriangleIndices(5, 11, 4));
            faces.Add(new TriangleIndices(11, 10, 2));
            faces.Add(new TriangleIndices(10, 7, 6));
            faces.Add(new TriangleIndices(7, 1, 8));

            // 5 faces around point 3
            faces.Add(new TriangleIndices(3, 9, 4));
            faces.Add(new TriangleIndices(3, 4, 2));
            faces.Add(new TriangleIndices(3, 2, 6));
            faces.Add(new TriangleIndices(3, 6, 8));
            faces.Add(new TriangleIndices(3, 8, 9));

            // 5 adjacent faces 
            faces.Add(new TriangleIndices(4, 9, 5));
            faces.Add(new TriangleIndices(2, 4, 11));
            faces.Add(new TriangleIndices(6, 2, 10));
            faces.Add(new TriangleIndices(8, 6, 7));
            faces.Add(new TriangleIndices(9, 8, 1));

            // refine triangles
            for (int i = 0; i < _recursionLevel; i++)
            {
               var faces2 = new List<TriangleIndices>();
                foreach (var tri in faces)
                {
                    // replace triangle by 4 triangles
                    int a = GetMiddlePoint(tri.V1, tri.V2, ref vertList, ref middlePointIndexCache, _radius);
                    int b = GetMiddlePoint(tri.V2, tri.V3, ref vertList, ref middlePointIndexCache, _radius);
                    int c = GetMiddlePoint(tri.V3, tri.V1, ref vertList, ref middlePointIndexCache, _radius);

                    faces2.Add(new TriangleIndices(tri.V1, a, c));
                    faces2.Add(new TriangleIndices(tri.V2, b, a));
                    faces2.Add(new TriangleIndices(tri.V3, c, b));
                    faces2.Add(new TriangleIndices(a, b, c));
                }
                faces = faces2;
            }

            GenerateMeshData(faces, vertList);
        }

        /// <summary> Generates mesh data in flat shading style. </summary>
        private void GenerateMeshData(List<TriangleIndices> faces, List<Vector3> vertList)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                var face = faces[i];

                var v0 = vertList[face.V1] + _center;
                var v1 = vertList[face.V2] + _center;
                var v2 = vertList[face.V3] + _center;

                AddTriangle(v0, v1, v2);
          
            }
        }

        // return index of point in the middle of p1 and p2
        private static int GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
        {
            // first check if we have it already
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;

            int ret;
            if (cache.TryGetValue(key, out ret))
                return ret;

            // not in cache, calculate it
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = new Vector3
            (
                (point1.x + point2.x) / 2f,
                (point1.y + point2.y) / 2f,
                (point1.z + point2.z) / 2f
            );

            // add vertex makes sure point is on unit sphere
            int i = vertices.Count;
            vertices.Add(middle.normalized * radius);

            // store it, return index
            cache.Add(key, i);

            return i;
        }

        #region Nested classes

        private struct TriangleIndices
        {
            public int V1;
            public int V2;
            public int V3;

            public TriangleIndices(int v1, int v2, int v3)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
            }
        }

        #endregion
    }
}
