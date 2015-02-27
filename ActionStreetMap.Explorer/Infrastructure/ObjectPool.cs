using System;
using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Defines default object pool. </summary>
    internal class ObjectPool: IObjectPool
    {
        private readonly Dictionary<Type, object> _listPoolMap = new Dictionary<Type, object>(8);
        private readonly ObjectArrayPool<float> _splatMapArrayPool = new ObjectArrayPool<float>(1);

        private readonly Dictionary<Type, object> _objectPoolMap = new Dictionary<Type, object>(2);

        #region Objects

        /// <inheritdoc />
        public T NewObject<T>(Func<T> factoryMethod)
        {
            Type type = typeof(T);
            if (!_objectPoolMap.ContainsKey(type))
                CreateObjectPoolMap<T>(type);

            return (_objectPoolMap[type] as ObjectTypePool<T>).New(factoryMethod);
        }

        /// <inheritdoc />
        public T NewObject<T>() where T : new()
        {
           return NewObject<T>(() => new T());
        }

        /// <inheritdoc />
        public void StoreObject<T>(T instance)
        {
            Type type = typeof(T);
            if (!_objectPoolMap.ContainsKey(type))
                CreateObjectPoolMap<T>(type);
            (_objectPoolMap[type] as ObjectTypePool<T>).Store(instance);
        }

        private void CreateObjectPoolMap<T>(Type type)
        {
            lock (_objectPoolMap)
            {
                if (!_objectPoolMap.ContainsKey(type))
                    _objectPoolMap.Add(type, new ObjectTypePool<T>(1, 1));
            }
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
            // TODO choose the best list from pool based on provided capacity
            Type type = typeof(T);
            if (!_listPoolMap.ContainsKey(type))
            {
                lock (_listPoolMap)
                {
                    if (!_listPoolMap.ContainsKey(type))
                        _listPoolMap.Add(type, new ObjectListPool<T>(4));
                }
            }

            return (_listPoolMap[type] as ObjectListPool<T>).New(capacity);
        }

        /// <inheritdoc />
        public void StoreList<T>(List<T> list, bool isClean = false)
        {
            Type type = typeof(T);
            if (!_listPoolMap.ContainsKey(type))
            {
                lock (_listPoolMap)
                {
                    if (!_listPoolMap.ContainsKey(type))
                        _listPoolMap.Add(type, new ObjectListPool<T>(64));
                }
            }

            (_listPoolMap[type] as ObjectListPool<T>).Store(list, isClean);
        }

        /// <inheritdoc />
        public T[, ,] NewArray<T>(int length1, int length2, int length3)
        {
            // NOTE workaround to have splat map outside terrain builder
            // TODO can be improved
            return _splatMapArrayPool.New(length1, length2, length3) as T[, ,];
        }

        /// <inheritdoc />
        public void StoreArray<T>(T[,] array)
        {
            _splatMapArrayPool.Store(array);
        }

        /// <inheritdoc />
        public T[,] NewArray<T>(int length1, int length2)
        {
            // NOTE workaround to have splat map outside terrain builder
            // TODO can be improved
            return _splatMapArrayPool.New(length1, length2) as T[,];
        }

        /// <inheritdoc />
        public void StoreArray<T>(T[, ,] array)
        {
            _splatMapArrayPool.Store(array);
        }

        /// <inheritdoc />
        public void Shrink()
        {
            // TODO reduce amount of stored data
        }
    }
}
