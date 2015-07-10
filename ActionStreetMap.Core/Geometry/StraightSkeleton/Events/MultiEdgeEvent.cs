using ActionStreetMap.Core.Geometry.StraightSkeleton.Events.Chains;
using ActionStreetMap.Core.Geometry.StraightSkeleton.Primitives;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Events
{
    internal class MultiEdgeEvent : SkeletonEvent
    {
        public readonly EdgeChain Chain;

        public override bool IsObsolete { get { return false; } }

        public MultiEdgeEvent(Vector2d point, double distance, EdgeChain chain) : base(point, distance)
        {
            Chain = chain;
        }
    }
}
