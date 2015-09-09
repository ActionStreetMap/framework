using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Maintains list of global behaviours. </summary>
    public sealed class ModelExtensionProvider
    {
        private readonly Object _lockObj = new object();

        private IContainer _container;
        private readonly Dictionary<string, Type> _modelBehaviours = new Dictionary<string, Type>(4);
        private Dictionary<string, IModelBuilder> _modelBuilders;

        /// <summary> Creates instance of <see cref="ModelExtensionProvider"/>. </summary>
        internal ModelExtensionProvider(IContainer container)
        {
            _container = container;
        }

        #region Registration

        /// <summary> Registers model behaviour type. </summary>
        public ModelExtensionProvider RegisterBehaviour(string name, Type modelBehaviourType)
        {
            Guard.IsAssignableFrom(typeof (IModelBehaviour), modelBehaviourType);

            _modelBehaviours.Add(name, modelBehaviourType);
            return this;
        }

        /// <summary> Registers model builder type. </summary>
        public ModelExtensionProvider RegisterBuilder(string name, Type modelBuilderType)
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
        public ModelExtensionProvider RegisterBuilder(IModelBuilder builder)
        {
            _container.RegisterInstance(builder, builder.Name);
            return this;
        }

        #endregion

        /// <summary> Gets behaviour type by its name. </summary>
        public Type GetBehaviour(string name)
        {
            return _modelBehaviours[name];
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
                        _modelBuilders = new Dictionary<string, IModelBuilder>(8);
                        foreach (var modelBuilder in _container.ResolveAll<IModelBuilder>())
                            _modelBuilders.Add(modelBuilder.Name, modelBuilder);
                        _container = null;
                    }
                }
            }
            return _modelBuilders[name];
        }
    }
}
