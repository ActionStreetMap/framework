using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Maps.Data.Helpers;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Maps.Data.Storage;
using ActionStreetMap.Maps.Formats;
using ActionStreetMap.Maps.Formats.O5m;
using ActionStreetMap.Maps.Formats.Pbf;
using ActionStreetMap.Maps.Formats.Xml;
using Node = ActionStreetMap.Maps.Entities.Node;
using Relation = ActionStreetMap.Maps.Entities.Relation;
using Way = ActionStreetMap.Maps.Entities.Way;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Maps.Data.Import
{
    internal abstract class IndexBuilder : IDisposable
    {
        protected const string CategoryKey = "index.exec";

        private SortedList<long, ScaledGeoCoordinate> _nodes = new SortedList<long, ScaledGeoCoordinate>();
        private SortedList<long, Way> _ways = new SortedList<long, Way>(10240);
        private readonly SortedList<long, uint> _wayOffsets = new SortedList<long, uint>(10240);

        private readonly List<MutableTuple<Relation, Envelop>> _relations = new List<MutableTuple<Relation, Envelop>>(10240);
        private readonly HashSet<long> _skippedRelations = new HashSet<long>();

        internal RTree<uint> Tree { get; set; }
        internal ElementStore Store { get; set; }
        internal BoundingBox BoundingBox { get; set; }
        protected IndexSettings Settings;
        protected IndexStatistic IndexStatistic;

        protected IObjectPool ObjectPool;
        protected ITrace Trace;

        protected IndexBuilder(IndexSettings settings, IObjectPool objectPool, ITrace trace)
        {
            Settings = settings;
            ObjectPool = objectPool;
            Trace = trace;
            IndexStatistic = new IndexStatistic(Trace);
        }

        public abstract void Build();

        protected IReader GetReader(string extension)
        {
            if (String.IsNullOrEmpty(extension) ||
                (extension.ToLower() != "o5m" && extension.ToLower() != "pbf" && extension.ToLower() != "xml"))
                throw new NotSupportedException(Strings.NotSupportedMapFormat);

            switch (extension)
            {
                case "o5m": return new O5mReader();
                case "pbf": return new PbfReader();
                default: return new XmlApiReader();
            }
        }

        public void ProcessNode(Node node, int tagCount)
        {
            // happens in pbf processing
            if (_nodes.ContainsKey(node.Id))
                return;

            IndexStatistic.IncrementTotal(ElementType.Node);
            if (node.Id < 0)
            {
                IndexStatistic.Skip(node.Id, ElementType.Node);
                return;
            }

            _nodes.Add(node.Id, new ScaledGeoCoordinate(node.Coordinate));

            if (tagCount > 0)
            {
                if (node.Tags.Any(tag => Settings.Spatial.Include.Nodes.Contains(tag.Key)))
                {
                    var offset = Store.Insert(node);
                    Tree.Insert(offset, new PointEnvelop(node.Coordinate));
                    IndexStatistic.Increment(ElementType.Node);
                }
                else
                    IndexStatistic.Skip(node.Id, ElementType.Node);
            }
        }

        public void ProcessWay(Way way, int tagCount)
        {
            IndexStatistic.IncrementTotal(ElementType.Way);
            if (way.Id < 0)
            {
                IndexStatistic.Skip(way.Id, ElementType.Way);
                return;
            }

            var envelop = new Envelop();
            way.Coordinates = new List<GeoCoordinate>(way.NodeIds.Count);
            foreach (var nodeId in way.NodeIds)
            {
                if (!_nodes.ContainsKey(nodeId))
                {
                    IndexStatistic.Skip(way.Id, ElementType.Way);
                    return;
                }
                var coordinate = _nodes[nodeId];
                way.Coordinates.Add(coordinate.Unscale());
                envelop.Extend(coordinate.Latitude, coordinate.Longitude);
            }

            if (tagCount > 0)
            {
                 uint offset = Store.Insert(way);
                 Tree.Insert(offset, envelop);
                 _wayOffsets.Add(way.Id, offset);
                IndexStatistic.Increment(ElementType.Way);
            }
            else
                // keep it as it may be used by relation
                _ways.Add(way.Id, way);
        }

        public void ProcessRelation(Relation relation, int tagCount)
        {
            IndexStatistic.IncrementTotal(ElementType.Relation);
            if (relation.Id < 0)
            {
                IndexStatistic.Skip(relation.Id, ElementType.Relation);
                return;
            }

            var envelop = new Envelop();          
            // this cicle prevents us to insert ways which are part of unresolved relation
            foreach (var member in relation.Members)
            {
                var type = (ElementType)member.TypeId;

                if (type == ElementType.Node || type == ElementType.Relation || // TODO not supported yet
                    (!_wayOffsets.ContainsKey(member.MemberId) && !_ways.ContainsKey(member.MemberId)))
                {
                    // outline relations should be ignored
                    if (type == ElementType.Relation && member.Role == "outline")
                        _skippedRelations.Add(member.MemberId);

                    _skippedRelations.Add(relation.Id);
                    IndexStatistic.Skip(relation.Id, ElementType.Relation);
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
                            way = Store.Get(memberOffset) as Way;
                        }
                        else if (_ways.ContainsKey(member.MemberId))
                        {
                            way = _ways[member.MemberId];
                            memberOffset = Store.Insert(way);
                            _wayOffsets.Add(member.MemberId, memberOffset);
                        }
                        foreach (GeoCoordinate t in way.Coordinates)
                            envelop.Extend(new PointEnvelop(t));
                        break;

                    default:
                        throw new InvalidOperationException("Unknown element type!");
                }
                // TODO merge tags?
                member.Offset = memberOffset;
            }
            _relations.Add(new MutableTuple<Relation, Envelop>(relation, envelop));
       }

        public virtual void ProcessBoundingBox(BoundingBox bbox)
        {
            BoundingBox = bbox;
        }

        private void FinishRelaitonProcessing()
        {
            foreach (var relationTuple in _relations)
            {
                if (_skippedRelations.Contains(relationTuple.Item1.Id))
                    continue;
                var offset = Store.Insert(relationTuple.Item1);
                Tree.Insert(offset, relationTuple.Item2);
                IndexStatistic.Increment(ElementType.Relation);
            }
        }

        public void Complete()
        {
            FinishRelaitonProcessing();
            IndexStatistic.Summary();
        }

        public void Clear()
        {
            _nodes.Clear();
            _nodes = null;
            _ways.Clear();
            _ways = null;
            GC.Collect();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual  void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Store != null)
                    Store.Dispose();
            }
        }
    }
}
