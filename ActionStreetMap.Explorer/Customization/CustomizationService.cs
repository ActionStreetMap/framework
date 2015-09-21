using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Unity.Wrappers;
using UnityEngine;
using Component = ActionStreetMap.Infrastructure.Dependencies.Component;
using Object = System.Object;

namespace ActionStreetMap.Explorer.Customization
{
    /// <summary> Maintains list of customization properties. </summary>
    /// <remarks> Registration is not thread safe. </remarks>
    public sealed class CustomizationService
    {
        private readonly Object _lockObj = new object();

        private IContainer _container;
        private readonly Dictionary<string, Type> _modelBehaviours;
        private Dictionary<string, IModelBuilder> _modelBuilders;
        private readonly Dictionary<string, TextureAtlas> _textureAtlases;
        private readonly Dictionary<string, Material> _materials;
        private readonly Dictionary<string, GradientWrapper> _gradients;

        /// <summary> Creates instance of <see cref="CustomizationService"/>. </summary>
        internal CustomizationService(IContainer container)
        {
            _container = container;
            _modelBehaviours = new Dictionary<string, Type>(4);
            _textureAtlases = new Dictionary<string, TextureAtlas>(2);
            _materials = new Dictionary<string, Material>();
            _gradients = new Dictionary<string, GradientWrapper>(16);
        }

        #region Model Behaviours

        /// <summary> Registers model behaviour type. </summary>
        public CustomizationService RegisterBehaviour(string name, Type modelBehaviourType)
        {
            Guard.IsAssignableFrom(typeof (IModelBehaviour), modelBehaviourType);

            _modelBehaviours.Add(name, modelBehaviourType);
            return this;
        }

        /// <summary> Gets behaviour type by its name. </summary>
        public Type GetBehaviour(string name)
        {
            return _modelBehaviours[name];
        }

        #endregion

        #region Model Builders

        /// <summary> Registers model builder type. </summary>
        public CustomizationService RegisterBuilder(string name, Type modelBuilderType)
        {
            Guard.IsAssignableFrom(typeof (IModelBuilder), modelBuilderType);

            _container.Register(Component
                .For<IModelBuilder>()
                .Use(modelBuilderType)
                .Named(name)
                .Singleton());
            return this;
        }

        /// <summary> Registers model builder instance. </summary>
        public CustomizationService RegisterBuilder(IModelBuilder builder)
        {
            _container.RegisterInstance(builder, builder.Name);
            return this;
        }

        /// <summary> Gets model builder by its name. </summary>
        public IModelBuilder GetBuilder(string name)
        {
            if (_modelBuilders == null)
            {
                lock (_lockObj)
                {
                    if (_modelBuilders == null)
                    {
                        var modelBuilders = new Dictionary<string, IModelBuilder>(8);
                        foreach (var modelBuilder in _container.ResolveAll<IModelBuilder>())
                            modelBuilders.Add(modelBuilder.Name, modelBuilder);

                        _container = null;
                        _modelBuilders = modelBuilders;
                    }
                }
            }
            return _modelBuilders[name];
        }

        #endregion

        #region Texture Atlases

        /// <summary> Registers atals using name provided. </summary>
        public CustomizationService RegisterAtlas(string name, TextureAtlas atlas)
        {
            _textureAtlases.Add(name, atlas);
            return this;
        }

        /// <summary> Gets texture atlas by name. </summary>
        public TextureAtlas GetAtlas(string name)
        {
            return _textureAtlases[name];
        }

        #endregion

        #region Gradients

        /// <summary> Gets gradient from string. </summary>
        public GradientWrapper GetGradient(string gradientString)
        {
            if (!_gradients.ContainsKey(gradientString))
            {
                lock (_gradients)
                {
                    if (!_gradients.ContainsKey(gradientString))
                    {
                        var value = GradientUtils.ParseGradient(gradientString);
                        _gradients.Add(gradientString, value);
                        return value;
                    }
                }
            }
            return _gradients[gradientString];
        }

        #endregion

        #region Materials

        /// <summary> Gets material by key. </summary>
        public Material GetMaterial(string key)
        {
            if (!_materials.ContainsKey(key))
                _materials[key] = Resources.Load<Material>(key);

            return _materials[key];
        }

        #endregion
    }
}
