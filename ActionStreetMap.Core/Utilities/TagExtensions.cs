using ActionStreetMap.Core.Tiling.Models;
using System.Collections.Generic;

namespace ActionStreetMap.Core.Utilities
{
    internal static class TagExtensions
    {
        public static bool ContainsKey(this TagCollection tags, string key)
        {
            return tags.GetIndexOf(key) >= 0;
        }

        public static bool ContainsKeyValue(this TagCollection tags, string key, string value)
        {
            string actualValue;
            return TryGetValue(tags, key, out actualValue) && actualValue == value;
        }

        public static bool IsNotEqual(this TagCollection tags, string key, string value)
        {
            string actualValue;
            return TryGetValue(tags, key, out actualValue) && actualValue != value;
        }

        public static bool IsLess(this TagCollection tags, string key, string value)
        {
            return CompareValues(tags, key, value, false);
        }

        public static bool IsGreater(this TagCollection tags, string key, string value)
        {
            return CompareValues(tags, key, value, true);
        }

        public static bool TryGetValue(this TagCollection tags, string key, out string value)
        {
            return TryGetValueInternal(tags, key, out value);
        }

        private static bool TryGetValueInternal(TagCollection tags, string key, out string value)
        {
            value = null;
            if (tags == null) return false;

            var index = tags.GetIndexOf(key);
            if (index < 0) return false;

            value = tags.ValueAt(index);
            return true;
        }

        private static bool CompareValues(TagCollection tags, string key, string value, bool isGreater)
        {
            if (tags == null) return false;

            string actualValue;
            if (!TryGetValue(tags, key, out actualValue))
                return false;

            float target = float.Parse(value);
            float fValue;
            return float.TryParse(actualValue, out fValue) && (isGreater ? fValue > target : fValue < target);
        }
    }
}
