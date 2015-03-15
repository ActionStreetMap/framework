using System;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Represents default GameObject factory. </summary>
    internal class GameObjectFactory : IGameObjectFactory
    {
        /// <inheritdoc />
        public virtual IGameObject CreateNew(string name)
        {
            return new UnityGameObject(name);
        }

        /// <inheritdoc />
        public IGameObject CreateNew(string name, IGameObject parent)
        {
            var go = CreateNew(name);
            if (go.IsEmpty)
            {
                Scheduler.MainThread.Schedule(() =>
                {
                    go.AddComponent(new GameObject());
                    if (go is UnityGameObject)
                        (go as UnityGameObject).SetParent(parent);
                });
            }
            else
                go.Parent = parent;

            return go;
        }
    }
}
