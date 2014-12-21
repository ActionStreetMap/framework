using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Osm.Entities;
using ActionStreetMap.Osm.Index.Data;
using ActionStreetMap.Osm.Index.Spatial;

namespace ActionStreetMap.Osm.Index
{
    internal class IndexBuilder: IIndexBuilder
    {
        private SortedList<long, ScaledGeoCoordinate> _nodes = new SortedList<long, ScaledGeoCoordinate>();
        private SortedList<long, Envelop> _ways = new SortedList<long, Envelop>(10240);
        //private SortedList<long, uint> _wayOffsets = new SortedList<long, uint>(10240);

        private List<Relation> _unresolvedRelations = new List<Relation>(4096);

        private readonly RTree<uint> _tree;
        private readonly ElementStore _elementStore;
        
        private int _processedCount;

        public IndexBuilder(RTree<uint> tree, ElementStore elementStore)
        {
            _tree = tree;
            _elementStore = elementStore;
        }

        public void ProcessNode(Node node, int tagCount)
        {
            if (node.Id < 0)
                return;
            _nodes.Add(node.Id, new ScaledGeoCoordinate(node.Coordinate));

            if (tagCount > 0)
            {
                // TODO define nodes which should be added as elements
                bool found = node.Tags.Any(tag => tag.Key.StartsWith("addr:street"));
                if (found)
                {
                    var offset = _elementStore.Insert(node);
                    _tree.Insert(offset, new PointEnvelop(node.Coordinate));
                }
            }

            _processedCount++;
            if (_processedCount % 10000 == 0)
                Console.WriteLine("processed {0}", _processedCount);
        }

        public void ProcessWay(Way way, int tagCount)
        {
            return;

            if (way.Id < 0)
                return;

            var envelop = new Envelop();
            foreach (var nodeId in way.NodeIds)
            {
                if (!_nodes.ContainsKey(nodeId))
                {
                    Console.WriteLine("Skipped:{0}", way.Id);
                    return;
                }
                var coordinate = _nodes[nodeId];
                envelop.Extend(coordinate.Latitude, coordinate.Longitude);
            }

            uint offset = 0;
            _tree.Insert(offset, envelop);
        }

        public void ProcessRelation(Relation relation, int tagCount)
        {
            if (relation.Id < 0)
                return;

            ProcessRelation(relation, true);
        }

        public void ProcessRelation(Relation relation, bool storeUnresolved)
        {
            /* var envelop = new Envelope();
            bool isValid = false;
            foreach (var member in relation.Members)
            {
                if (member.TypeId == 0)
                {
                    if (!_nodes.ContainsKey(member.MemberId))
                    {
                        _trace.Output(String.Format("Relation {0} has unresolved node: {1} and will be skipped.", 
                            relation.Id, member.MemberId));
                        return;
                    }
                    var coordinate = _nodes[member.MemberId];
                    envelop.Extend(coordinate.Longitude, coordinate.Latitude);
                    isValid = true;
                } 
                else if (member.TypeId == 1)
                {
                    // TODO investigate why we cannot find way here
                    if (!_ways.ContainsKey(member.MemberId))
                    {
                        _trace.Output(String.Format("Relation {0} has unresolved way: {1} and will be skipped.", relation.Id, member.MemberId));
                        return;
                    }
                    envelop.Extend(_ways[member.MemberId]);
                    isValid = true;
                }
                else if (member.TypeId == 2)
                {
                    // TODO use try or indexof to avoid double search
                    if (!_relations.ContainsKey(member.MemberId))
                    {
                        if (storeUnresolved)
                            _unresolvedRelations.Insert(relation);
                        else
                            _trace.Output(String.Format("Relation {0} has unresolved relation: {1}!", relation.Id, member.MemberId));
                        return;
                    }
                    envelop.Extend(_relations[member.MemberId]);
                    isValid = true;
                    // TODO process this case!
                    // NOTE relation can be unprocessed yet
                    //envelop.Extend(_relations[member.MemberId]);
                }
            }
            if (isValid)
            {
                Tree.Insert(new RTreeData()
                {
                    Type = 2,
                    Id = relation.Id,
                    RelRefs = relation.Members.Select(m => new Tuple<byte, long>((byte)m.TypeId, m.MemberId)).ToList()
                }, envelop);
            }
            else
            {
                // TODO possibly consists of points
                _trace.Output(String.Format("Relation {0} has invalid envelop!", relation.Id));
            }*/
        }

        public void ProcessBoundingBox(BoundingBox bbox)
        {
        }

        public void Complete()
        {
            /*foreach (var relation in _unresolvedRelations)
            {
                ProcessRelation(relation, false);
            }*/
        }

        public void Clear()
        {
            //Tree = null;
            _nodes.Clear();
            _nodes = null;
            //_relations.Clear();
            //_relations = null;
            _unresolvedRelations.Clear();
            _unresolvedRelations = null;
            _ways.Clear();
            _ways = null;

            GC.Collect();
            GC.WaitForFullGCComplete();
        }
    }
}
