using System.Collections;
using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Primitives
{
    public class SafeHashSet<T>: IEnumerable<T>
    {
        private readonly object _lockObj = new object();
        private readonly HashSet<T> _hashSet = new HashSet<T>();

        public bool Contains(T item)
        {
            lock (_lockObj)
            {
                return _hashSet.Contains(item);
            }
        }

        public bool TryAdd(T item)
        {
            lock (_lockObj)
            {
                if (_hashSet.Contains(item))
                    return false;
                _hashSet.Add(item);
                return true;
            }
        }

        public bool TryRemove(T item)
        {
            lock (_lockObj)
            {
                if (!_hashSet.Contains(item))
                    return false;
                _hashSet.Remove(item);
                return true;
            }
        }

        public void Clear()
        {
            _hashSet.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _hashSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
