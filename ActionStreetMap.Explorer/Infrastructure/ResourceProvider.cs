using System.Collections.Generic;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Defines behavior of Unity's resource loader/provider. </summary>
    public interface IResourceProvider
    {
        /// <summary> Gets material. </summary>
        /// <param name="key">Key.</param>
        /// <returns>Material.</returns>
        Material GetMaterial(string key);

        /// <summary> Gets gradient wrapper. </summary>
        /// <param name="key">Key.</param>
        /// <returns> Objects which wraps unity gradient.</returns>
        GradientWrapper GetGradient(string key);
    }

    /// <summary> Default, dictionary based implementation of IResourceProvider. </summary>
    internal class UnityResourceProvider : IResourceProvider
    {
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private readonly Dictionary<string, GradientWrapper> _gradients = new Dictionary<string, GradientWrapper>(16);

        /// <inheritdoc />
        public Material GetMaterial(string key)
        {
            if (!_materials.ContainsKey(key))
                _materials[key] = Resources.Load<Material>(key);

            return _materials[key];
        }

        /// <inheritdoc />
        public GradientWrapper GetGradient(string key)
        {
            if (!_gradients.ContainsKey(key))
            {
                lock (_gradients)
                {
                    if (!_gradients.ContainsKey(key))
                    {
                        var value = GradientUtils.ParseGradient(key);
                        _gradients.Add(key, value);
                        return value;
                    }
                }
            }
            return _gradients[key];
        }
    }
}
