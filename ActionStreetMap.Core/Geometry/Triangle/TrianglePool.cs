using ActionStreetMap.Core.Geometry.Triangle.Meshing.Data;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Geometry.Triangle
{
    /// <summary> Provides pool of objects specific for triangle library. </summary>
    internal static class TrianglePool
    {
        private static LockFreeStack<Mesh> _meshStack = new LockFreeStack<Mesh>();
        private static LockFreeStack<Topology.Triangle> _triStack = new LockFreeStack<Topology.Triangle>();
        private static LockFreeStack<BadTriangle> _badTriStack = new LockFreeStack<BadTriangle>();
        private static LockFreeStack<Topology.Segment> _segStack = new LockFreeStack<Topology.Segment>();

        #region Mesh

        public static Mesh AllocMesh()
        {
            return _meshStack.Pop() ?? new Mesh();
        }

        public static void FreeMesh(Mesh mesh)
        {
            mesh.Dispose();
            _meshStack.Push(mesh);
        }

        #endregion

        #region Tris

        public static Topology.Triangle AllocTri()
        {
            return _triStack.Pop() ?? new Topology.Triangle();
        }

        public static void FreeTri(Topology.Triangle tri)
        {
            tri.Reset();
            _triStack.Push(tri);
        }

        public static BadTriangle AllocBadTri()
        {
            return _badTriStack.Pop() ?? new BadTriangle();
        }

        public static void FreeBadTri(BadTriangle badTri)
        {
            badTri.Reset();
            _badTriStack.Push(badTri);
        }

        #endregion

        #region Segs

        public static Topology.Segment AllocSeg()
        {
            return _segStack.Pop() ?? new Topology.Segment();
        }

        public static void FreeSeg(Topology.Segment seg)
        {
            seg.Reset();
            _segStack.Push(seg);
        }

        #endregion

    }
}
