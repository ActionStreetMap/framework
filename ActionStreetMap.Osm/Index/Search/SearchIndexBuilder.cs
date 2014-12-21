using System;
using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Osm.Entities;
using ActionStreetMap.Osm.Index.Data;

namespace ActionStreetMap.Osm.Index.Search
{
    public class SearchIndexBuilder: IIndexBuilder
    {
        private readonly string _indexPath;
        private SearchEngine _engine;

        internal KeyValueStore _store;
        private int _count;

        public SearchIndexBuilder(string indexPath)
        {
            _indexPath = indexPath;
        }

        public void ProcessNode(Node node, int tagCount)
        {
            if (node.Tags != null)
            {
                //_engine.Index(new Document(node), false);
                foreach (var tag in node.Tags)
                {
                    if (tag.Key.StartsWith("addr:street"))
                    {
                        _store.Insert(tag);
                        _count++;

                        if (_count%10000 == 0)
                            Console.WriteLine("added {0}", _count);
                    }
                }
            }
        }

        public void ProcessWay(Way way, int tagCount)
        {
            
        }

        public void ProcessRelation(Relation relation, int tagCount)
        {
            
        }

        public void ProcessRelation(Relation relation, bool storeUnresolved)
        {
            
        }

        public void ProcessBoundingBox(BoundingBox bbox)
        {
            _store = new KeyValueStore(new MemoryStream(10000000));
        }

        public void Complete()
        {
            
        }

        public void Clear()
        {
            
        }
    }
}
