using System;
using System.IO;
using System.Threading;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Positioning;
using ActionStreetMap.Core.Positioning.Nmea;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Tests
{
    internal class Program
    {
        private readonly GeoCoordinate _startGeoCoordinate = new GeoCoordinate(52.53176, 13.38702);
        private readonly string _nmeaFilePath = TestHelper.TestNmeaFilePath;

        private readonly Container _container = new Container();
        private readonly MessageBus _messageBus = new MessageBus();
        private readonly PerformanceLogger _logger = new PerformanceLogger();
        private readonly DemoTileListener _tileListener;
        private IPositionObserver<GeoCoordinate> _positionObserver;

        private readonly ManualResetEvent _waitEvent = new ManualResetEvent(false);

        public Program()
        {
            // NOTE not used directly but it subscribes to messages from message bus
            // and logs them to console
            _tileListener = new DemoTileListener(_messageBus, _logger);
        }

        private static void Main(string[] args)
        {
            var program = new Program();
            program.RunGame();
            //program.DoContinuosMovements();
            //program.RunMocker();
            //program.Wait(); 
            Console.ReadKey();
        }

        public void RunMocker()
        {
            Action<TimeSpan> delayAction = Thread.Sleep;
            using (Stream stream = new FileStream(_nmeaFilePath, FileMode.Open))
            {
                var mocker = new NmeaPositionMocker(stream, _messageBus);
                mocker.OnDone += (s, e) => _waitEvent.Set();
                mocker.Start(delayAction);
            }
        }

        public void RunGame()
        {
            _logger.Start();
            var componentRoot = TestHelper.GetGameRunner(_container, _messageBus);

            // start game on default position
            componentRoot.RunGame(_startGeoCoordinate);

            _positionObserver = _container.Resolve<ITilePositionObserver>();

            _messageBus.AsObservable<GeoPosition>().Do(position =>
            {
                Console.WriteLine("GeoPosition: {0}", position);
                _positionObserver.OnNext(position.Coordinate);
            }).Subscribe();
        }

        public void Wait()
        {
            _waitEvent.WaitOne(TimeSpan.FromSeconds(60));
            _logger.Stop();
        }

        #region Performance analysis

        public void DoContinuosMovements()
        {
            for (int i = 0; i < 15000; i++)
            {
                var newCoordinate = new GeoCoordinate(
                    _startGeoCoordinate.Latitude + 0.00001 * i,
                    _startGeoCoordinate.Longitude);
                _positionObserver.OnNext(newCoordinate);
            }

            for (int i = 15000; i >= 0; i--)
            {
                var newCoordinate = new GeoCoordinate(
                    _startGeoCoordinate.Latitude + 0.00001 * i,
                    _startGeoCoordinate.Longitude);
                _positionObserver.OnNext(newCoordinate);
            }
        }

        #endregion
    }
}