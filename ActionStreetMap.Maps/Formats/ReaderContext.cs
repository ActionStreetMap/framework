using System.IO;
using ActionStreetMap.Maps.Data.Import;

namespace ActionStreetMap.Maps.Formats
{
    internal class ReaderContext
    {
        public Stream SourceStream;

        public IndexBuilder Builder;

        public bool SkipTags;
        public bool SkipNodes;
        public bool SkipWays;
        public bool SkipRels;

        public long[] SkipArray;

        public bool ReuseEntities;
    }
}
