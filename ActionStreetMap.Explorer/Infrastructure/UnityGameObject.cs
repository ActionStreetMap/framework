using System;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Reactive;
using UnityEngine;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Wrapper of real Unity's GameObject. </summary>
    internal class UnityGameObject : IGameObject
    {
        private readonly string _name;
        private object _gameObject;

        /// <summary> Creates UnityGameObject. Internally creates Unity's GameObject with given name. </summary>
        /// <param name="name">Name.</param>
        public UnityGameObject(string name)
        {
            _name = name;
        }

        /// <summary> Creates UnityGameObject. </summary>
        /// <param name="name">Name.</param>
        /// <param name="gameObject">GameObject to be wrapperd.</param>
        public UnityGameObject(string name, GameObject gameObject)
        {
            _gameObject = gameObject;
            if (!string.IsNullOrEmpty(name))
                gameObject.name = name;
            _name = name;
        }

        /// <inheritdoc />
        public T AddComponent<T>(T component)
        {
            // work-around to run Unity-specific
            if (typeof(T).IsAssignableFrom(typeof(GameObject)))
            {
                if (_gameObject != null)
                    throw new InvalidOperationException("GameObject is already added!");

                _gameObject = component;
                if (!string.IsNullOrEmpty(_name))
                    (component as GameObject).name = _name;
            }
            return component;
        }

        /// <inheritdoc />
        public T GetComponent<T>()
        {
            if (_gameObject != null)
                return (T) _gameObject;

            throw new InvalidOperationException("GameObject isn't yet added!");
        }

        /// <inheritdoc />
        public bool IsEmpty { get { return _gameObject == null; } }

        /// <inheritdoc />
        public string Name { get { return _name; } }

        /// <inheritdoc />
        public IGameObject Parent
        {
            set
            {
                Scheduler.MainThread.Schedule(
                    () => { (_gameObject as GameObject).transform.parent = value.GetComponent<GameObject>().transform; });
            }
        }

        /// <summary>  Sets parent on current thread. </summary>
        internal void SetParent(IGameObject parent)
        {
            (_gameObject as GameObject).transform.parent = parent.GetComponent<GameObject>().transform;
        }
    }
}
