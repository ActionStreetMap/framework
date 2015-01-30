using System.Collections.Generic;
using System.Xml;
using ActionStreetMap.Core;
using ActionStreetMap.Maps.Entities;
using ActionStreetMap.Maps.Index.Import;

namespace ActionStreetMap.Maps.Formats.Xml
{
    /// <summary> Provides API to parse response of Overpass API backend. </summary>
    internal class XmlApiReader : IReader
    {
        private ReaderContext _context;
        private IndexBuilder _builder;

        private XmlReader _reader;
        private Element _currentElement;

        public void Read(ReaderContext context)
        {
            _context = context;
            _builder = context.Builder;

            using (_reader = XmlReader.Create(_context.SourceStream))
            {
                _reader.MoveToContent();
                while (_reader.Read())
                {
                    if (_reader.NodeType == XmlNodeType.Element)
                    {
                        if (_reader.Name == "node") ParseNode();
                        else if (_reader.Name == "tag") ParseTag();
                        else if (_reader.Name == "nd") ParseNd();
                        else if (_reader.Name == "way") ParseWay();
                        else if (_reader.Name == "relation") ParseRelation();
                        else if (_reader.Name == "member") ParseMember();
                        else if (_reader.Name == "bounds") ParseBounds();
                    }
                }
                ProcessCurrent();
            }
        }

        private void ParseBounds()
        {
            var minLat = double.Parse(_reader.GetAttribute("minlat"));
            var minLon = double.Parse(_reader.GetAttribute("minlon"));
            var maxLat = double.Parse(_reader.GetAttribute("maxlat"));
            var maxLon = double.Parse(_reader.GetAttribute("maxlon"));
            _builder.ProcessBoundingBox(new BoundingBox(new GeoCoordinate(minLat, minLon),
                new GeoCoordinate(maxLat, maxLon)));
        }

        private void ParseNode()
        {
            //id="21487162" lat="52.5271274" lon="13.3870120"
            var id = long.Parse(_reader.GetAttribute("id"));
            var lat = double.Parse(_reader.GetAttribute("lat"));
            var lon = double.Parse(_reader.GetAttribute("lon"));

            var node = new Node
            {
                Id = id,
                Coordinate = new GeoCoordinate(lat, lon)
            };
            ProcessCurrent();
            _currentElement = node;
        }

        private void ParseTag()
        {
            var key = _reader.GetAttribute("k");
            var value = _reader.GetAttribute("v");
            _currentElement.AddTag(key, value);
        }

        private void ParseWay()
        {
            var id = long.Parse(_reader.GetAttribute("id"));
            ProcessCurrent();
            // TODO use object pool
            _currentElement = new Way
            {
                Id = id,
                NodeIds = new List<long>()
            };
        }

        private void ParseNd()
        {
            var refId = long.Parse(_reader.GetAttribute("ref"));
            (_currentElement as Way).NodeIds.Add(refId);
        }

        private void ParseRelation()
        {
            var id = long.Parse(_reader.GetAttribute("id"));

            ProcessCurrent();

            _currentElement = new Relation
            {
                Id = id,
                Members = new List<RelationMember>()
            };
        }

        private void ParseMember()
        {
            var refId = long.Parse(_reader.GetAttribute("ref"));
            var type = _reader.GetAttribute("type");
            var role = _reader.GetAttribute("role");

            (_currentElement as Relation).Members.Add(new RelationMember
            {
                TypeId = (type == "way" ? 1 : (type == "node" ? 0 : 2)),
                MemberId = refId,
                Role = role,
            });
        }

        private void ProcessCurrent()
        {
            if (_currentElement == null) return;

            var tagCount = _currentElement.Tags == null ? 0 : _currentElement.Tags.Count;
            if (_currentElement is Node)
                _builder.ProcessNode(_currentElement as Node, tagCount);
            else if (_currentElement is Way)
                _builder.ProcessWay(_currentElement as Way, tagCount);
            else
                _builder.ProcessRelation(_currentElement as Relation, tagCount);
        }
    }
}