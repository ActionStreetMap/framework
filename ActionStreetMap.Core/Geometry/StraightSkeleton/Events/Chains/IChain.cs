using ActionStreetMap.Core.Geometry.StraightSkeleton.Circular;

namespace ActionStreetMap.Core.Geometry.StraightSkeleton.Events.Chains
{
    internal interface IChain
    {
        Edge PreviousEdge { get; }
        Edge NextEdge { get; }
        Vertex PreviousVertex { get; }
        Vertex NextVertex { get; }
        Vertex CurrentVertex { get; }
        ChainType ChainType { get; }
    }
}