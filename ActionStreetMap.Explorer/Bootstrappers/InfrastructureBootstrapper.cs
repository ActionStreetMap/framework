using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Commands;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
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
            Container.Register(Component.For<IGameObjectFactory>().Use<GameObjectFactory>().Singleton());
            Container.Register(Component.For<IObjectPool>().Use<ObjectPool>().Singleton());

            // commands
            Container.Register(Component.For<CommandController>().Use<CommandController>().Singleton());
            Container.Register(Component.For<ICommand>().Use<SysCommand>().Singleton().Named("sys"));
            Container.Register(Component.For<ICommand>().Use<TagCommand>().Singleton().Named("tag"));
            Container.Register(Component.For<ICommand>().Use<LocateCommand>().Singleton().Named("locate"));
            Container.Register(Component.For<ICommand>().Use<GeocodeCommand>().Singleton().Named("geocode"));

            return true;
        }
    }
}
