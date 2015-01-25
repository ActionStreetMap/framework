using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Positioning;
using ActionStreetMap.Core.Positioning.Nmea;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Index;
using ActionStreetMap.Maps.Index.Import;
using ActionStreetMap.Maps.Index.Spatial;
using ActionStreetMap.Maps.Index.Storage;

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
            var sw = new Stopwatch();
            sw.Start();
             program.RunGame();
            //program.RunMocker();
            //program.Wait();

            // program.DoContinuosMovements();

            /* program.CreateIndex(
                @"g:\__ASM\_other_projects\osmconvert\ile-de-france.o5m",
                @"g:\__ASM\__repository\framework\Tests\TestAssets\DemoResources\Config\themes\default\index.json",
                "Index");*/
            //program.ReadIndex("Index");
            //program.SubscribeOnMainThreadTest();

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

        private static void InvokeAndMeasure(Action action)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            action.Invoke();
            sw.Stop();
            Console.WriteLine("Action completed in {0}ms", sw.ElapsedMilliseconds);
        }

        #region Index experiments

        private void CreateIndex(string o5mFile, string settingsFile, string outputDirectory)
        {
            var builder = new IndexBuilder(new ConsoleTrace());
            builder.Configure(new ConfigSection(String.Format("{{\"index\":\"{0}\"}}", settingsFile.Replace("\\", "/"))));
            builder.Build(o5mFile, outputDirectory);
        }

        private void ReadIndex(string indexDirectory)
        {
            var logger = new PerformanceLogger();
            logger.Start();

            var usage = new KeyValueUsage(new FileStream(String.Format(Consts.KeyValueUsagePathFormat, indexDirectory), FileMode.Open));
            var tree = SpatialIndex<uint>.Load(new FileStream(String.Format(Consts.SpatialIndexPathFormat, indexDirectory), FileMode.Open));
            var index = KeyValueIndex.Load(new FileStream(String.Format(Consts.KeyValueIndexPathFormat, indexDirectory), FileMode.Open));
            var keyValueStore = new KeyValueStore(index, usage, new FileStream(String.Format(Consts.KeyValueStorePathFormat, indexDirectory), FileMode.Open));
            var store = new ElementStore(keyValueStore, new FileStream(String.Format(Consts.ElementStorePathFormat, indexDirectory), FileMode.Open));

            /*InvokeAndMeasure(() =>
            {
                var results = tree.Search(new Envelop(new GeoCoordinate(52.54, 13.346),
                        new GeoCoordinate(52.552, 13.354)));
                foreach (var result in results)
                {
                    var element = store.Get(result);
                    Console.WriteLine(element);
                }
            });*/

            logger.Stop();
        }

        #endregion

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

        #region Rx test

        public void SubscribeOnMainThreadTest()
        {
            Console.WriteLine("Main:{0}", Thread.CurrentThread.ManagedThreadId);
            var heavyMethod = Observable.Start(() =>
            {
                // heavy method...
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                Console.WriteLine("heavyMethod:{0}", Thread.CurrentThread.ManagedThreadId);
                return 10;
            });

            var heavyMethod2 = Observable.Start(() =>
            {
                // heavy method...
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
                Console.WriteLine("heavyMethod2:{0}", Thread.CurrentThread.ManagedThreadId);
                return 10;
            });

            // Join and await two other thread values
            Observable.WhenAll(heavyMethod, heavyMethod2)
                //.ObserveOnMainThread() // return to main thread
                .Subscribe(xs =>
                {
                    Console.WriteLine("Subscribe:{0}", Thread.CurrentThread.ManagedThreadId);
                });
            Console.ReadKey();
        }

        #endregion
    }
}