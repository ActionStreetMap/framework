using System;
using ActionStreetMap.Core.Geometry.StraightSkeleton.Primitives;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Circular
{
    public class Edge : CircularNode
    {
        public readonly Vector2d Begin;
        public readonly Vector2d End;
        
        public readonly LineLinear2d LineLinear2d;
        public readonly Vector2d Norm;

        public Ray2d BisectorNext;
        public Ray2d BisectorPrevious;

        public Edge(Vector2d begin, Vector2d end)
        {
            Begin = begin;
            End = end;

            LineLinear2d = new LineLinear2d(begin, end);
            Norm = (end - begin).Normalized(); 
        }

        public override String ToString()
        {
            return "Edge [p1=" + Begin + ", p2=" + End + "]";
        }
    }
}