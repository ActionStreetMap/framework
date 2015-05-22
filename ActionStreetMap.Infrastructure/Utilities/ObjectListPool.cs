using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary> Provides pool of lists of certain size. </summary>
    public class ObjectListPool<T>
    {
        private readonly object _lockObj = new object();
        private readonly Stack<List<T>> _objectStack;

        /// <summary> Creates <see cref="ObjectListPool{T}"/>. </summary>
        /// <param name="initialBufferSize">Initial buffer size.</param>
        public ObjectListPool(int initialBufferSize)
        {
            _objectStack = new Stack<List<T>>(initialBufferSize);
        }

        /// <summary> Returns list from pool or create new one. </summary>
        /// <returns> List.</returns>
        public List<T> New(int capacity)
        {
            lock (_lockObj)
            {
                if (_objectStack.Count > 0)
                {
                    var list = _objectStack.Pop();
                    return list;
                }
            }
            return new List<T>(capacity);
        }

        /// <summary> Stores list in pool. </summary>
        /// <param name="list">List to store.</param>
        /// <param name="isClean"></param>
        public void Store(List<T> list, bool isClean)
        {
            if (!isClean) list.Clear();
            lock (_lockObj)
            {
                _objectStack.Push(list);
            }
        }
    }
}
