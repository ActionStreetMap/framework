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
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Search;

namespace ActionStreetMap.Tests
{
    internal class Program
    {
        public static readonly GeoCoordinate StartGeoCoordinate = new GeoCoordinate(52.53192, 13.38736);

        public static readonly Container _container = new Container();

        private const string LogTag = "host";
        private readonly string _nmeaFilePath = TestHelper.TestNmeaFilePath;

        private readonly PerformanceLogger _logger = new PerformanceLogger();
        private IMessageBus _messageBus;
        private ITrace _trace;
        private DemoTileListener _tileListener;
        private IPositionObserver<GeoCoordinate> _geoPositionObserver;
        private IPositionObserver<MapPoint> _mapPositionObserver;

        private readonly ManualResetEvent _waitEvent = new ManualResetEvent(false);

        private static void Main(string[] args)
        {
            var program = new Program();
            program.RunGame();
            // program.PrintIndoorData();
            //program.DoContinuosMovements();
            //program.RunMocker();
            //program.Wait(); 

            Console.ReadKey();
        }

        public void PrintIndoorData()
        {
            var boundingBox = BoundingBox.Create(new GeoCoordinate(52.47910, 13.45432), 500);
            var search = _container.Resolve<ISearchEngine>();
            search.SearchByTag("indoor", "yes", boundingBox).Subscribe(e =>
            {
                Console.WriteLine(e);
            }, () =>
                Console.WriteLine("Search completed"));
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
                //.SetLocalMapData(@"g:\__ASM\__repository\_index\Index_Berlin_copy")
                .Build();
                
            var componentRoot = TestHelper.GetGameRunner(_container, config);

            _messageBus = _container.Resolve<IMessageBus>();
            _trace = _container.Resolve<ITrace>();
            _tileListener = new DemoTileListener(_messageBus, _logger);

            // start game on default position
            componentRoot.RunGame(StartGeoCoordinate);

            _geoPositionObserver = _container.Resolve<ITileController>();
            _mapPositionObserver = _container.Resolve<ITileController>();

            _messageBus.AsObservable<GeoPosition>().Do(position =>
            {
                _trace.Debug(LogTag, "GeoPosition: {0}", position.ToString());
                _geoPositionObserver.OnNext(position.Coordinate);
            }).Subscribe();

            _messageBus.AsObservable<MapPoint>().Do(position =>
            {
                _trace.Debug(LogTag, "MapPosition: {0}", position.ToString());
                _mapPositionObserver.OnNext(position);
            }).Subscribe();
        }

        public void Wait()
        {
            _waitEvent.WaitOne(TimeSpan.FromSeconds(60));
            _logger.Stop();
        }

        #region Performance analysis

        private void DoContinuosMovements()
        {
            _geoPositionObserver.OnNext(StartGeoCoordinate);
            float speed = 30; // meters per second
            float distance = 1000; // meters
            float angle = 30; // grads

            MoveTo(angle, speed, distance);
            MoveTo(360-angle, speed, distance);

            _trace.Debug(LogTag, "DoContinuosMovements: end");
        }

        private void MoveTo(float angle, float speed, float distance)
        {
            var steps = distance / speed;
            double angleInRad = angle * Math.PI / 180;
            for (int i = 0; i < steps; i++)
            {
                float xOffset = (float)(i * speed * Math.Cos(angleInRad));
                float yOffset = (float)(i * speed * Math.Sin(angleInRad));
                _messageBus.Send(new MapPoint(xOffset, yOffset));
                Thread.Sleep(1000);
            }
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