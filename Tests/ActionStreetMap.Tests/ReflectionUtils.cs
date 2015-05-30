using System;

using System.Linq;
using System.Reflection;

namespace ActionStreetMap.Tests
{
    public static class ReflectionUtils
    {
        public static T GetFieldValue<T>(object instance, string fieldName)
        {
            return (T) instance.GetType()
                .GetFields(BindingFlags.Public |
                           BindingFlags.NonPublic |
                           BindingFlags.Instance).Single(f => f.Name == fieldName).GetValue(instance);
        }
    }
}
