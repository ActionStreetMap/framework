using System.Collections.Generic;
using ActionStreetMap.Core;

namespace ActionStreetMap.Models.Geometry
{
    /// <summary>
    ///     Provides logic to do triangulation.
    /// </summary>
    internal class Triangulator
    {
        /// <summary>
        ///     Triangulates given polygon.
        /// </summary>
        /// <param name="points">Points which represents polygon.</param>
        /// <param name="indices">Indices.</param>
        /// <param name="reverse">Reverse points.</param>
        /// <returns>Triangles.</returns>
        public static int[] Triangulate(List<MapPoint> points, List<int> indices, bool reverse = true)
        {
            int n = points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area(points) > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2; )
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V, points))
                {
                    int s, t;
                    int a = V[u];
                    int b = V[v];
                    int c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);

                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            if(reverse) indices.Reverse();

            return indices.ToArray();
        }

        private static float Area(List<MapPoint> points)
        {
            int n = points.Count;
            float a = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                MapPoint pval = points[p];
                MapPoint qval = points[q];
                a += pval.X * qval.Y - qval.X * pval.Y;
            }
            return (a * 0.5f);
        }

        private static bool Snip(int u, int v, int w, int n, int[] V, List<MapPoint> points)
        {
            int p;
            MapPoint A = points[V[u]];
            MapPoint B = points[V[v]];
            MapPoint C = points[V[w]];
            if (float.Epsilon > (((B.X - A.X) * (C.Y - A.Y)) - ((B.Y - A.Y) * (C.X - A.X))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                MapPoint P = points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private static bool InsideTriangle(MapPoint a, MapPoint b, MapPoint c, MapPoint p)
        {
            float ax = c.X - b.X;
            float ay = c.Y - b.Y;
            float bx = a.X - c.X;
            float @by = a.Y - c.Y;
            float cx = b.X - a.X;
            float cy = b.Y - a.Y;
            float apx = p.X - a.X;
            float apy = p.Y - a.Y;
            float bpx = p.X - b.X;
            float bpy = p.Y - b.Y;
            float cpx = p.X - c.X;
            float cpy = p.Y - c.Y;

            float aCROSSbp = ax * bpy - ay * bpx;
            float cCROSSap = cx * apy - cy * apx;
            float bCROSScp = bx * cpy - @by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}