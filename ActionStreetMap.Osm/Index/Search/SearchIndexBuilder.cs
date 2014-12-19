using System.IO;
using ActionStreetMap.Core;
using ActionStreetMap.Osm.Entities;

namespace ActionStreetMap.Osm.Index.Search
{
    public class SearchIndexBuilder: IIndexBuilder
    {
        private readonly string _indexPath;
        private SearchEngine _engine;

        public SearchIndexBuilder(string indexPath)
        {
            _indexPath = indexPath;
        }

        public void ProcessNode(Node node, int tagCount)
        {
            if (node.Tags != null)
            {
                _engine.Index(new Document(node), false);
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
            var indexPath = Path.GetFullPath(_indexPath);
            Directory.Delete(indexPath, true);
            _engine = new SearchEngine(indexPath, "index", true);
        }

        public void Complete()
        {
            _engine.Save();
        }

        public void Clear()
        {
            
        }
    }
}
