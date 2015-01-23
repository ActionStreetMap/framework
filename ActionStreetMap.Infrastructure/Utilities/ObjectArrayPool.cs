using System;
using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary>
    ///     ObjectArrayPool
    /// </summary>
    public class ObjectArrayPool<T>
    {
        private readonly object _lockObj = new object();
        private readonly Stack<T[,,]> _objectArray3Stack;

        /// <summary>
        ///     Creates ObjectListPool.
        /// </summary>
        /// <param name="initialBufferSize">Initial buffer size.</param>
        public ObjectArrayPool(int initialBufferSize)
        {
            _objectArray3Stack = new Stack<T[, ,]>(initialBufferSize);
        }

        /// <summary>
        ///     Returns list from pool or create new one.
        /// </summary>
        public T[, ,] New(int length1, int length2, int length3)
        {
            // TODO check length!
            // NOTE this is naive implementation used only for one purpose so far.
            lock (_lockObj)
            {
                if (_objectArray3Stack.Count > 0)
                {
                    var list = _objectArray3Stack.Pop();
                    return list;
                }
            }
            return new T[length1, length2, length3];
        }

        /// <summary>
        ///     Stores list in pool.
        /// </summary>
        /// <param name="arrayObj">Array to store.</param>
        public void Store(object arrayObj)
        {
            var array = arrayObj as T[,,];
            Array.Clear(array, 0, array.Length);
            lock (_lockObj)
            {
                _objectArray3Stack.Push(array);
            }
        }
    }
}
