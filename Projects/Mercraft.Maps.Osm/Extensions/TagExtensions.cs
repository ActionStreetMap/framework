﻿using System.Collections.Generic;
using System.Linq;

namespace Mercraft.Maps.Osm.Extensions
{
    /// <summary>
    ///     Contains extensions that aid in interpreting some of the OSM-tags.
    /// </summary>
    public static class TagExtensions
    {
        private static readonly string[] BooleanTrueValues = {"yes", "true", "1"};
        private static readonly string[] BooleanFalseValues = {"no", "false", "0"};


        /// <summary>
        ///     Returns true if the given tags key has an associated value that can be interpreted as true.
        /// </summary>
        public static bool IsTrue(this IList<KeyValuePair<string, string>> tags, string tagKey)
        {
            if (tags == null || IsNullOrWhiteSpace(tagKey))
                return false;

            // TryGetValue tests if the 'tagKey' is present, returns true if the associated value can be interpreted as true.
            // returns false if the associated value can be interpreted as false.
            string tagValue;
            return tags.TryGetValue(tagKey, out tagValue) &&
                   BooleanTrueValues.Contains(tagValue.ToLowerInvariant());
        }

        /// <summary>
        ///     Returns true if the given tags key has an associated value that can be interpreted as false.
        /// </summary>
        public static bool IsFalse(this IList<KeyValuePair<string, string>> tags, string tagKey)
        {
            if (tags == null || IsNullOrWhiteSpace(tagKey))
                return false;
            string tagValue;
            return tags.TryGetValue(tagKey, out tagValue) &&
                   BooleanFalseValues.Contains(tagValue.ToLowerInvariant());
        }

        #region Collection helpers

        /// <summary>
        ///     Returns true if the given tag exists.
        /// </summary>
        public static bool TryGetValue(this IList<KeyValuePair<string, string>> tags, string key, out string value)
        {
            foreach (var tag in tags)
            {
                if (tag.Key == key)
                {
                    value = tag.Value;
                    return true;
                }
            }
            value = string.Empty;
            return false;
        }

        /// <summary>
        ///     Returns true if the given key is found in this tags collection.
        /// </summary>
        public static bool ContainsKey(this IList<KeyValuePair<string, string>> tags, string key)
        {
            return tags != null && tags.Any(tag => tag.Key == key);
        }

        #endregion

        public static bool IsNullOrWhiteSpace(string str)
        {
            return str == null || str.Trim() == "";
        }
    }
}