using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Explorer
{
    /// <summary>
    ///     Represents entry point class for ASM logic.
    /// </summary>
    public interface IGameRunner
    {
        /// <summary>
        ///      Runs game with provided coordinate as map center.
        /// </summary>
        /// <param name="startCoordinate">Geo coordinate for (0,0) map point.</param>
        void RunGame(GeoCoordinate startCoordinate);
    }

    /// <summary>
    ///     Represents application component root.
    /// </summary>
    public class GameRunner : IGameRunner, IPositionObserver<MapPoint>, IPositionObserver<GeoCoordinate>
    {
        private readonly IContainer _container;
        private readonly IMessageBus _messageBus;
        private IPositionObserver<MapPoint> _mapPositionObserver;
        private IPositionObserver<GeoCoordinate> _geoPositionObserver;

        /// <summary>
        ///     Creates instance of <see cref="GameRunner"/>.
        /// </summary>
        /// <param name="container">DI container.</param>
        /// <param name="messageBus">Message bus.</param>
        public GameRunner(IContainer container, IMessageBus messageBus)
        {
            _container = container;
            _messageBus = messageBus;
        } 

        /// <inheritdoc />
        public void RunGame(GeoCoordinate coordinate)
        {
            // run bootstrappers
            _container.RegisterInstance(_messageBus);
            _container.Resolve<IBootstrapperService>().Run();

            // resolve actual position observers
            var tilePositionObserver = _container.Resolve<ITilePositionObserver>();
            _mapPositionObserver = tilePositionObserver;
            _geoPositionObserver = tilePositionObserver;
            // notify about geo coordinate change
            _geoPositionObserver.OnNext(coordinate);
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