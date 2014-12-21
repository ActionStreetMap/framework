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

        private KeyValueIndex _index;
        internal ElementStore _store;
        private int _addedCount;
        private int _processedCount;

        public SearchIndexBuilder(string indexPath)
        {
            _indexPath = indexPath;
        }

        public void ProcessNode(Node node, int tagCount)
        {
            if (tagCount > 0)
            {
                bool found = false;
                foreach (var tag in node.Tags)
                {
                    if (tag.Key.StartsWith("addr:street"))
                    {
                        found = true;
                        break;
                    }
                }
                /*//_engine.Index(new Document(node), false);
                foreach (var tag in node.Tags)
                {
                    if (tag.Key.StartsWith("addr:street"))
                    {
                        _store.Insert(tag);
                        _addedCount++;

                        if (_addedCount%10000 == 0)
                            Console.WriteLine("added {0}", _addedCount);
                    }
                }*/
                if (found)
                {
                    _store.Insert(node);
                    _addedCount++;
                    if (_addedCount%10000 == 0)
                        Console.WriteLine("added {0}", _addedCount);
                }
                _processedCount++;
                if (_processedCount % 10000 == 0)
                    Console.WriteLine("processed {0}", _processedCount);

            }
        }

        public void ProcessWay(Way way, int tagCount)
        {
            //if (tagCount > 0)
            //    _store.Insert(way);
        }

        public void ProcessRelation(Relation relation, int tagCount)
        {
            
        }

        public void ProcessRelation(Relation relation, bool storeUnresolved)
        {
            
        }

        public void ProcessBoundingBox(BoundingBox bbox)
        {
            var keyValueStoreFile = new FileStream(@"Index\keyValueStore.bytes", FileMode.Create);
            _index = new KeyValueIndex(300000, 4);
            var keyValueStore = new KeyValueStore(_index, keyValueStoreFile);

            var storeFile = new FileStream(@"Index\elementStore.bytes", FileMode.Create);
            _store = new ElementStore(keyValueStore, storeFile);
        }

        public void Complete()
        {
            KeyValueIndex.Save(_index, new FileStream(@"Index\keyValueIndex.bytes", FileMode.Create));
            _store.Dispose();
        }

        public void Clear()
        {
            
        }
    }
}
