using System.IO;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;

#if UNITY_WEBPLAYER
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;
#endif

namespace ActionStreetMap.Unity.IO
{
    /// <summary> Provides a way to interact with regular file system. </summary>
    public class FileSystemService: IFileSystemService
    {
        private const string LogTag = "file";

        private readonly IPathResolver _pathResolver;
        private readonly ITrace _trace;

        /// <summary> Creates <see cref="FileSystemService"/>. </summary>
        /// <param name="pathResolver">Path resolver.</param>
        /// <param name="trace">Trace.</param>
        public FileSystemService(IPathResolver pathResolver, ITrace trace)
        {
            _pathResolver = pathResolver;
            _trace = trace;
            _trace = trace;
        }

        /// <inheritdoc />
        public Stream ReadStream(string path)
        {
            var resolvedPath = _pathResolver.Resolve(path);
            _trace.Debug(LogTag, "read stream from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return new MemoryStream(GetBytesSync(resolvedPath));
#else
            return File.Open(resolvedPath, FileMode.Open);
#endif
        }

        public Stream WriteStream(string path)
        {
            var resolvedPath = _pathResolver.Resolve(path);
            _trace.Debug(LogTag, "write stream from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return new MemoryStream();
#else
            return new FileStream(resolvedPath, FileMode.Create);
#endif
        }

        /// <inheritdoc />
        public string ReadText(string path)
        {
            var resolvedPath = _pathResolver.Resolve(path);
            _trace.Debug(LogTag, "read text from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return GetTextSync(resolvedPath);
#else
            using (var reader = new StreamReader(resolvedPath))
                return reader.ReadToEnd();
#endif
        }

        /// <inheritdoc />
        public byte[] ReadBytes(string path)
        {
            var resolvedPath = _pathResolver.Resolve(path);
            _trace.Debug(LogTag, "read bytes from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return GetBytesSync(resolvedPath);
#else
            return File.ReadAllBytes(resolvedPath);
#endif
        }

        /// <inheritdoc />
        public bool Exists(string path)
        {
            var resolvedPath = _pathResolver.Resolve(path);
            _trace.Debug(LogTag, "checking {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return Observable.Start(() => Resources.Load<TextAsset>(resolvedPath) != null, Scheduler.MainThread).Wait();
#else
            return File.Exists(resolvedPath);
#endif
        }

        /// <inheritdoc />
        public string[] GetFiles(string path, string searchPattern)
        {
            var resolvedPath = _pathResolver.Resolve(path);
            _trace.Debug(LogTag, "getting files from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return new string[0];
#else
            return Directory.GetFiles(resolvedPath, searchPattern);
#endif
        }

        /// <inheritdoc />
        public string[] GetDirectories(string path, string searchPattern)
        {
            var resolvedPath = _pathResolver.Resolve(path);
            _trace.Debug(LogTag, "getting directories from {0}", resolvedPath);
#if UNITY_WEBPLAYER
             return new string[0];
#else
            return Directory.GetDirectories(resolvedPath, searchPattern);
#endif
        }
#if UNITY_WEBPLAYER
        private string GetTextSync(string resolvedPath)
        {
            // NOTE this method should NOT be called from MainThread.
             return Observable.Start(() => Resources.Load<TextAsset>(resolvedPath).text, Scheduler.MainThread).Wait();
        }

        private byte[] GetBytesSync(string resolvedPath)
        {
            // NOTE this method should NOT be called from MainThread.
            return Observable.Start(() => Resources.Load<TextAsset>(resolvedPath).bytes, Scheduler.MainThread).Wait();
        }
#endif
    }
}
