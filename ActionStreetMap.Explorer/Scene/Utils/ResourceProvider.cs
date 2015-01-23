using System.Collections.Generic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Utils
{
    /// <summary>
    ///     Defines behavior of Unity's resource loader/provider.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        ///     Gets game object by key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Game object.</returns>
        GameObject GetGameObject(string key);

        /// <summary>
        ///     Gets material.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Material.</returns>
        Material GetMatertial(string key);

        /// <summary>
        ///     Gets Texture.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Texture.</returns>
        Texture GetTexture(string key);

        /// <summary>
        ///     Gets Texture2D.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Texture2D.</returns>
        Texture2D GetTexture2D(string key);
    }

    /// <summary>
    ///     Default, dictionary based implementation of IResourceProvider
    /// </summary>
    public class UnityResourceProvider : IResourceProvider
    {
        private readonly Dictionary<string, GameObject> _gameObjects = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private readonly Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
        private readonly Dictionary<string, Texture2D> _textures2D = new Dictionary<string, Texture2D>();

        /// <inheritdoc />
        public GameObject GetGameObject(string key)
        {
            if (!_gameObjects.ContainsKey(key))
                _gameObjects[key] = Resources.Load<GameObject>(key);

            return _gameObjects[key];
        }

        /// <inheritdoc />
        public Material GetMatertial(string key)
        {
            if (!_materials.ContainsKey(key))
                _materials[key] = Resources.Load<Material>(key);

            return _materials[key];
        }

        /// <inheritdoc />
        public Texture GetTexture(string key)
        {
            if (!_textures.ContainsKey(key))
                _textures[key] = Resources.Load<Texture>(key);

            return _textures[key];
        }

        /// <inheritdoc />
        public Texture2D GetTexture2D(string key)
        {
            if (!_textures2D.ContainsKey(key))
                _textures2D[key] = Resources.Load<Texture2D>(key);

            return _textures2D[key];
        }
    }
}
