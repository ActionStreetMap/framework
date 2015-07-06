using System;
using ActionStreetMap.Core.Geometry.Triangle.Meshing.Data;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Geometry.Triangle
{
    /// <summary> Provides pool of objects specific for triangle library. </summary>
    /// <remarks> It's done as static class to keep library's public API untouched. </remarks>
    internal static class TrianglePool
    {
        #region Mesh
        
        private static readonly LockFreeStack<Mesh> MeshStack = new LockFreeStack<Mesh>();

        public static Mesh AllocMesh()
        {
            return MeshStack.Pop() ?? new Mesh();
        }

        public static void FreeMesh(Mesh mesh)
        {
            mesh.Dispose();
            MeshStack.Push(mesh);
        }

        #endregion

        #region Tris

        private static readonly LockFreeStack<Topology.Triangle> TriStack = new LockFreeStack<Topology.Triangle>();
        private static readonly LockFreeStack<BadTriangle> BadTriStack = new LockFreeStack<BadTriangle>();

        public static Topology.Triangle AllocTri()
        {
            return TriStack.Pop() ?? new Topology.Triangle();
        }

        public static void FreeTri(Topology.Triangle tri)
        {
            tri.Reset();
            TriStack.Push(tri);
        }

        public static BadTriangle AllocBadTri()
        {
            return BadTriStack.Pop() ?? new BadTriangle();
        }

        public static void FreeBadTri(BadTriangle badTri)
        {
            badTri.Reset();
            BadTriStack.Push(badTri);
        }

        #endregion

        #region Segs

        private static readonly LockFreeStack<Topology.Segment> SegStack = new LockFreeStack<Topology.Segment>();

        public static Topology.Segment AllocSeg()
        {
            return SegStack.Pop() ?? new Topology.Segment();
        }

        public static void FreeSeg(Topology.Segment seg)
        {
            seg.Reset();
            SegStack.Push(seg);
        }

        #endregion

        #region RobustPredicates

        private static readonly LockFreeStack<double[]> Double4Array = new LockFreeStack<double[]>();
        private static readonly LockFreeStack<double[]> Double5Array = new LockFreeStack<double[]>();
        private static readonly LockFreeStack<double[]> Double8Array = new LockFreeStack<double[]>();
        private static readonly LockFreeStack<double[]> Double12Array = new LockFreeStack<double[]>();
        private static readonly LockFreeStack<double[]> Double16Array = new LockFreeStack<double[]>();
        private static readonly LockFreeStack<double[]> Double32Array = new LockFreeStack<double[]>();
        private static readonly LockFreeStack<double[]> Double48Array = new LockFreeStack<double[]>();
        private static readonly LockFreeStack<double[]> Double64Array = new LockFreeStack<double[]>();
        private static readonly LockFreeStack<double[]> Double1152Array = new LockFreeStack<double[]>();

        public static double[] AllocDoubleArray(int length)
        {
            switch (length)
            {
                case 4: return Double4Array.Pop() ?? new double[4];
                case 5: return Double5Array.Pop() ?? new double[5];
                case 8: return Double8Array.Pop() ?? new double[8];
                case 12: return Double12Array.Pop() ?? new double[12];
                case 16: return Double16Array.Pop() ?? new double[16];
                case 32: return Double32Array.Pop() ?? new double[32];
                case 48: return Double48Array.Pop() ?? new double[48];
                case 64: return Double64Array.Pop() ?? new double[64];
                case 1152: return Double1152Array.Pop() ?? new double[1152];
            }
            throw new ArgumentException();
        }

        public static void FreeDoubleArray(double[] array)
        {
            int length = array.Length;
            Array.Clear(array, 0, array.Length);

            switch (length)
            {
                case 4: Double4Array.Push(array); break;
                case 5: Double5Array.Push(array); break;
                case 8: Double8Array.Push(array); break;
                case 12: Double12Array.Push(array); break;
                case 16: Double16Array.Push(array); break;
                case 32: Double32Array.Push(array); break;
                case 48: Double48Array.Push(array); break;
                case 64: Double64Array.Push(array); break;
                case 1152: Double1152Array.Push(array); break;
                default: throw new ArgumentException();
            }
        }

        #endregion
    }
}
