using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Formats.Json;

namespace ActionStreetMap.Maps.Data.Import
{
    internal class IndexSettings
    {
        public SpatialOptions Spatial { get; private set; }
        public SearchOptions Search { get; private set; }

        public IndexSettings()
        {
            Spatial = new SpatialOptions();
            Search = new SearchOptions();
        }

        #region Reading

        public void ReadFromJson(JSONNode node)
        {
            var spatialNode = node["spatial"];
            Spatial.MaxEntries = spatialNode["rtree_max_entries"].AsInt;
            Spatial.Include = ReadTagList(spatialNode["include"]);
            Spatial.Exclude = ReadTagList(spatialNode["exclude"]);
            Spatial.RemoveTags = ReadStringList(spatialNode["remove_tags"]);

            var searchNode = node["search"];
            Search.PrefixLength = searchNode["prefix_length"].AsInt;
            Search.KvIndexCapacity = searchNode["keyvalue_index_capacity"].AsInt;
        }

        private TagList ReadTagList(JSONNode node)
        {
            return new TagList()
            {
                All = ReadStringList(node["all"]),
                Nodes = ReadStringList(node["nodes"]),
                Ways = ReadStringList(node["ways"]),
                Relations = ReadStringList(node["Relations"]),
            };
        }

        private HashSet<string> ReadStringList(JSONNode node)
        {
            var hashSet = new HashSet<string>();
            foreach (var strNode in node.Childs)
                hashSet.Add(strNode.Value);

            return hashSet;
        }

        #endregion

        #region Nested classes

        public class SpatialOptions
        {
            public int MaxEntries { get; set; }
            public HashSet<string> RemoveTags { get; set; }
            public TagList Include { get; set; }
            public TagList Exclude { get; set; }
        }

        // TODO
        public class SearchOptions
        {
            public int PrefixLength { get; set; }
            public int KvIndexCapacity { get; set; }
        }

        public class TagList
        {
            public HashSet<string> All { get; set; }
            public HashSet<string> Nodes { get; set; }
            public HashSet<string> Ways { get; set; }
            public HashSet<string> Relations { get; set; }
        }

        #endregion
    }
}
