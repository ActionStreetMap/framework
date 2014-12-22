using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Osm.Entities;
using ActionStreetMap.Osm.Helpers;
using ActionStreetMap.Osm.Index.Helpers;
using ActionStreetMap.Osm.Index.Spatial;
using ActionStreetMap.Osm.Index.Storage;

namespace ActionStreetMap.Osm.Index
{
    internal class IndexBuilder
    {
        private SortedList<long, ScaledGeoCoordinate> _nodes = new SortedList<long, ScaledGeoCoordinate>();
        private SortedList<long, Way> _ways = new SortedList<long, Way>(10240);
        private SortedList<long, uint> _wayOffsets = new SortedList<long, uint>(10240);

        private readonly RTree<uint> _tree;
        private readonly ElementStore _store;
        private readonly Statistics _statistics;

        public IndexBuilder(RTree<uint> tree, ElementStore store, ITrace trace)
        {
            _tree = tree;
            _store = store;
            _statistics = new Statistics(trace);
        }

        public void ProcessNode(Node node, int tagCount)
        {
            _statistics.IncrementTotal(ElementType.Node);
            if (node.Id < 0)
            {
                _statistics.Skip(node.Id, ElementType.Node);
                return;
            }

            _nodes.Add(node.Id, new ScaledGeoCoordinate(node.Coordinate));

            if (tagCount > 0)
            {
                // TODO define nodes which should be added as elements
                bool found = node.Tags.Any(tag => tag.Key.StartsWith("addr:"));
                if (found)
                {
                    var offset = _store.Insert(node);
                    _tree.Insert(offset, new PointEnvelop(node.Coordinate));
                    _statistics.Increment(ElementType.Node);
                }
                else
                    _statistics.Skip(node.Id, ElementType.Node);
            }
        }

        public void ProcessWay(Way way, int tagCount)
        {
            _statistics.IncrementTotal(ElementType.Way);
            if (way.Id < 0)
            {
                _statistics.Skip(way.Id, ElementType.Way);
                return;
            }

            var envelop = new Envelop();
            way.Coordinates = new List<GeoCoordinate>(way.NodeIds.Count);
            foreach (var nodeId in way.NodeIds)
            {
                if (!_nodes.ContainsKey(nodeId))
                {
                    _statistics.Skip(way.Id, ElementType.Way);
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
                _statistics.Increment(ElementType.Way);
            }
            else
                // keep it as it may be used by relation
                _ways.Add(way.Id, way);
        }

        public void ProcessRelation(Relation relation, int tagCount)
        {
            _statistics.IncrementTotal(ElementType.Relation);
            if (relation.Id < 0)
            {
                _statistics.Skip(relation.Id, ElementType.Relation);
                return;
            }

            var envelop = new Envelop();
           

            // this cicle prevents us to insert ways which are part of unresolved relation
            foreach (var member in relation.Members)
            {
                var type = (ElementType)member.TypeId;

                if (type == ElementType.Node || type == ElementType.Relation ||  // TODO not supported yet
                    (!_wayOffsets.ContainsKey(member.MemberId) && !_ways.ContainsKey(member.MemberId)))
                {
                    _statistics.Skip(relation.Id, ElementType.Relation);
                    return;
                }
            }

            foreach (var member in relation.Members)
            {
                var type = (ElementType) member.TypeId;
                uint memberOffset = 0;
                switch (type)
                {
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

                    default:
                        throw new InvalidOperationException("Unknown element type!");
                }
                // TODO merge tags?
                member.Offset = memberOffset;
            }

            var offset = _store.Insert(relation);
            _tree.Insert(offset, envelop);
            _statistics.Increment(ElementType.Relation);
       }

        public void ProcessBoundingBox(BoundingBox bbox)
        {
        }

        public void Complete()
        {
            _statistics.Summary();
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

        #region Nested

        internal class Statistics
        {
            private readonly ITrace _trace;
            private int _processedNodesCount;
            private int _processedWaysCount;
            private int _processedRelationsCount;

            private int _addedNodesCount;
            private int _addedWaysCount;
            private int _addedRelationsCount;

            private int _skippedNodesCount;
            private int _skippedWaysCount;
            private int _skippedRelationsCount;

            public Statistics(ITrace trace)
            {
                _trace = trace;
            }

            public void Increment(ElementType type)
            {
                switch (type)
                {
                    case ElementType.Node:
                        _addedNodesCount++;
                        break;
                    case ElementType.Way:
                        _addedWaysCount++;
                        break;
                    case ElementType.Relation:
                        _addedRelationsCount++;
                        break;
                }
            }

            public void IncrementTotal(ElementType type)
            {
                switch (type)
                {
                    case ElementType.Node:
                        PrintProgress(++_processedNodesCount, "node");
                        break;
                    case ElementType.Way:
                        PrintProgress(++_processedWaysCount, "way");
                        break;
                    case ElementType.Relation:
                        PrintProgress(++_processedRelationsCount, "relation");
                        break;
                }
            }

            private void PrintProgress(int value, string typeName)
            {
                if (value % 10000 == 0)
                    _trace.Output(String.Format("processed {0}: {1}", typeName, value));
            }

            public void Skip(long id, ElementType type)
            {
                switch (type)
                {
                    case ElementType.Node:
                        _skippedNodesCount++;
                        break;
                    case ElementType.Way:
                        _skippedWaysCount++;
                        break;
                    case ElementType.Relation:
                        _skippedRelationsCount++;
                        break;
                }
            }

            public void Summary()
            {
                PrintSummary("PROCESSED", _processedNodesCount, _processedWaysCount, _processedRelationsCount);
                PrintSummary("ADDED", _addedNodesCount, _addedWaysCount, _addedRelationsCount);
                PrintSummary("SKIPPED", _skippedNodesCount, _skippedWaysCount, _skippedRelationsCount);
            }

            private void PrintSummary(string totalText, int nodes, int ways, int relations)
            {
                _trace.Output(String.Format("Total {0} elements: {1}", totalText, nodes + ways + relations));
                _trace.Output(String.Format("\tnodes: {0}", nodes));
                _trace.Output(String.Format("\tways: {0}", ways));
                _trace.Output(String.Format("\trelations: {0}", relations));
                _trace.Output(String.Format(""));
            }
        }

        #endregion
    }
}
