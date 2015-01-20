using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Primitives
{
    public class SafeDictionary<TKey, TValue>
    {
        private readonly object _lockObj = new object();
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get
            {
                lock (_lockObj)
                {
                    return _dictionary[key];
                }
            }
            set
            {
                lock (_lockObj)
                {
                    _dictionary[key] = value;
                }
            }
        }

        public bool TryAddValue(TKey key, TValue value)
        {
            lock (_lockObj)
            {
                if (_dictionary.ContainsKey(key)) 
                    return false;

                _dictionary.Add(key, value);
                return true;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lockObj)
            {
                return _dictionary.TryGetValue(key, out value);
            }
        }
    }
}