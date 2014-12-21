using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Positioning;
using ActionStreetMap.Core.Positioning.Nmea;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Osm.Formats;
using ActionStreetMap.Osm.Formats.O5m;
using ActionStreetMap.Osm.Index.Data;
using ActionStreetMap.Osm.Index.Search;

namespace ActionStreetMap.Tests
{
    internal class Program
    {
        private readonly GeoCoordinate _startGeoCoordinate = new GeoCoordinate(52.5499766666667, 13.350695);
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
            /*program.RunGame();
            program.RunMocker();
            program.Wait();*/

            program.ReadTextIndex();
           // program.CreateTextIndex();
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

        #region Index experiments

        private void CreateTextIndex()
        {
            var logger = new PerformanceLogger();
            logger.Start();

            var builder = new SearchIndexBuilder("Index");
            var reader = new O5mReader(new ReaderContext()
            {
                SourceStream = new FileStream(@"g:\__ASM\_other_projects\splitter\berlin2.o5m", FileMode.Open),
                Builder = builder,
                ReuseEntities = false,
                SkipTags = false,
            });

            reader.Parse();
            builder.Clear();
            builder.Complete();
            logger.Stop();

            InvokeAndMeasure(() =>
            {
                var pairs = builder._store.Search(new KeyValuePair<string, string>("addr:street", "Eichen"));
                foreach (var pair in pairs)
                    Console.WriteLine(pair);
            });
        }

        private void ReadTextIndex()
        {
            var buffer = new byte[256];
            var stream = new MemoryStream(buffer);
            stream.WriteByte(4);
            stream.WriteByte(7);

            var index = new KeyValueIndex(100, 3);
            var store = new KeyValueStore(index, stream);

            store.Insert(new KeyValuePair<string, string>("addr", "eic"));
            store.Insert(new KeyValuePair<string, string>("addr", "inv"));
            store.Insert(new KeyValuePair<string, string>("addr", "inc"));
            store.Insert(new KeyValuePair<string, string>("addr", "ina"));
            store.Insert(new KeyValuePair<string, string>("addr", "inb"));
            store.Insert(new KeyValuePair<string, string>("addr", "eif"));
            store.Insert(new KeyValuePair<string, string>("addr", "eic"));

            //eichendorffstraße
            //store.Insert(new KeyValuePair<string, string>("addr", "eichendorffstraße"));
            //store.Insert(new KeyValuePair<string, string>("addr", "eichendor"));

            var pairs = store.Search(new KeyValuePair<string, string>("addr", "eic"));
            foreach (var pair in pairs)
                Console.WriteLine(pair);

           
            stream.Flush();
            stream.Close();
        }

        private static void InvokeAndMeasure(Action action)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            action.Invoke();
            sw.Stop();
            Console.WriteLine("Action completed in {0}ms", sw.ElapsedMilliseconds);
        }

        #endregion
    }
}