using ActionStreetMap.Infrastructure.IO;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Path resolver which does nothing. </summary>
    public class DefaultPathResolver : IPathResolver
    {
        /// <inheritdoc />
        public string Resolve(string path)
        {
            return path;
        }
    }
}
