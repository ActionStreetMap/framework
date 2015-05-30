using System;
using System.IO;
using System.Threading;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Positioning;
using ActionStreetMap.Core.Positioning.Nmea;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Import;
using ActionStreetMap.Unity.IO;

namespace ActionStreetMap.Tests
{
    internal class Program
    {
        public static readonly GeoCoordinate StartGeoCoordinate = new GeoCoordinate(52.53152, 13.38708);
        public static readonly Container _container = new Container();

        private const string LogTag = "host";
        private readonly string _nmeaFilePath = TestHelper.TestNmeaFilePath;

        private readonly PerformanceLogger _logger = new PerformanceLogger();
        private IMessageBus _messageBus;
        private ITrace _trace;
        private DemoTileListener _tileListener;
        private IPositionObserver<GeoCoordinate> _positionObserver;

        private readonly ManualResetEvent _waitEvent = new ManualResetEvent(false);

        private static void Main(string[] args)
        {
            var program = new Program();
             program.RunGame();
            // program.DoContinuosMovements();
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

            var config = ConfigBuilder.GetDefault()
                .Build();
                
            var componentRoot = TestHelper.GetGameRunner(_container, config);

            _messageBus = _container.Resolve<IMessageBus>();
            _trace = _container.Resolve<ITrace>();
            _tileListener = new DemoTileListener(_messageBus, _logger);

            // start game on default position
            componentRoot.RunGame(StartGeoCoordinate);

            _positionObserver = _container.Resolve<ITileController>();

            _messageBus.AsObservable<GeoPosition>().Do(position =>
            {
                _trace.Debug(LogTag, "GeoPosition: {0}", position);
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
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < 15000; i++)
                {
                    var newCoordinate = new GeoCoordinate(
                        StartGeoCoordinate.Latitude + 0.00001*i,
                        StartGeoCoordinate.Longitude);
                    _positionObserver.OnNext(newCoordinate);
                }

                for (int i = 15000; i >= 0; i--)
                {
                    var newCoordinate = new GeoCoordinate(
                        StartGeoCoordinate.Latitude + 0.00001*i,
                        StartGeoCoordinate.Longitude);
                    _positionObserver.OnNext(newCoordinate);
                }
            }

            _trace.Debug(LogTag, "DoContinuosMovements: end");
        }

        #endregion

        #region Index building

        private void BuildIndex(string filePath, string outputDirectory)
        {
            // populate container with services.
            TestHelper.GetGameRunner(_container);
            _container.Resolve<MapIndexUtility>()
                .BuildIndex(filePath, outputDirectory);
        }

        #endregion
    }
}