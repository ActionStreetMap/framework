using System;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary>
    ///     Represents default tile activator. 
    /// </summary>
    public class TileActivator: ITileActivator
    {
        private const string Category = "tile";

        /// <summary>
        ///     Trace.
        /// </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <inheritdoc />
        public void Activate(Tile tile)
        {
            Trace.Output(Category, String.Format("Activate tile: {0}", tile.MapCenter));
            Scheduler.MainThread.Schedule(() => ProcessWithChildren(tile, true));
        }

        /// <inheritdoc />
        public void Deactivate(Tile tile)
        {
            Trace.Output(Category, String.Format("Deactivate tile: {0}", tile.MapCenter));
            Scheduler.MainThread.Schedule(() => ProcessWithChildren(tile, false));
        }

        /// <inheritdoc />
        public void Destroy(Tile tile)
        {
            Trace.Output(Category, String.Format("Destroy tile: {0}", tile.MapCenter));
            tile.Registry.Dispose();
            Scheduler.MainThread.Schedule(() => DestroyWithChildren(tile));
        }

        /// <summary>
        ///     Destroys tile using UnityEngine.Object.Destroy.
        /// </summary>
        /// <param name="tile">Tile.</param>
        protected virtual void DestroyWithChildren(Tile tile)
        {
            UnityEngine.Object.Destroy(tile.GameObject.GetComponent<GameObject>());
        }

        /// <summary>
        ///     Calls UnityEngine.GameObject.SetActive(active) for given tile.
        /// </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="active">Active flag.</param>
        protected virtual void ProcessWithChildren(Tile tile, bool active)
        {
            tile.GameObject.GetComponent<GameObject>().SetActive(active);
        }
    }
}
