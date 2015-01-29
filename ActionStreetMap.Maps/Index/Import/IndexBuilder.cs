using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.Primitives;
using ActionStreetMap.Maps.Formats;
using ActionStreetMap.Maps.Formats.O5m;
using ActionStreetMap.Maps.Formats.Pbf;
using ActionStreetMap.Maps.Index.Helpers;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Storage;
using Node = ActionStreetMap.Maps.Entities.Node;
using Relation = ActionStreetMap.Maps.Entities.Relation;
using Way = ActionStreetMap.Maps.Entities.Way;

namespace ActionStreetMap.Maps.Index.Import
{
    internal sealed class IndexBuilder : IConfigurable, IDisposable
    {
        private SortedList<long, ScaledGeoCoordinate> _nodes = new SortedList<long, ScaledGeoCoordinate>();
        private SortedList<long, Way> _ways = new SortedList<long, Way>(10240);
        private SortedList<long, uint> _wayOffsets = new SortedList<long, uint>(10240);

        private List<MutableTuple<Relation, Envelop>> _relations = new List<MutableTuple<Relation, Envelop>>(10240);
        private HashSet<long> _skippedRelations = new HashSet<long>();

        private RTree<uint> _tree;
        private ElementStore _store;
        private IndexSettings _settings;
        private IndexStatistic _indexStatistic;

        private string _outputDirectory;

        private ITrace _trace;

        public IndexBuilder(ITrace trace)
        {
            _trace = trace;
            _indexStatistic = new IndexStatistic(_trace);
        }

        public void Build(string filePath, string outputDirectory)
        {
            var reader = GetReader(filePath);
            
            _outputDirectory = outputDirectory;

            var kvUsageMemoryStream = new MemoryStream();
            var kvUsage = new KeyValueUsage(kvUsageMemoryStream);

            var keyValueStoreFile = new FileStream(String.Format(Consts.KeyValueStorePathFormat, outputDirectory), FileMode.Create);
            var index = new KeyValueIndex(_settings.Search.KvIndexCapacity, _settings.Search.PrefixLength);
            var keyValueStore = new KeyValueStore(index, kvUsage, keyValueStoreFile);

            var storeFile = new FileStream(String.Format(Consts.ElementStorePathFormat, outputDirectory), FileMode.Create);
            _store = new ElementStore(keyValueStore, storeFile);
            _tree = new RTree<uint>(65);

            reader.Read();
            Clear();
            Complete();

            using (var kvFileStream = new FileStream(String.Format(Consts.KeyValueUsagePathFormat, outputDirectory), FileMode.Create))
            {
                var buffer = kvUsageMemoryStream.GetBuffer();
                kvFileStream.Write(buffer, 0, (int)kvUsageMemoryStream.Length);
            }

            KeyValueIndex.Save(index, new FileStream(String.Format(Consts.KeyValueIndexPathFormat, outputDirectory), FileMode.Create));
            SpatialIndex<uint>.Save(_tree, new FileStream(String.Format(Consts.SpatialIndexPathFormat, outputDirectory), FileMode.Create));
            _store.Dispose();
            _store = null;
            _tree = null;
        }

        private IReader GetReader(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            // TODO support different formats
            if (String.IsNullOrEmpty(extension) ||
                (extension.ToLower() != ".o5m" && extension.ToLower() != ".pbf"))
                throw new NotSupportedException(Strings.NotSupportedMapFormat);

            var readerContext = new ReaderContext
            {
                SourceStream = new FileStream(filePath, FileMode.Open),
                Builder = this,
                ReuseEntities = false,
                SkipTags = false,
            };
            return extension == ".o5m" ? (IReader) new O5mReader(readerContext) : 
                new PbfReader(readerContext);
        }

        public void ProcessNode(Node node, int tagCount)
        {
            // happens in pbf processing
            if (_nodes.ContainsKey(node.Id))
                return;

            _indexStatistic.IncrementTotal(ElementType.Node);
            if (node.Id < 0)
            {
                _indexStatistic.Skip(node.Id, ElementType.Node);
                return;
            }

            _nodes.Add(node.Id, new ScaledGeoCoordinate(node.Coordinate));

            if (tagCount > 0)
            {
                if (node.Tags.Any( tag => _settings.Spatial.Include.Nodes.Contains(tag.Key)))
                {
                    var offset = _store.Insert(node);
                    _tree.Insert(offset, new PointEnvelop(node.Coordinate));
                    _indexStatistic.Increment(ElementType.Node);
                }
                else
                    _indexStatistic.Skip(node.Id, ElementType.Node);
            }
        }

        public void ProcessWay(Way way, int tagCount)
        {
            _indexStatistic.IncrementTotal(ElementType.Way);
            if (way.Id < 0)
            {
                _indexStatistic.Skip(way.Id, ElementType.Way);
                return;
            }

            var envelop = new Envelop();
            way.Coordinates = new List<GeoCoordinate>(way.NodeIds.Count);
            foreach (var nodeId in way.NodeIds)
            {
                if (!_nodes.ContainsKey(nodeId))
                {
                    _indexStatistic.Skip(way.Id, ElementType.Way);
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
                _indexStatistic.Increment(ElementType.Way);
            }
            else
                // keep it as it may be used by relation
                _ways.Add(way.Id, way);
        }

        public void ProcessRelation(Relation relation, int tagCount)
        {
            _indexStatistic.IncrementTotal(ElementType.Relation);
            if (relation.Id < 0)
            {
                _indexStatistic.Skip(relation.Id, ElementType.Relation);
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
                    _indexStatistic.Skip(relation.Id, ElementType.Relation);
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

        private void FinishRelaitonProcessing()
        {
            foreach (var relationTuple in _relations)
            {
                if (_skippedRelations.Contains(relationTuple.Item1.Id))
                    continue;
                var offset = _store.Insert(relationTuple.Item1);
                _tree.Insert(offset, relationTuple.Item2);
                _indexStatistic.Increment(ElementType.Relation);
            }
        }

        public void ProcessBoundingBox(BoundingBox bbox)
        {
            // TODO save header file
            using (var writer = new StreamWriter(new FileStream(String.Format(Consts.HeaderPathFormat, _outputDirectory),
                    FileMode.Create)))
            {
                writer.Write("{0} {1}", bbox.MinPoint, bbox.MaxPoint);
            }
        }

        public void Complete()
        {
            FinishRelaitonProcessing();
            _indexStatistic.Summary();
        }

        public void Clear()
        {
            _nodes.Clear();
            _nodes = null;
            _ways.Clear();
            _ways = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        public void Configure(IConfigSection configSection)
        {
            var settingsPath = configSection.GetString("index", null);

            var jsonString = File.ReadAllText(settingsPath);
            var node = JSON.Parse(jsonString);
            _settings = new IndexSettings();
            _settings.ReadFromJson(node);
        }

        public void Dispose()
        {
            if(_store != null)
                _store.Dispose();
        }
    }
}
