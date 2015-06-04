using System;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Tiling
{
    /// <summary> Represents default tile activator. </summary>
    public class TileActivator: ITileActivator
    {
        private const string LogTag = "tile";

        /// <summary> Trace. </summary>
        [Dependency]
        public ITrace Trace { get; set; }

        /// <inheritdoc />
        public void Activate(Tile tile)
        {
            Trace.Debug(LogTag, "activate tile: {0}", tile.MapCenter.ToString());
            Scheduler.MainThread.Schedule(() => ProcessWithChildren(tile, true));
        }

        /// <inheritdoc />
        public void Deactivate(Tile tile)
        {
            Trace.Debug(LogTag, "deactivate tile: {0}", tile.MapCenter.ToString());
            Scheduler.MainThread.Schedule(() => ProcessWithChildren(tile, false));
        }

        /// <inheritdoc />
        public void Destroy(Tile tile)
        {
            Trace.Debug(LogTag, "destroy tile: {0}", tile.MapCenter.ToString());
            tile.Registry.Dispose();
            Scheduler.MainThread.Schedule(() => DestroyWithChildren(tile));
        }

        /// <summary> Destroys tile using UnityEngine.Object.Destroy. </summary>
        /// <param name="tile">Tile.</param>
        protected virtual void DestroyWithChildren(Tile tile)
        {
            var parent = tile.GameObject.GetComponent<GameObject>();
            // NOTE should it be recursive as child may have children?
            foreach (Transform child in parent.transform)
                UnityEngine.Object.Destroy(child.gameObject);

            UnityEngine.Object.Destroy(parent);
            // NOTE this is required to release meshes as they are not garbage collected
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        /// <summary> Calls UnityEngine.GameObject.SetActive(active) for given tile. </summary>
        /// <param name="tile">Tile.</param>
        /// <param name="active">Active flag.</param>
        protected virtual void ProcessWithChildren(Tile tile, bool active)
        {
            tile.GameObject.GetComponent<GameObject>().SetActive(active);
        }
    }
}
