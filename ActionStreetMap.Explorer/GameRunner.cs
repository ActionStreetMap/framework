using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Bootstrappers;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Unity.IO;

namespace ActionStreetMap.Explorer
{
    /// <summary> Represents application component root. Not thread safe. </summary>
    public sealed class GameRunner : IPositionObserver<Vector2d>, IPositionObserver<GeoCoordinate>
    {
        private readonly IContainer _container;

        private IPositionObserver<Vector2d> _mapPositionObserver;
        private IPositionObserver<GeoCoordinate> _geoPositionObserver;

        private bool _isInitialized;

        /// <summary> Creates instance of <see cref="GameRunner"/>. </summary>
        /// <param name="container">DI container.</param>
        /// <param name="config">Configuration.</param>
        public GameRunner(IContainer container, IConfigSection config)
        {
            _container = container;
            _container.RegisterInstance<IConfigSection>(config);

            // register bootstrappers
            _container.Register(Component.For<IBootstrapperService>().Use<BootstrapperService>());
            _container.Register(Component.For<IBootstrapperPlugin>().Use<InfrastructureBootstrapper>().Named("infrastructure"));
            _container.Register(Component.For<IBootstrapperPlugin>().Use<TileBootstrapper>().Named("tile"));
            _container.Register(Component.For<IBootstrapperPlugin>().Use<SceneBootstrapper>().Named("scene"));
        }

        /// <summary> Registers specific bootstrapper plugin. </summary>
        /// <returns> Current GameRunner.</returns>
        public GameRunner RegisterPlugin<T>(string name, params object[] args) where T: IBootstrapperPlugin
        {
            if (_isInitialized) 
                throw new InvalidOperationException(Strings.CannotRegisterPluginForCompletedBootstrapping);
            _container.Register(Component.For<IBootstrapperPlugin>().Use(typeof(T), args).Named(name).Singleton());
            return this;
        }

        /// <summary> Runs game. </summary>
        /// <remarks> Do not call this method on UI thread to prevent its blocking. </remarks>
        /// <param name="coordinate">GeoCoordinate for (0,0) map point. </param>
        public void RunGame(GeoCoordinate coordinate)
        {
            var messageBus = _container.Resolve<IMessageBus>();
            // resolve actual position observers
            var tilePositionObserver = _container.Resolve<ITileController>();
            _mapPositionObserver = tilePositionObserver;
            _geoPositionObserver = tilePositionObserver;

            // notify about geo coordinate change
            _geoPositionObserver.OnNext(coordinate);

            messageBus.Send(new GameStartedMessage(tilePositionObserver.CurrentTile));
        }

        /// <summary> Runs bootstrapping process. </summary>
        public GameRunner Bootstrap()
        {
            if (_isInitialized) 
                return this;

            _isInitialized = true;

            // run bootstrappers
            _container.Resolve<IBootstrapperService>().Run();
            
            return this;
        }

        #region IObserver<MapPoint> implementation

        Vector2d IPositionObserver<Vector2d>.CurrentPosition { get { return _mapPositionObserver.CurrentPosition; } }

        void IObserver<Vector2d>.OnNext(Vector2d value) { _mapPositionObserver.OnNext(value); }

        void IObserver<Vector2d>.OnError(Exception error) { _mapPositionObserver.OnError(error); }

        void IObserver<Vector2d>.OnCompleted() { _mapPositionObserver.OnCompleted(); }

        #endregion

        #region IObserver<GeoCoordinate> implementation

        GeoCoordinate IPositionObserver<GeoCoordinate>.CurrentPosition { get { return _geoPositionObserver.CurrentPosition; } }

        void IObserver<GeoCoordinate>.OnNext(GeoCoordinate value) { _geoPositionObserver.OnNext(value); }

        void IObserver<GeoCoordinate>.OnError(Exception error) { _geoPositionObserver.OnError(error); }

        void IObserver<GeoCoordinate>.OnCompleted() { _geoPositionObserver.OnCompleted(); }

        #endregion

        /// <summary> This message is sent once RunGame is completed.  </summary>
        public class GameStartedMessage
        {
            /// <summary> Current active tile. </summary>
            public readonly Tile Tile;

            /// <summary> Creates instabce of <see cref="GameStartedMessage"/>. </summary>
            internal GameStartedMessage(Tile tile)
            {
                Tile = tile;
            }
        }
    }
}