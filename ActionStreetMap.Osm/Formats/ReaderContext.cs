using System.IO;
using ActionStreetMap.Osm.Index;

namespace ActionStreetMap.Osm.Formats
{
    internal class ReaderContext
    {
        public Stream SourceStream;

        public IIndexBuilder Builder;

        public bool SkipTags;
        public bool SkipNodes;
        public bool SkipWays;
        public bool SkipRels;

        public long[] SkipArray;

        public bool ReuseEntities;
    }
}
