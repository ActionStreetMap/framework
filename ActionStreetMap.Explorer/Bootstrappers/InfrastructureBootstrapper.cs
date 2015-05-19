using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Commands;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Interactions;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Unity.IO;
using ActionStreetMap.Unity.Utils;

namespace ActionStreetMap.Explorer.Bootstrappers
{
    /// <summary> Register infrastructure classes. </summary>
    public class InfrastructureBootstrapper: BootstrapperPlugin
    {
        /// <inheritdoc />
        public override string Name { get { return "infrastructure"; } }

        /// <inheritdoc />
        public override bool Run()
        {
            // NOTE dummy services, should be overriden by actual application
            Container.Register(Component.For<ITrace>().Use<DefaultTrace>().Singleton());
            Container.Register(Component.For<IPathResolver>().Use<DefaultPathResolver>().Singleton());

            Container.Register(Component.For<IGameObjectFactory>().Use<GameObjectFactory>().Singleton());
            Container.Register(Component.For<IObjectPool>().Use<ObjectPool>().Singleton());

            // Commands
            Container.Register(Component.For<CommandController>().Use<CommandController>().Singleton());
            Container.Register(Component.For<ICommand>().Use<SysCommand>().Singleton().Named("sys"));
            Container.Register(Component.For<ICommand>().Use<SearchCommand>().Singleton().Named("search"));
            Container.Register(Component.For<ICommand>().Use<LocateCommand>().Singleton().Named("locate"));
            Container.Register(Component.For<ICommand>().Use<GeocodeCommand>().Singleton().Named("geocode"));

            Container.Register(Component.For<IModelBehaviour>().Use<DummyBehaviour>().Singleton().Named("dummy"));

            // Override throw instruction (default in UnityMainThreadDispathcer should call this method as well)
            ActionStreetMap.Infrastructure.Reactive.Stubs.Throw = exception =>
            {
                Trace.Error("FATAL", exception, "Unhandled exception is thrown!");
                throw exception;
            };

            return true;
        }
    }
}
