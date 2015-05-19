namespace ActionStreetMap.Infrastructure.IO
{
    /// <summary> Defines path resolver interface. It's used to substitute path if necessary. </summary>
    public interface IPathResolver
    {
        /// <summary> Resolves given path. </summary>
        /// <param name="path">Path.</param>
        /// <returns>Resolved path.</returns>
        string Resolve(string path);
    }
}