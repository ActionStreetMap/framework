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


        /*private static object _lockObj = new object();
        private static Stack<Topology.Triangle> _trisStack = new Stack<Topology.Triangle>(10240);
        private static Stack<Topology.Segment> _segsStack = new Stack<Topology.Segment>(10240);

        public static Topology.Triangle AllocTri()
        {
            lock (_lockObj)
            {
                if (_trisStack.Count > 0)
                    return _trisStack.Pop();
                return new Topology.Triangle();
            }
        }

        public static void FreeTri(Topology.Triangle tri)
        {
            lock (_lockObj)
            {
                if(_trisStack.Count == 10240)
                    return;

                tri.Cleanup();
                _trisStack.Push(tri);
            }
        }

        public static Topology.Segment AllocSeg()
        {
            lock (_lockObj)
            {
                if (_segsStack.Count > 0)
                    return _segsStack.Pop();
                return new Topology.Segment();
            }
        }

        public static void FreeSeg(Topology.Segment seg)
        {
            lock (_lockObj)
            {
                if (_segsStack.Count == 10240)
                    return;

                seg.Cleanup();
                _segsStack.Push(seg);
            }
        }*/
    }
}
