using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene
{
    /// <summary> Defines behavior of Unity's resource loader/provider. </summary>
    public interface IResourceProvider
    {
        /// <summary> Gets game object by key. </summary>
        /// <param name="key">Key.</param>
        /// <returns>Game object.</returns>
        GameObject GetGameObject(string key);

        /// <summary> Gets material. </summary>
        /// <param name="key">Key.</param>
        /// <returns>Material.</returns>
        Material GetMatertial(string key);

        /// <summary> Gets Texture. </summary>
        /// <param name="key">Key.</param>
        /// <returns>Texture.</returns>
        Texture GetTexture(string key);

        /// <summary> Gets Texture2D. </summary>
        /// <param name="key">Key.</param>
        /// <returns>Texture2D.</returns>
        Texture2D GetTexture2D(string key);

        /// <summary> Gets gradient wrapper. </summary>
        /// <param name="key">Key.</param>
        /// <returns> Objects which wraps unity gradient.</returns>
        GradientWrapper GetGradient(string key);
    }

    /// <summary> Default, dictionary based implementation of IResourceProvider. </summary>
    internal class UnityResourceProvider : IResourceProvider, IConfigurable
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Dictionary<string, GameObject> _gameObjects = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private readonly Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
        private readonly Dictionary<string, Texture2D> _textures2D = new Dictionary<string, Texture2D>();

        private Dictionary<string, GradientWrapper> _gradients;

        /// <summary> Creates instance of <see cref="UnityResourceProvider"/>. </summary>
        /// <param name="fileSystemService">File system service.</param>
        [Dependency]
        public UnityResourceProvider(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

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

        /// <inheritdoc />
        public GradientWrapper GetGradient(string key)
        {
            return _gradients[key];
        }

        public void Configure(IConfigSection configSection)
        {
            var gradientFilePath = configSection.GetString(@"gradients", null);
            _gradients = ParseGradients(gradientFilePath);
        }

        private Dictionary<string, GradientWrapper> ParseGradients(string gradientFilePath)
        {
            var gradientContent = _fileSystemService.ReadText(gradientFilePath);
            var json = JSON.Parse(gradientContent);

            var jsonGradients = json["gradients"].AsArray;
            var gradients = new Dictionary<string, GradientWrapper>(jsonGradients.Count);
            foreach (JSONNode jsonGrad in jsonGradients)
            {
                var name = jsonGrad["name"].Value;
                // colors
                var jsonColors = jsonGrad["color"].AsArray;
                var colors = new GradientWrapper.ColorKey[jsonColors.Count];
                for (var i = 0; i < jsonColors.Count; i++)
                {
                    var key = new GradientWrapper.ColorKey();
                    var colorArray = jsonColors[i]["c"].Value.Split(',');
                    key.Color = new Color(
                        int.Parse(colorArray[0]) / 255f, 
                        int.Parse(colorArray[1]) / 255f,
                        int.Parse(colorArray[2]) / 255f);
                    key.Time = float.Parse(jsonColors[i]["t"].Value);
                    colors[i] = key;
                }
                // alphas
                var jsonAlphas = jsonGrad["alpha"].AsArray;
                var alphas = new GradientWrapper.AlphaKey[jsonAlphas.Count];
                for (int i = 0; i < jsonAlphas.Count; i++)
                {
                    var key = new GradientWrapper.AlphaKey();
                    key.Alpha = float.Parse(jsonAlphas[i]["a"].Value);
                    key.Time = float.Parse(jsonAlphas[i]["t"].Value);
                    alphas[i] = key;
                }
                gradients.Add(name, new GradientWrapper(colors, alphas));
            }

            return gradients;
        }
    }
}
