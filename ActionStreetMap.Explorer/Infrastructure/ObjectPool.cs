using System;
using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary>
    ///     Defines default object pool.
    /// </summary>
    public class ObjectPool: IObjectPool
    {
        private readonly Dictionary<Type, object> _listPoolMap = new Dictionary<Type, object>(8);
        private readonly ObjectArrayPool<float> _splatMapArrayPool = new ObjectArrayPool<float>(1);

        /// <inheritdoc />
        public List<T> NewList<T>()
        {
            Type type = typeof(T);
            if (!_listPoolMap.ContainsKey(type))
            {
                lock (_listPoolMap)
                {
                    if (!_listPoolMap.ContainsKey(type))
                        _listPoolMap.Add(type, new ObjectListPool<T>(64, 32));
                }
            }

            return (_listPoolMap[type] as ObjectListPool<T>).New();
        }

        /// <inheritdoc />
        public List<T> NewList<T>(int capacity)
        {
            // TODO choose the best list from pool based on provided capacity
            return NewList<T>();
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
                        _listPoolMap.Add(type, new ObjectListPool<T>(64, 32));
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
