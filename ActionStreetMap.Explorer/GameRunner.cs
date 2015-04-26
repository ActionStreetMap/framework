using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Bootstrappers;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Unity.IO;

namespace ActionStreetMap.Explorer
{
    /// <summary> Represents application component root. </summary>
    public sealed class GameRunner : IPositionObserver<MapPoint>, IPositionObserver<GeoCoordinate>
    {
        private const string LogTag = "runner";
        private readonly IContainer _container;
        private IMessageBus _messageBus;
        private IPositionObserver<MapPoint> _mapPositionObserver;
        private IPositionObserver<GeoCoordinate> _geoPositionObserver;

        private bool _isActive;

        /// <summary> Launched once game is launched. </summary>
        public event EventHandler GameStarted;

        /// <summary> 
        ///     Creates instance of <see cref="GameRunner"/>. <see cref="ITrace"/>, <see cref="IPathResolver"/> and
        ///     <see cref="IMessageBus"/> services should be already registered inside container.
        /// </summary>
        /// <param name="container">DI container.</param>
        /// <param name="rootConfigPath">Path of main configuration.</param>
        public GameRunner(IContainer container, string rootConfigPath)
        {
            _container = container;         
            ITrace trace = null;
            try
            {
                // NOTE these classes should be provided by client application.
                trace = _container.Resolve<ITrace>();
                var pathResolver = _container.Resolve<IPathResolver>();
                _messageBus = _container.Resolve<IMessageBus>();
                // read config
                var fileSystemService = new FileSystemService(pathResolver, trace);
                container.RegisterInstance(typeof (IFileSystemService), fileSystemService);
                container.RegisterInstance<IConfigSection>(new ConfigSection(rootConfigPath, fileSystemService));
            }
            catch (DependencyException depEx)
            {
                throw new ArgumentException(Strings.CannotRunGameWithoutPrerequesites, "container", depEx);
            }
            catch (Exception ex)
            {
                if (trace != null)
                    trace.Error(LogTag, ex, Strings.CannotReadMainConfig, rootConfigPath);
                throw;
            }

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
            if (_isActive) 
                throw new InvalidOperationException(Strings.CannotRegisterPluginForActiveGame);
            _container.Register(Component.For<IBootstrapperPlugin>().Use(typeof(T), args).Named(name).Singleton());
            return this;
        }

        /// <summary> Runs game. Do not call this method on UI thread to prevent its blocking. </summary>
        /// <param name="coordinate">GeoCoordinate for (0,0) map point. </param>
        public void RunGame(GeoCoordinate coordinate)
        {
            if(_isActive)
                throw new InvalidOperationException(Strings.CannotRunGameTwice);

            _isActive = true;

            // run bootstrappers
            _container.Resolve<IBootstrapperService>().Run();

            // resolve actual position observers
            var tilePositionObserver = _container.Resolve<ITilePositionObserver>();
            _mapPositionObserver = tilePositionObserver;
            _geoPositionObserver = tilePositionObserver;

            // notify about geo coordinate change
            _geoPositionObserver.OnNext(coordinate);

            GameStarted(this, new EventArgs());
        }

        #region IObserver<MapPoint> implementation

        MapPoint IPositionObserver<MapPoint>.Current { get { return _mapPositionObserver.Current; } }

        void IObserver<MapPoint>.OnNext(MapPoint value) { _mapPositionObserver.OnNext(value); }

        void IObserver<MapPoint>.OnError(Exception error) { _mapPositionObserver.OnError(error); }

        void IObserver<MapPoint>.OnCompleted() { _mapPositionObserver.OnCompleted(); }

        #endregion

        #region IObserver<GeoCoordinate> implementation

        GeoCoordinate IPositionObserver<GeoCoordinate>.Current { get { return _geoPositionObserver.Current; } }

        void IObserver<GeoCoordinate>.OnNext(GeoCoordinate value) { _geoPositionObserver.OnNext(value); }

        void IObserver<GeoCoordinate>.OnError(Exception error) { _geoPositionObserver.OnError(error); }

        void IObserver<GeoCoordinate>.OnCompleted() { _geoPositionObserver.OnCompleted(); }

        #endregion
    }
}