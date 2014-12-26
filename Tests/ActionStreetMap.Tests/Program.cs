using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Positioning;
using ActionStreetMap.Core.Positioning.Nmea;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Osm.Formats;
using ActionStreetMap.Osm.Formats.O5m;
using ActionStreetMap.Osm.Index;
using ActionStreetMap.Osm.Index.Import;
using ActionStreetMap.Osm.Index.Spatial;
using ActionStreetMap.Osm.Index.Storage;

namespace ActionStreetMap.Tests
{
    internal class Program
    {
        private readonly GeoCoordinate _startGeoCoordinate = new GeoCoordinate(55.7537315, 37.6198537);
        private readonly string _nmeaFilePath = TestHelper.TestNmeaFilePath;

        private readonly Container _container = new Container();
        private readonly MessageBus _messageBus = new MessageBus();
        private readonly PerformanceLogger _logger = new PerformanceLogger();
        private readonly DemoTileListener _tileListener;
        private IPositionListener _positionListener;

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
            //program.RunGame();
            //program.RunMocker();
            //program.Wait();

            //program.CreateIndex();
             program.ReadIndex();
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

            _positionListener = _container.Resolve<IPositionListener>();

            _messageBus.AsObservable<GeoPosition>().Do(position =>
            {
                Console.WriteLine("GeoPosition: {0}", position);
                _positionListener.OnGeoPositionChanged(position.Coordinate);
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

        private const string Directory = "Index_test";

        private void CreateIndex()
        {
            var builder = new IndexBuilder();
            builder.Trace = new ConsoleTrace();
            builder.Configure(new ConfigSection("{\"index\":\"g:/__ASM/__repository/framework/Tests/TestAssets/DemoResources/Config/themes/default/index.json\"}"));
            builder.Build(@"g:\__ASM\_other_projects\osmconvert\moscow.o5m", Directory);
        }

        private void ReadIndex()
        {
            var logger = new PerformanceLogger();
            logger.Start();

            var usage = new KeyValueUsage(new FileStream(String.Format(Consts.KeyValueUsagePathFormat, Directory),FileMode.Open));
            var tree = SpatialIndex<uint>.Load(new FileStream(String.Format(Consts.SpatialIndexPathFormat, Directory), FileMode.Open));
            var index = KeyValueIndex.Load(new FileStream(String.Format(Consts.KeyValueIndexPathFormat, Directory), FileMode.Open));
            var keyValueStore = new KeyValueStore(index, usage, new FileStream(String.Format(Consts.KeyValueStorePathFormat, Directory), FileMode.Open));
            var store = new ElementStore(keyValueStore, new FileStream(String.Format(Consts.ElementStorePathFormat, Directory), FileMode.Open));

            InvokeAndMeasure(() =>
            {
                var results = tree.Search(new Envelop(new GeoCoordinate(52.5281163, 13.3848696),
                        new GeoCoordinate(52.5357719, 13.3896976)));
                foreach (var result in results)
                {
                    var element = store.Get(result);
                    Console.WriteLine(element);
                }
            });

            logger.Stop();
        }

        #endregion
    }
}