using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary> Provides pool of lists of certain size. </summary>
    internal class ObjectListPool<T>
    {
        private readonly LockFreeStack<List<T>> _objectStack;

        /// <summary> Creates <see cref="ObjectListPool{T}"/>. </summary>
        public ObjectListPool()
        {
            _objectStack = new LockFreeStack<List<T>>();
        }

        /// <summary> Returns list from pool or create new one. </summary>
        /// <returns> List.</returns>
        public List<T> New(int capacity)
        {
            return _objectStack.Pop() ?? new List<T>(capacity);
        }

        /// <summary> Stores list in pool. </summary>
        /// <param name="list">List to store.</param>
        /// <param name="isClean"></param>
        public void Store(List<T> list, bool isClean)
        {
            if (!isClean) list.Clear();
            _objectStack.Push(list);
        }
    }
}
