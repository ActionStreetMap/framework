using System;
using System.Collections;
using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Primitives
{
    /// <summary>
    ///     A double key dictionary.
    /// </summary>
    /// <typeparam name="K">
    ///     The first key type.
    /// </typeparam>
    /// <typeparam name="T">
    ///     The second key type.
    /// </typeparam>
    /// <typeparam name="V">
    ///     The value type.
    /// </typeparam>
    /// <remarks>
    ///     See http://noocyte.wordpress.com/2008/02/18/double-key-dictionary/
    ///     A Remove method was added.
    /// </remarks>
    public class DoubleKeyDictionary<K, T, V> : IEnumerable<DoubleKeyPairValue<K, T, V>>,
        IEquatable<DoubleKeyDictionary<K, T, V>>
    {
        private readonly object _lockObj = new object();

        /// <summary>
        ///     The m_inner dictionary.
        /// </summary>
        private Dictionary<T, V> m_innerDictionary;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoubleKeyDictionary{K,T,V}" /> class.
        /// </summary>
        public DoubleKeyDictionary()
        {
            OuterDictionary = new Dictionary<K, Dictionary<T, V>>();
        }

        /// <summary>
        ///     Gets or sets OuterDictionary.
        /// </summary>
        private Dictionary<K, Dictionary<T, V>> OuterDictionary { get; set; }

        /// <summary>
        ///     Gets or sets the value with the specified indices.
        /// </summary>
        /// <value></value>
        public V this[K index1, T index2]
        {
            get { return OuterDictionary[index1][index2]; }

            set { Add(index1, index2, value); }
        }

        /// <summary>
        ///     Clears this dictionary.
        /// </summary>
        public void Clear()
        {
            OuterDictionary.Clear();
            if (m_innerDictionary != null)
                m_innerDictionary.Clear();
        }

        /// <summary>
        ///     Adds the specified key.
        /// </summary>
        /// <param name="key1">
        ///     The key1.
        /// </param>
        /// <param name="key2">
        ///     The key2.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        public void Add(K key1, T key2, V value)
        { 
            lock (_lockObj)
            {
                if (OuterDictionary.ContainsKey(key1))
                {
                    if (m_innerDictionary.ContainsKey(key2))
                        OuterDictionary[key1][key2] = value;
                    else
                    {
                        m_innerDictionary = OuterDictionary[key1];
                        m_innerDictionary.Add(key2, value);
                        OuterDictionary[key1] = m_innerDictionary;
                    }
                }
                else
                {
                    m_innerDictionary = new Dictionary<T, V>();
                    m_innerDictionary[key2] = value;
                    OuterDictionary.Add(key1, m_innerDictionary);
                }
            }
        }

        /// <summary>
        ///     Determines whether the specified dictionary contains the key.
        /// </summary>
        /// <param name="index1">
        ///     The index1.
        /// </param>
        /// <param name="index2">
        ///     The index2.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the specified index1 contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(K index1, T index2)
        {
            if (!OuterDictionary.ContainsKey(index1))
                return false;

            if (!OuterDictionary[index1].ContainsKey(index2))
                return false;

            return true;
        }

        /// <summary>
        ///     Equalses the specified other.
        /// </summary>
        public bool Equals(DoubleKeyDictionary<K, T, V> other)
        {
            // NOTE we don't care about this logic so far
            return false;
        }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns>
        /// </returns>
        public IEnumerator<DoubleKeyPairValue<K, T, V>> GetEnumerator()
        {
            foreach (var outer in OuterDictionary)
            {
                foreach (var inner in outer.Value)
                   yield return new DoubleKeyPairValue<K, T, V>(outer.Key, inner.Key, inner.Value);
            }
        }

        /// <summary>
        ///     Removes the specified key.
        /// </summary>
        /// <param name="key1">
        ///     The key1.
        /// </param>
        /// <param name="key2">
        ///     The key2.
        /// </param>
        public void Remove(K key1, T key2)
        {
            OuterDictionary[key1].Remove(key2);
            if (OuterDictionary[key1].Count == 0)
                OuterDictionary.Remove(key1);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    ///     Represents two keys and a value.
    /// </summary>
    /// <typeparam name="K">
    ///     First key type.
    /// </typeparam>
    /// <typeparam name="T">
    ///     Second key type.
    /// </typeparam>
    /// <typeparam name="V">
    ///     Value type.
    /// </typeparam>
    public class DoubleKeyPairValue<K, T, V>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DoubleKeyPairValue{K,T,V}" /> class.
        /// </summary>
        /// <param name="key1">
        ///     The key1.
        /// </param>
        /// <param name="key2">
        ///     The key2.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        public DoubleKeyPairValue(K key1, T key2, V value)
        {
            Key1 = key1;
            Key2 = key2;
            Value = value;
        }

        /// <summary>
        ///     Gets or sets the key1.
        /// </summary>
        /// <value>The key1.</value>
        public K Key1 { get; set; }

        /// <summary>
        ///     Gets or sets the key2.
        /// </summary>
        /// <value>The key2.</value>
        public T Key2 { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public V Value { get; set; }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Key1 + " - " + Key2 + " - " + Value;
        }
    }
}