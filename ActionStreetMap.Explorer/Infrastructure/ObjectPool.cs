using System;
using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary>
    ///     Defines default object pool.
    /// </summary>
    public class ObjectPool: IObjectPool
    {
        private readonly Dictionary<Type, object> _listPoolMap = new Dictionary<Type, object>(8);

        /// <inheritdoc />
        public T New<T>()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Store<T>(T obj)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public List<T> NewList<T>()
        {
            Type type = typeof(T);
            if (!_listPoolMap.ContainsKey(type))
                _listPoolMap.Add(type, new ObjectListPool<T>(64, 32));

            return (_listPoolMap[type] as ObjectListPool<T>).New() as List<T>;
        }

        /// <inheritdoc />
        public List<T> NewList<T>(int capacity)
        {
            // TODO choose the best list from pool based on provided capacity
            return NewList<T>();
        }

        /// <inheritdoc />
        public void Store<T>(List<T> list)
        {
            Type type = typeof(T);
            if (!_listPoolMap.ContainsKey(type))
                _listPoolMap.Add(type, new ObjectListPool<T>(64, 32));

            (_listPoolMap[type] as ObjectListPool<T>).Store(list);
        }

        /// <inheritdoc />
        public void Shrink()
        {
            // TODO reduce amount of stored data
        }
    }
}
