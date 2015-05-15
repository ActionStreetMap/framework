using System;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Tiling
{
    /// <summary>
    ///    Provides the way to register and unregister world specific objects (e.g. buildings, roads, etc.) 
    ///     in tile.
    /// </summary>
    internal class TileRegistry : IDisposable
    {
        private readonly RenderMode _renderMode;
        // so far, we store only Ids
        private readonly SafeHashSet<long> _localIds;

        // NOTE actually, this is workaround.
        // TODO should be designed better solution to prevent rendering of cross tile objects.
        /// <summary>  Contains global list of registered object ids. </summary>
        private static readonly SafeHashSet<long> GlobalIds = new SafeHashSet<long>();

        /// <summary> Creates ModelRegistry using global registered id hashset. </summary>
        internal TileRegistry(RenderMode renderMode)
        {
            _renderMode = renderMode;
            _localIds = new SafeHashSet<long>();
        }

        #region Registrations

        /// <summary> Registers model id. </summary>
        public void Register(long id)
        {
            if (_renderMode == RenderMode.Scene)
                _localIds.TryAdd(id);
        }

        /// <summary>
        ///     Registers specific model id in global storage which prevents object with the same Id to be inserted in any tile.
        /// </summary>
        /// <param name="id">Id.</param>
        public void RegisterGlobal(long id)
        {
            if (_renderMode == RenderMode.Scene)
            {
                _localIds.TryAdd(id);
                GlobalIds.TryAdd(id);
            }
        }

        #endregion

        /// <summary> Checks whether object with specific id is registered in global and local storages. </summary>
        /// <param name="id">Object id.</param>
        /// <returns>True if registration is found.</returns>
        public bool Contains(long id)
        {
            return GlobalIds.Contains(id) || _localIds.Contains(id);
        }

        #region IDisposable implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // remove all registered ids from global list if they are in current registry
                foreach (var id in _localIds)
                {
                    if (GlobalIds.Contains(id))
                        GlobalIds.TryRemove(id);
                }

                _localIds.Clear();
            }
        }

        #endregion
    }
}
