using System;
using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Config
{
    /// <summary> Represents code-based configuration section. </summary>
    /// <remarks> This is naive, not performance optimized implementation.</remarks>
    public class CodeConfigSection: IConfigSection
    {
        private readonly string _keyPrefix;
        private readonly Dictionary<string, object> _keyValueMap;

        /// <summary> Creates instance of <see cref="CodeConfigSection"/>. </summary>
        public CodeConfigSection()
        {
            _keyPrefix = "";
            _keyValueMap = new Dictionary<string, object>();
        }

        private CodeConfigSection(string keyPrefix, Dictionary<string, object> keyValueMap)
        {
            _keyPrefix = keyPrefix;
            _keyValueMap = keyValueMap;
        }

        /// <summary> Adds specific setting. </summary>
        public CodeConfigSection Add<T>(string path, T value)
        {
            _keyValueMap.Add(GetKey(path), value);
            return this;
        }

        #region IConfigSection implementation

        /// <inheritdoc />
        public IEnumerable<IConfigSection> GetSections(string xpath)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public IConfigSection GetSection(string xpath)
        {
            return new CodeConfigSection(GetKey(xpath), _keyValueMap);
        }

        /// <inheritdoc />
        public string GetString(string xpath, string defaultValue)
        {
            var key = GetKey(xpath);
            if (_keyValueMap.ContainsKey(key))
                return _keyValueMap[key].ToString();
            return defaultValue;
        }

        /// <inheritdoc />
        public int GetInt(string xpath, int defaultValue)
        {
            int value;
            return int.TryParse(GetString(xpath, null), out value) ? value : defaultValue;
        }

        /// <inheritdoc />
        public float GetFloat(string xpath, float defaultValue)
        {
            float value;
            return float.TryParse(GetString(xpath, null), out value) ? value : defaultValue;
        }

        /// <inheritdoc />
        public bool GetBool(string xpath, bool defaultValue)
        {
            bool value;
            return bool.TryParse(GetString(xpath, null), out value) ? value : defaultValue;
        }

        /// <inheritdoc />
        public Type GetType(string xpath)
        {
            return Type.GetType(GetString(xpath, null));
        }

        /// <inheritdoc />
        public T GetInstance<T>(string xpath)
        {
            return (T)Activator.CreateInstance(GetType(xpath));
        }

        /// <inheritdoc />
        public T GetInstance<T>(string xpath, params object[] args)
        {
            return (T)Activator.CreateInstance(GetType(xpath), args);
        }

        #endregion

        private string GetKey(string path)
        {
            return String.Format(@"{0}/{1}", _keyPrefix, path);
        }
    }
}
