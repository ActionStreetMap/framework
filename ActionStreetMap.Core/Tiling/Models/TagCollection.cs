using System;
using System.Collections;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Tiling.Models
{
    /// <summary> Represents tag collection. </summary>
    public class TagCollection : IEnumerable<KeyValuePair<string, string>>
    {
        private bool _isReadOnly;

        private readonly List<string> _keys;
        private readonly List<string> _values;

        /// <summary> Creates instance of <see cref="TagCollection"/>. </summary>
        /// <param name="capacity"></param>
        public TagCollection(int capacity) 
        {
            _keys = new List<string>(capacity);
            _values = new List<string>(capacity);
        }

        internal TagCollection()
        {
            _keys = new List<string>();
            _values = new List<string>();
        }

        /// <summary> Adds tag with given key and value to collection. </summary>
        public void Add(string key, string value)
        {
            if (_isReadOnly) throw new InvalidOperationException(Strings.CannotAddTagCollection);

            _keys.Add(key);
            _values.Add(value);
        }

        /// <summary> Gets tag for given index. </summary>
        public KeyValuePair<string, string> this[int index] 
        { 
            get { return new KeyValuePair<string, string>(_keys[index], _values[index]); } 
        }

        /// <summary> Gets value for given key. </summary>
        public string this[string key] { get { return _values[IndexOf(key)]; } }

        /// <summary> Gets value by given index. </summary>
        public string ValueAt(int index) { return _values[index]; }

        /// <summary> Gets key by given index. </summary>
        public string KeyAt(int index) { return _keys[index]; }

        /// <summary> Gets index of given key. </summary>
        public int IndexOf(string key)
        {
            if (!_isReadOnly)
                throw new InvalidOperationException(Strings.CannotSearchTagCollection);
            
            return _keys.BinarySearch(key, StringComparer.OrdinalIgnoreCase); 
        }

        /// <summary> Makes collection readonly. </summary>
        public TagCollection AsReadOnly()
        {
            if (_isReadOnly) return this;

            if (_keys.Count < _keys.Capacity)
            {
                _keys.TrimExcess();
                _values.TrimExcess();
            }
            SortLists();
            _isReadOnly = true;

            return this;
        }

        private void SortLists()
        {
            // bubble sort
            bool stillGoing = true;
            var count = _keys.Count - 1;
            while (stillGoing)
            {
                stillGoing = false;
                for (int i = 0; i < count; i++)
                {
                    string keyX = _keys[i];
                    string keyY = _keys[i + 1];
                    string valueX = _values[i];
                    string valueY = _values[i + 1];
                    if (StringComparer.OrdinalIgnoreCase.Compare(keyX, keyY) > 0)
                    {
                        _keys[i] = keyY;
                        _keys[i + 1] = keyX;
                        _values[i] = valueY;
                        _values[i + 1] = valueX;
                        stillGoing = true;
                    }
                }
            }
        }

        /// <summary> Merges tag collection to current. </summary>
        public void Merge(TagCollection other)
        {
            foreach (var kv in other)
            {
                var index = IndexOf(kv.Key);
                if (index < 0)
                {
                    _isReadOnly = false;
                    Add(kv.Key, kv.Value);
                    // NOTE that's not nice: O(N^3) complexity, hopefully we don't expect big collections here
                    // So, can use this approach instead to allocate extra memory
                    SortLists();
                    _isReadOnly = true;
                }
            }
        }

        /// <summary> Gets count of items </summary>
        public int Count { get { return _keys.Count; } }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return new KeyValuePair<string, string>(_keys[i], _values[i]);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}
