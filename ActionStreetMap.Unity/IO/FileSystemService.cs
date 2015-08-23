using System;
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
    public class FileSystemService : IFileSystemService
    {
        protected const string LogTag = "file";

        protected readonly IPathResolver PathResolver;
        protected readonly ITrace Trace;

        /// <summary> Creates <see cref="FileSystemService"/>. </summary>
        /// <param name="pathResolver">Path resolver.</param>
        /// <param name="trace">Trace.</param>
        [Dependency]
        public FileSystemService(IPathResolver pathResolver, ITrace trace)
        {
            PathResolver = pathResolver;
            Trace = trace;
        }

        /// <inheritdoc />
        public Stream ReadStream(string path)
        {
            var resolvedPath = PathResolver.Resolve(path);
            Trace.Debug(LogTag, "read stream from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return new MemoryStream(GetBytesSync(resolvedPath));
#else
            return File.Open(resolvedPath, FileMode.Open, 
                FileAccess.Read, FileShare.Read);
#endif
        }

        public Stream WriteStream(string path)
        {
            var resolvedPath = PathResolver.Resolve(path);
            Trace.Debug(LogTag, "write stream from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return new MemoryStream();
#else
            return new FileStream(resolvedPath, FileMode.Create);
#endif
        }

        /// <inheritdoc />
        public string ReadText(string path)
        {
            var resolvedPath = PathResolver.Resolve(path);
            Trace.Debug(LogTag, "read text from {0}", resolvedPath);
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
            var resolvedPath = PathResolver.Resolve(path);
            Trace.Debug(LogTag, "read bytes from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return GetBytesSync(resolvedPath);
#else
            return File.ReadAllBytes(resolvedPath);
#endif
        }

        /// <inheritdoc />
        public bool Exists(string path)
        {
            var resolvedPath = PathResolver.Resolve(path);
            Trace.Debug(LogTag, "checking {0}", resolvedPath);
#if UNITY_WEBPLAYER
            return Observable.Start(() => Resources.Load<TextAsset>(resolvedPath) != null, Scheduler.MainThread).Wait();
#else
            return File.Exists(resolvedPath);
#endif
        }

        /// <inheritdoc />
        public virtual string[] GetFiles(string path, string searchPattern)
        {
            var resolvedPath = PathResolver.Resolve(path);
            Trace.Debug(LogTag, "getting files from {0}", resolvedPath);
#if UNITY_WEBPLAYER
            throw new NotImplementedException();
#else
            return Directory.GetFiles(resolvedPath, searchPattern);
#endif
        }

        /// <inheritdoc />
        public virtual string[] GetDirectories(string path, string searchPattern)
        {
            var resolvedPath = PathResolver.Resolve(path);
            Trace.Debug(LogTag, "getting directories from {0}", resolvedPath);
#if UNITY_WEBPLAYER
             throw new NotImplementedException();
#else
            return Directory.GetDirectories(resolvedPath, searchPattern);
#endif
        }

        public virtual void CreateDirectory(string path)
        {
#if UNITY_WEBPLAYER
             throw new NotImplementedException();
#else
            var resolvedPath = PathResolver.Resolve(path);
            Trace.Debug(LogTag, "create directory: {0}", resolvedPath);
            Directory.CreateDirectory(resolvedPath);
#endif
        }

#if UNITY_WEBPLAYER
        private string GetTextSync(string resolvedPath)
        {
            // NOTE this method should NOT be called from MainThread.
             return Observable.Start(() => Resources.Load<TextAsset>(resolvedPath).text, Scheduler.MainThread)
                 .Wait(TimeSpan.FromSeconds(15));
        }

        private byte[] GetBytesSync(string resolvedPath)
        {
            // NOTE this method should NOT be called from MainThread.
            return Observable.Start(() => Resources.Load<TextAsset>(resolvedPath).bytes, Scheduler.MainThread)
                .Wait(TimeSpan.FromSeconds(15));
        }
#endif
    }
}
