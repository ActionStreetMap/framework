using System.Collections.Generic;
using ActionStreetMap.Core.Geometry.StraightSkeleton.Events.Chains;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Events
{
    internal class MultiSplitEvent : SkeletonEvent
    {
        public readonly List<IChain> Chains;

        public override bool IsObsolete { get { return false; } }

        public MultiSplitEvent(Vector2d point, double distance, List<IChain> chains)
            : base(point, distance)
        {
            Chains = chains;
        }
    }
}