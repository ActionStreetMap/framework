using System;
using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary> ObjectTypePool, naive implementation. </summary>
    internal class ObjectTypePool<T>
    {
        private readonly object _lockObj = new object();
        private readonly int _maxSize;
        private readonly Stack<T> _objectStack;

        /// <summary> Creates <see cref="ObjectTypePool{T}"/>. </summary>
        /// <param name="initialBufferSize">Initial buffer size.</param>
        /// <param name="maxSize">Max object count.</param>
        public ObjectTypePool(int initialBufferSize, int maxSize = int.MaxValue)
        {
            _objectStack = new Stack<T>(initialBufferSize);
            _maxSize = maxSize;
        }

        public T New(Func<T> factoryMethod)
        {
            lock (_lockObj)
            {
                if (_objectStack.Count > 0)
                {
                    var list = _objectStack.Pop();
                    return list;
                }
            }
            return factoryMethod.Invoke();
        }

        public void Store(T instance)
        {
            lock (_lockObj)
            {
                if ( _objectStack.Count < _maxSize)
                {
                    _objectStack.Push(instance);
                }
            }
        }
    }
}
