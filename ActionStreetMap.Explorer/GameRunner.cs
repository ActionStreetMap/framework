using System;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;

namespace ActionStreetMap.Explorer
{
    /// <summary>
    ///     Represents application component root.
    /// </summary>
    public class GameRunner : IGameRunner, IPositionListener
    {
        /// <summary>
        ///     DI container.
        /// </summary>
        private readonly IContainer _container;

        /// <summary>
        ///     Message bus.
        /// </summary>
        private readonly IMessageBus _messageBus;

        /// <summary>
        ///     Actual zone loader.
        /// </summary>
        private IPositionListener _positionListener;

        /// <inheritdoc />
        public GeoCoordinate CurrentPosition { get { return _positionListener.CurrentPosition; } }

        /// <inheritdoc />
        public MapPoint CurrentPoint { get { return _positionListener.CurrentPoint; } }

        /// <inheritdoc />
        public GeoCoordinate RelativeNullPoint
        {
            get { return _positionListener.RelativeNullPoint; }
            set { throw new InvalidOperationException(Strings.CannotChangeRelativeNullPoint); }
        }

        /// <summary>
        ///     Creates instance of <see cref="GameRunner"/>.
        /// </summary>
        /// <param name="container">DI container.</param>
        /// <param name="messageBus">Message bus.</param>
        public GameRunner(IContainer container, IMessageBus messageBus)
        {
            _container = container;
            _messageBus = messageBus;
            Initialize();
        } 

        private void Initialize()
        {
            // run bootstrappers
            _container.RegisterInstance(_messageBus);
            _container.Resolve<IBootstrapperService>().Run();
        }

        /// <inheritdoc />
        public void RunGame()
        {
            _positionListener = _container.Resolve<IPositionListener>();
            OnMapPositionChanged(new MapPoint(0, 0));
        }

        /// <inheritdoc />
        public void RunGame(GeoCoordinate coordinate)
        {
            _positionListener = _container.Resolve<IPositionListener>();
            _positionListener.RelativeNullPoint = coordinate;

            OnGeoPositionChanged(coordinate);
        }

        /// <inheritdoc />
        public void OnMapPositionChanged(MapPoint position)
        {
            _positionListener.OnMapPositionChanged(position);
        }

        /// <inheritdoc />
        public void OnGeoPositionChanged(GeoCoordinate position)
        {
            _positionListener.OnGeoPositionChanged(position);
        }
    }
}