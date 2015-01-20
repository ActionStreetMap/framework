using System;
using ActionStreetMap.Core.Scene.World.Buildings;
using ActionStreetMap.Core.Scene.World.Infos;
using ActionStreetMap.Core.Scene.World.Roads;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Core.Scene
{
    /// <summary>
    ///    Provides the way to register and unregister world specific objects (e.g. buildings, roads, etc.) in tile.
    /// </summary>
    public class TileRegistry : IDisposable
    {
        // so far, we store only Ids
        private readonly SafeHashSet<long> _localIds;

        // NOTE actually, this is workaround.
        // TODO should be designed better solution to prevent rendering of cross tile objects.
        /// <summary>
        ///     Contains global list of registered object ids
        /// </summary>
        private static readonly SafeHashSet<long> GlobalIds = new SafeHashSet<long>();

        /// <summary>
        ///     Creates ModelRegistry using global registered id hashset.
        /// </summary>
        internal TileRegistry()
        {
            _localIds = new SafeHashSet<long>();
        }

        #region Registrations

        /// <summary>
        ///     Registers building ьщвуд.
        /// </summary>
        /// <param name="building">Building.</param>
        public void Register(Building building)
        {
            _localIds.TryAdd(building.Id);
        }

        /// <summary>
        ///    Registres road. 
        /// </summary>
        /// <param name="road">Road.</param>
        public void Register(Road road)
        {
            road.Elements.ForEach(e => _localIds.TryAdd(e.Id));
        }

        /// <summary>
        ///     Registers info.
        /// </summary>
        /// <param name="info">Info.</param>
        public void Register(Info info)
        {
            _localIds.TryAdd(info.Id);
        }

        /// <summary>
        ///     Registers specific model id in global storage which prevents object with the same Id to be inserted in any tile.
        /// </summary>
        /// <param name="id">Id.</param>
        public void RegisterGlobal(long id)
        {
            _localIds.TryAdd(id);
            GlobalIds.TryAdd(id);
        }

        #endregion

        /// <summary>
        ///     Checks whether object with specific id is registered in global and local storages.
        /// </summary>
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
