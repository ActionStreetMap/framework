using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Commands;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.ThickLine;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Interactions;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
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

            // Register object pool and all consumed types as it's necessary by its current implementation
            var objectPool = new ObjectPool()
                .RegisterObjectType<MeshTriangle>(() => new MeshTriangle(), 10240)
                .RegisterListType<MeshTriangle>(32)
                .RegisterObjectType<Clipper>(() => new Clipper(), 16)
                .RegisterObjectType<ClipperOffset>(() => new ClipperOffset(), 16)
                .RegisterListType<Tuple<Surface, Action<IMesh>>>(32)
                .RegisterListType<RoadElement>(32)
                .RegisterListType<Surface>(32)
                .RegisterListType<GeoCoordinate>(256)
                .RegisterListType<MapPoint>(256)
                .RegisterListType<LineElement>(32)
                .RegisterListType<int>(256);

            Container.RegisterInstance<IObjectPool>(objectPool);

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
