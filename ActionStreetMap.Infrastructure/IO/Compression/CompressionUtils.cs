using System.Collections.Generic;
using System.IO;

namespace ActionStreetMap.Infrastructure.IO.Compression
{
    /// <summary> Provides some additional compression methods. </summary>
    public static class CompressionUtils
    {
        /// <summary> Decompress given bytes into outputStream. </summary>
        /// <param name="compressed">Compressed bytes.</param>
        public static IEnumerable<KeyValuePair<string, byte[]>> Unzip(byte[] compressed)
        {
            var zipStream = new InflateZipStream();
            return zipStream.Unzip(new MemoryStream(compressed));
        }
    }
}
