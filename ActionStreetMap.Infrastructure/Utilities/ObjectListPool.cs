using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary>
    ///     Provides pool of lists of certain size.
    /// </summary>
    public class ObjectListPool<T>
    {
        private readonly object _lockObj = new object();
        private readonly Stack<List<T>> _objectStack;
        private readonly int _listSize;

        /// <summary>
        ///     Creates ObjectListPool.
        /// </summary>
        /// <param name="initialBufferSize">Initial buffer size.</param>
        /// <param name="listSize">List capacity.</param>
        public ObjectListPool(int initialBufferSize, int listSize)
        {
            _objectStack = new Stack<List<T>>(initialBufferSize);
            _listSize = listSize;
        }

        /// <summary>
        ///     Returns list from pool or create new one.
        /// </summary>
        /// <returns>List.</returns>
        public List<T> New()
        {
            lock (_lockObj)
            {
                if (_objectStack.Count > 0)
                {
                    var list = _objectStack.Pop();
                    return list;
                }
            }
            return new List<T>(_listSize);
        }

        /// <summary>
        ///     Stores list in pool.
        /// </summary>
        /// <param name="list">List to store.</param>
        public void Store(List<T> list)
        {
            list.Clear();
            lock (_lockObj)
            {
                _objectStack.Push(list);
            }
        }
    }
}
