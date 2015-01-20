using System;
using ActionStreetMap.Core.Unity;
using UnityEngine;

namespace ActionStreetMap.Models.Utils
{
    internal class GameObjectWrapper : IGameObject
    {
        private readonly string _name;
        private GameObject _gameObject;

        public GameObjectWrapper(string name, GameObject gameObject)
        {
            _gameObject = gameObject;
            _gameObject.name = name;
            _name = name;
        }

        public GameObjectWrapper(string name)
        {
            _name = name;
        }

        public T AddComponent<T>(T component)
        {
            if (typeof (T).IsAssignableFrom(typeof (GameObject)))
            {
                _gameObject = component as GameObject;
                _gameObject.name = _name;
            }

            return component;
        }

        public T GetComponent<T>()
        {
            throw new NotSupportedException();
        }

        public string Name { get; set; }
        public IGameObject Parent { set; private get; }
    }
}