using System;
using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary> ObjectArrayPool. this is naive implementation. </summary>
    internal class ObjectArrayPool<T>
    {
        private readonly object _lockObj = new object();
        private readonly Stack<T[,]> _objectArray2Stack;
        private readonly Stack<T[,,]> _objectArray3Stack;

        /// <summary> Creates <see cref="ObjectArrayPool{T}"/>. </summary>
        /// <param name="initialBufferSize">Initial buffer size.</param>
        public ObjectArrayPool(int initialBufferSize)
        {
            _objectArray2Stack = new Stack<T[,]>(initialBufferSize);
            _objectArray3Stack = new Stack<T[,,]>(initialBufferSize);
        }

        /// <summary> Returns list from pool or create new one. </summary>
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

        /// <summary> Returns list from pool or create new one. </summary>
        public T[,] New(int length1, int length2)
        {
            // TODO check length!
            // NOTE this is naive implementation used only for one purpose so far.
            lock (_lockObj)
            {
                if (_objectArray2Stack.Count > 0)
                {
                    var array = _objectArray2Stack.Pop();
                    return array;
                }
            }
            return new T[length1, length2];
        }

        /// <summary> Stores list in pool. </summary>
        /// <param name="arrayObj">Array to store.</param>
        public void Store(object arrayObj)
        {
            // TODO this looks ugly
            var array3 = arrayObj as T[,,];
            if (array3 != null)
            {
                Array.Clear(array3, 0, array3.Length);
                lock (_lockObj)
                {
                    // Do not store more than one
                    if (_objectArray3Stack.Count == 0)
                        _objectArray3Stack.Push(array3);
                    return;
                }
            }

            var array2 = arrayObj as T[,];
            if (array2 != null)
            {
                Array.Clear(array2, 0, array2.Length);
                lock (_lockObj)
                {
                    // Do not store more than one
                    if (_objectArray2Stack.Count == 0)
                        _objectArray2Stack.Push(array2);
                    return;
                }
            }
        }
    }
}
