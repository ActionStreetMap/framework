using ActionStreetMap.Core;
using ActionStreetMap.Osm.Entities;

namespace ActionStreetMap.Osm.Index
{
    internal interface IIndexBuilder
    {
        void ProcessNode(Node node, int tagCount);
        void ProcessWay(Way way, int tagCount);
        void ProcessRelation(Relation relation, int tagCount);
        void ProcessRelation(Relation relation, bool storeUnresolved);
        void ProcessBoundingBox(BoundingBox bbox);
        void Complete();
        void Clear();
    }
}
