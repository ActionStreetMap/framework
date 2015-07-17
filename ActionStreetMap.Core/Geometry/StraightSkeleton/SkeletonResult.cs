using System.Collections.Generic;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton
{
    /// <summary> Represents skeleton algorithm results. </summary>
    public class SkeletonResult
    {
        /// <summary> Result of skeleton algorithm for edge. </summary>
        public readonly List<EdgeResult> Edges;

        /// <summary> Distance points from edges. </summary>
        public readonly Dictionary<Vector2d, double> Distances;

        /// <summary> Creates instance of <see cref="SkeletonResult"/>. </summary>
        public SkeletonResult(List<EdgeResult> edges, Dictionary<Vector2d, double> distances)
        {
            Edges = edges;
            Distances = distances;
        }
    }
}