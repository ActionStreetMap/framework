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
        private SortedList<long, Way> _ways = new SortedList<long, Way>(10240);
        private SortedList<long, uint> _wayOffsets = new SortedList<long, uint>(10240);

        private readonly RTree<uint> _tree;
        private readonly ElementStore _store;
        
        private int _processedNodesCount;
        private int _processedWaysCount;
        private int _processedRelationsCount;
        private int _skippedRelationsCount = 0;

        public IndexBuilder(RTree<uint> tree, ElementStore store)
        {
            _tree = tree;
            _store = store;
        }

        public void ProcessNode(Node node, int tagCount)
        {
            if (node.Id < 0)
                return;

            _nodes.Add(node.Id, new ScaledGeoCoordinate(node.Coordinate));

            if (tagCount > 0)
            {
                // TODO define nodes which should be added as elements
                bool found = node.Tags.Any(tag => tag.Key.StartsWith("addr:"));
                if (found)
                {
                    var offset = _store.Insert(node);
                    _tree.Insert(offset, new PointEnvelop(node.Coordinate));
                }
            }

            _processedNodesCount++;
            if (_processedNodesCount % 10000 == 0)
                Console.WriteLine("processed nodes {0}", _processedNodesCount);
        }

        public void ProcessWay(Way way, int tagCount)
        {
            if (way.Id < 0)
                return;

            var envelop = new Envelop();
            way.Coordinates = new List<GeoCoordinate>(way.NodeIds.Count);
            foreach (var nodeId in way.NodeIds)
            {
                if (!_nodes.ContainsKey(nodeId))
                {
                    Console.WriteLine("Skipped:{0}", way.Id);
                    return;
                }
                var coordinate = _nodes[nodeId];
                way.Coordinates.Add(coordinate.Unscale());
                envelop.Extend(coordinate.Latitude, coordinate.Longitude);
            }

            if (tagCount > 0)
            {
                 uint offset = _store.Insert(way);
                 _tree.Insert(offset, envelop);
                 _wayOffsets.Add(way.Id, offset);
            }
            else
                // keep it as it may be used by relation
                _ways.Add(way.Id, way);

            _processedWaysCount++;
            if (_processedWaysCount % 10000 == 0)
                Console.WriteLine("processed ways {0}", _processedWaysCount);
        }

        public void ProcessRelation(Relation relation, int tagCount)
        {
            if (relation.Id < 0)
                return;

            ProcessRelation(relation, true);
        }

        public void ProcessRelation(Relation relation, bool storeUnresolved)
        {
            var envelop = new Envelop();
            _processedRelationsCount++;

            // this cicle prevents us to insert ways which are part of unresolved relation
            foreach (var member in relation.Members)
            {
                if (!_wayOffsets.ContainsKey(member.MemberId) && !_ways.ContainsKey(member.MemberId))
                {
                    Console.WriteLine("Relation {0} has unresolved way: {1} and will be skipped.", relation.Id, member.MemberId);
                    _skippedRelationsCount++;
                    return;
                }
            }

            foreach (var member in relation.Members)
            {
                var type = (ElementType) member.TypeId;
                uint memberOffset = 0;
                switch (type)
                {
                    case ElementType.Node:
                        // TODO not supported yet
                        _skippedRelationsCount++;
                        return;
                    case ElementType.Way:
                        Way way = null;
                        if (_wayOffsets.ContainsKey(member.MemberId))
                        {
                            memberOffset = _wayOffsets[member.MemberId];
                            way = _store.Get(memberOffset) as Way;
                        }
                        else if (_ways.ContainsKey(member.MemberId))
                        {
                            way = _ways[member.MemberId];
                            memberOffset = _store.Insert(way);
                            _wayOffsets.Add(member.MemberId, memberOffset);
                        }

                        for (int i = 0; i < way.Coordinates.Count; i++)
                            envelop.Extend(new PointEnvelop(way.Coordinates[i]));
                        break;
                    case ElementType.Relation:
                        // TODO not supported yet
                        _skippedRelationsCount++;
                        return;
                    default:
                        throw new InvalidOperationException("Unknown element type!");
                }
                // TODO merge tags?
                member.Offset = memberOffset;
            }

            var offset = _store.Insert(relation);
            _tree.Insert(offset, envelop);

            if (_processedRelationsCount % 1000 == 0)
                Console.WriteLine("processed relations {0}", _processedRelationsCount);
       }

        public void ProcessBoundingBox(BoundingBox bbox)
        {
        }

        public void Complete()
        {
            Console.WriteLine("Total relations: {0} including skipped:{1}", _processedRelationsCount, _skippedRelationsCount);
        }

        public void Clear()
        {
            //Tree = null;
            _nodes.Clear();
            _nodes = null;
            _ways.Clear();
            _ways = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
        }
    }
}
