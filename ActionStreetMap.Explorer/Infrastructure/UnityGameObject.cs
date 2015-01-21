using System;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary>
    ///     Wrapper of real Unity's GameObject.
    /// </summary>
    public class UnityGameObject : IGameObject
    {
        private readonly string _name;
        private GameObject _gameObject;

        /// <summary>
        ///     Creates UnityGameObject. Internally creates Unity's GameObject with given name.
        /// </summary>
        /// <param name="name">Name.</param>
        public UnityGameObject(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     Creates UnityGameObject.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="gameObject">GameObject to be wrapperd.</param>
        public UnityGameObject(string name, GameObject gameObject)
        {
            _gameObject = gameObject;
            _gameObject.name = name;
        }

        /// <inheritdoc />
        public T AddComponent<T>(T component)
        {
            // work-around to run Unity-specific
            if (typeof(T).IsAssignableFrom(typeof(GameObject)))
            {
                _gameObject = component as GameObject;
                _gameObject.name = _name;
            }
            return component;
        }

        /// <inheritdoc />
        public T GetComponent<T>()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public string Name
        {
            get { return _gameObject.name; }
            set { _gameObject.name = value; }
        }

        /// <inheritdoc />
        public IGameObject Parent
        {
            set
            {
                Scheduler.MainThread.Schedule(
                    () => { _gameObject.transform.parent = value.GetComponent<GameObject>().transform; });
            }
        }
    }
}
