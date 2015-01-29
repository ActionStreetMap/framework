using System.IO;

namespace ActionStreetMap.Infrastructure.IO
{
    /// <summary>
    ///     Defines a way of interacting with abstract file system. 
    /// </summary>
    public interface IFileSystemService
    {
        /// <summary>
        ///     Reads stream using path.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <returns>Stream.</returns>
        Stream ReadStream(string path);

        /// <summary>
        ///    Gets write stream for given path.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <returns>Stream.</returns>
        Stream WriteStream(string path);

        /// <summary>
        ///     Reads text using path.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <returns>Text string.</returns>
        string ReadText(string path);

        /// <summary>
        ///     Reads bytes using path.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <returns>Byte array.</returns>
        byte[] ReadBytes(string path);

        /// <summary>
        ///     Checks whether given path exists.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <returns>True if path exists.</returns>
        bool Exists(string path);

        /// <summary>
        ///     Gets list of file names using given path and search pattern.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>File names.</returns>
        string[] GetFiles(string path, string searchPattern);

        /// <summary>
        ///     Gets list of directories names using given path and search pattern.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <returns>Directory names.</returns>
        string[] GetDirectories(string path, string searchPattern);
    }
}
