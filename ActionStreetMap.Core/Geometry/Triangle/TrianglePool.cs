using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Geometry.Triangle
{
    /// <summary> Provides pool of objects specific for triangle library. </summary>
    internal static class TrianglePool
    {
        private static LockFreeStack<Topology.Triangle> _triStack = new LockFreeStack<Topology.Triangle>();
        private static LockFreeStack<Topology.Segment> _segStack = new LockFreeStack<Topology.Segment>();

        public static Topology.Triangle AllocTri()
        {
            return _triStack.Pop() ?? new Topology.Triangle();
        }

        public static void FreeTri(Topology.Triangle tri)
        {
            tri.Cleanup();
            _triStack.Push(tri);
        }
         
        public static Topology.Segment AllocSeg()
        {
            return _segStack.Pop() ?? new Topology.Segment();
        }

        public static void FreeSeg(Topology.Segment seg)
        {
            seg.Cleanup();
            _segStack.Push(seg);
        } 
         
         
         

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
