using System;
using System.Text.RegularExpressions;

namespace ActionStreetMap.Core.Utils
{
    internal static class SanitizeHelper
    {
        private static readonly Regex RegexFloat = new Regex(@"(?<number>[0-9\.]*).*");
        
        public static string SanitizeFloat(string s)
        {
            var v = RegexFloat.Match(s);

            if (!v.Success || v.Groups["number"].Value == "")
                throw new ArgumentException("Unable to recognize float value", s);

            return v.Groups["number"].Value;
        }

        public static float ParseFloat(object obj)
        {
            if (obj is float)
                return (float)obj;
            return float.Parse(SanitizeFloat(obj as string));
        }
    }
}
