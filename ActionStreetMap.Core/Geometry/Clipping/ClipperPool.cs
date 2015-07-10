using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Geometry.Clipping
{
    /// <summary> Provides pool of objects specific for clipper library. </summary>
    /// <remarks> It's done as static class to keep library's public API untouched. </remarks>
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
