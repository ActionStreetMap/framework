using System;
using ActionStreetMap.Core.Geometry.StraightSkeleton.Circular;
using ActionStreetMap.Core.Geometry.StraightSkeleton.Primitives;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Events
{
    internal class EdgeEvent : SkeletonEvent
    {
        public readonly Vertex NextVertex;
        public readonly Vertex PreviousVertex;

        public override bool IsObsolete
        {
            get { return PreviousVertex.IsProcessed || NextVertex.IsProcessed; }
        }

        public EdgeEvent(Vector2d point, double distance, Vertex previousVertex, Vertex nextVertex) :
            base(point, distance)
        {
            PreviousVertex = previousVertex;
            NextVertex = nextVertex;
        }

        public override String ToString()
        {
            return "EdgeEvent [V=" + V + ", PreviousVertex="
                   + (PreviousVertex != null ? PreviousVertex.Point.ToString() : "null") +
                   ", NextVertex="
                   + (NextVertex != null ? NextVertex.Point.ToString() : "null") + ", Distance=" +
                   Distance + "]";
        }
    }
}