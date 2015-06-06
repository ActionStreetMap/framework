using System;
using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Defines default object pool. </summary>
    internal class ObjectPool: IObjectPool
    {
        private readonly Dictionary<Type, object> _listPoolMap = new Dictionary<Type, object>(8);
        private readonly Dictionary<Type, object> _objectPoolMap = new Dictionary<Type, object>(2);

        #region Objects

        /// <inheritdoc />
        public T NewObject<T>()
        {
            return (_objectPoolMap[typeof(T)] as ObjectTypePool<T>).New();
        }

        /// <inheritdoc />
        public void StoreObject<T>(T instance)
        {
            (_objectPoolMap[typeof(T)] as ObjectTypePool<T>).Store(instance);
        }

        #endregion

        /// <inheritdoc />
        public List<T> NewList<T>()
        {
            return NewList<T>(2);
        }

        /// <inheritdoc />
        public List<T> NewList<T>(int capacity)
        {
            return (_listPoolMap[typeof(T)] as ObjectListPool<T>).New(capacity);
        }

        /// <inheritdoc />
        public void StoreList<T>(List<T> list, bool isClean = false)
        {
            (_listPoolMap[typeof(T)] as ObjectListPool<T>).Store(list, isClean);
        }

        /// <inheritdoc />
        public IObjectPool RegisterObjectType<T>(Func<T> factoryMethod, int capacity)
        {
            var type = typeof (T);
            _objectPoolMap.Add(type, new ObjectTypePool<T>(factoryMethod, capacity));
            return this;
        }

        /// <inheritdoc />
        public IObjectPool RegisterListType<T>(int capacity)
        {
            var type = typeof (T);
            _listPoolMap.Add(type, new ObjectListPool<T>(capacity));
            return this;
        }

        /// <inheritdoc />
        public void Shrink()
        {
            // TODO reduce amount of stored data
        }
    }
}
