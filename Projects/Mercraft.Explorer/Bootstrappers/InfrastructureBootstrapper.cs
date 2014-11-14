﻿using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Explorer.Bootstrappers
{
    /// <summary>
    ///     Register infrastructure classes.
    /// </summary>
    public class InfrastructureBootstrapper: BootstrapperPlugin
    {
        /// <inheritdoc />
        public override string Name { get { return "infrastructure"; } }

        /// <inheritdoc />
        public override bool Run()
        {
            Container.Register(Component.For<ITrace>().Use<UnityConsoleTrace>().Singleton());
            Container.Register(Component.For<IGameObjectFactory>().Use<GameObjectFactory>().Singleton());
            Container.Register(Component.For<IObjectPool>().Use<ObjectPool>().Singleton());
            return true;
        }
    }
}
