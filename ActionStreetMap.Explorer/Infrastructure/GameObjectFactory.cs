using System;
using ActionStreetMap.Core.Unity;
using UnityEngine;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary>
    ///     Represents default GameObject factory.
    /// </summary>
    public class GameObjectFactory : IGameObjectFactory
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
            go.Parent = parent;
            return go;
        }

        /// <inheritdoc />
        public IGameObject Wrap(string name, object gameObject)
        {
            var instance = gameObject as GameObject;
            if(instance == null)
                throw new ArgumentException(
                    String.Format("Unable to wrap {0}. Expecting UnityEngine.GameObject", gameObject), "gameObject");

            return new UnityGameObject(name, instance);
        }
    }
}
