using System;
using System.Collections.Generic;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Infrastructure
{
    /// <summary> Maintains list of global behaviours. </summary>
    public sealed class BehaviourProvider
    {
        private readonly IContainer _container;
        private readonly Dictionary<string, Type> _modelBehaviours = new Dictionary<string, Type>(4);

        /// <summary> Creates instance of <see cref="BehaviourProvider"/>. </summary>
        internal BehaviourProvider(IContainer container)
        {
            _container = container;
        }

        /// <summary> Registers model behaviour type. </summary>
        public BehaviourProvider RegisterBehaviour(string name, Type modelBehaviourType)
        {
            Guard.IsAssignableFrom(typeof(IModelBehaviour), modelBehaviourType);

            _modelBehaviours.Add(name, modelBehaviourType);
            return this;
        }

        /// <summary> Registers model builder type. </summary>
        public BehaviourProvider RegisterBuilder(string name, Type modelBuilderType)
        {
            Guard.IsAssignableFrom(typeof(IModelBuilder), modelBuilderType);

            _container.Register(Component
                .For<IModelBuilder>()
                .Use(modelBuilderType)
                .Named(name)
                .Singleton());
            return this;
        }

        /// <summary> Registers model builder instance. </summary>
        public BehaviourProvider RegisterBuilder(IModelBuilder builder)
        {
            _container.RegisterInstance(builder, builder.Name);
            return this;
        }

        /// <summary> Gets behaviour type by its name. </summary>
        public Type GetBehaviour(string name)
        {
            return _modelBehaviours[name];
        }

        /// <summary> Gets model builder by its name. </summary>
        public IModelBuilder GetBuilder(string name)
        {
            return _container.Resolve<IModelBuilder>(name);
        }
    }
}
