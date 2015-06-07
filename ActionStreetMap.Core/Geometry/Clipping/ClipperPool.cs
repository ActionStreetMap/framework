using System;
using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Core.Geometry.Clipping
{
    internal static class ClipperPool
    {
        private static LockFreeStack<TEdge> _edgeStack = new LockFreeStack<TEdge>();
        //private static ObjectListPool<TEdge> _edgeListPool = new ObjectListPool<TEdge>(128);

        public static TEdge AllocEdge()
        {
            return _edgeStack.Pop() ?? new TEdge();
        }

        public static void FreeEdge(TEdge edge)
        {
            edge.Reset();
            _edgeStack.Push(edge);
        }

        /*public static List<TEdge> AllocEdgeList(int capacity)
        {
            return _edgeListPool.New(capacity);
        }

        public static void FreeEdgeList(List<TEdge> edges)
        {
            _edgeListPool.Store(edges, false);
        }*/

    }
}
