using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
            if (_isReadOnly) throw new InvalidOperationException(Strings.CannotAddTags);

            _keys.Add(key);
            _values.Add(value);
        }

        /// <summary> Gets tag for given index. </summary>
        public KeyValuePair<string, string> this[int index] 
        { 
            get { return new KeyValuePair<string, string>(_keys[index], _values[index]); } 
        }

        /// <summary> Gets value for given key. </summary>
        public string this[string key] { get { return _values[GetIndexOf(key)]; } }

        /// <summary> Gets value by given index. </summary>
        public string ValueAt(int index) { return _values[index]; }

        /// <summary> Gets key by given index. </summary>
        public string KeyAt(int index) { return _keys[index]; }

        /// <summary>  Gets index of given key. </summary>
        public int GetIndexOf(string key)
        {
            return _keys.BinarySearch(key, StringComparer.OrdinalIgnoreCase); 
        }

        /// <summary> Makes collection readonly. </summary>
        public TagCollection AsReadOnly()
        {
            _isReadOnly = true;
            if (_keys.Count < _keys.Capacity)
            {
                _keys.TrimExcess();
                _values.TrimExcess();
            }
            return this;
        }

        /// <summary> Allows to add new tags to collection. </summary>
        internal TagCollection AllowAdd()
        {
            _isReadOnly = false;
            return this;
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
