using System.IO;
using ActionStreetMap.Infrastructure.IO;
#if UNITY_WEBPLAYER
using UnityEngine;
#endif

namespace ActionStreetMap.Unity.IO
{
    /// <summary> Provides a way to interact with regular file system. </summary>
    public class FileSystemService: IFileSystemService
    {
        private readonly IPathResolver _pathResolver;

        /// <summary> Creates <see cref="FileSystemService"/>. </summary>
        /// <param name="pathResolver"></param>
        public FileSystemService(IPathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        /// <inheritdoc />
        public Stream ReadStream(string path)
        {
#if UNITY_WEBPLAYER
            var file = Resources.Load<TextAsset>(_pathResolver.Resolve(path));
            return new MemoryStream(file.bytes);
#else
            return File.Open(_pathResolver.Resolve(path), FileMode.Open);
#endif
        }

        public Stream WriteStream(string path)
        {
#if UNITY_WEBPLAYER
            return new MemoryStream();
#else
            return new FileStream(_pathResolver.Resolve(path), FileMode.Create);
#endif
        }

        /// <inheritdoc />
        public string ReadText(string path)
        {
#if UNITY_WEBPLAYER
            var file = Resources.Load<TextAsset>(_pathResolver.Resolve(path));
            return file.text;
#else
            using (var reader = new StreamReader(_pathResolver.Resolve(path)))
                return reader.ReadToEnd();
#endif
        }

        /// <inheritdoc />
        public byte[] ReadBytes(string path)
        {
#if UNITY_WEBPLAYER
            var file = Resources.Load<TextAsset>(_pathResolver.Resolve(path));
            return file.bytes;
#else
            return File.ReadAllBytes(_pathResolver.Resolve(path));
#endif
        }

        /// <inheritdoc />
        public bool Exists(string path)
        {
#if UNITY_WEBPLAYER
            return true;
#else
            return File.Exists(_pathResolver.Resolve(path));
#endif
        }

        /// <inheritdoc />
        public string[] GetFiles(string path, string searchPattern)
        {
#if UNITY_WEBPLAYER
            return new string[0];
#else
            return Directory.GetFiles(_pathResolver.Resolve(path), searchPattern);
#endif
        }

        /// <inheritdoc />
        public string[] GetDirectories(string path, string searchPattern)
        {
#if UNITY_WEBPLAYER
             return new string[0];
#else
            return Directory.GetDirectories(_pathResolver.Resolve(path), searchPattern);
#endif
        }
    }
}
