using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Geometry.Clipping
{
    internal static class ClipperPool
    {
        private static readonly LockFreeStack<TEdge> EdgeStack = new LockFreeStack<TEdge>();
        private static readonly LockFreeStack<Scanbeam> ScanbeamStack = new LockFreeStack<Scanbeam>();

        public static TEdge AllocEdge()
        {
            return EdgeStack.Pop() ?? new TEdge();
        }

        public static void FreeEdge(TEdge edge)
        {
            edge.Reset();
            EdgeStack.Push(edge);
        }

        public static Scanbeam AllocScanbeam()
        {
            return ScanbeamStack.Pop() ?? new Scanbeam();
        }

        public static void FreeScanbeam(Scanbeam scanbeam)
        {
            scanbeam.Next = null;
            scanbeam.Y = 0;
            ScanbeamStack.Push(scanbeam);
        }
    }
}
