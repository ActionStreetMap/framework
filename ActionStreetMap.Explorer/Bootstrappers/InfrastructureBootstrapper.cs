using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Scene.InDoor;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Commands;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Interactions;
using ActionStreetMap.Explorer.Scene.Terrain;
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
                .RegisterObjectType<TerrainMeshTriangle>(() => new TerrainMeshTriangle())
                .RegisterListType<TerrainMeshTriangle>(32)
                .RegisterObjectType<Clipper>(() => new Clipper())
                .RegisterObjectType<ClipperOffset>(() => new ClipperOffset())
                .RegisterListType<Tuple<Surface, Action<Mesh>>>(32)
                .RegisterListType<RoadElement>(32)
                .RegisterListType<Surface>(32)
                .RegisterListType<GeoCoordinate>(256)
                .RegisterListType<Point>(256)

                .RegisterListType<Vector2d>(32)
                .RegisterListType<LineSegment2d>(8)
                .RegisterListType<IntPoint>(32)
                .RegisterListType<Apartment>(8)

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
