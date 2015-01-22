using System;
using System.Diagnostics;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Tests
{
    public class DemoTileListener
    {
        private PerformanceLogger _logger = new PerformanceLogger();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public DemoTileListener(IMessageBus messageBus, PerformanceLogger logger)
        {
            _logger = logger;
            messageBus.AsObservable<TileFoundMessage>().Do(m => OnTileFound(m.Tile, m.Position)).Subscribe();

            messageBus.AsObservable<TileLoadStartMessage>().Do(m => OnTileBuildStarted(m.TileCenter)).Subscribe();
            messageBus.AsObservable<TileLoadFinishMessage>().Do(m => OnTileBuildFinished(m.Tile)).Subscribe();
            messageBus.AsObservable<TileActivateMessage>().Do(m => OnTileActivated(m.Tile)).Subscribe();
            messageBus.AsObservable<TileDeactivateMessage>().Do(m => OnTileDeactivated(m.Tile)).Subscribe();
            messageBus.AsObservable<TileDestroyMessage>().Do(m => OnTileDestroyed(m.Tile)).Subscribe();
        }

        private void OnTileDestroyed(Tile tile)
        {
            System.Console.WriteLine("Tile destroyed: center:{0}", tile.MapCenter);
        }

        public void OnTileFound(Tile tile, MapPoint position)
        {
        }

        public void OnTileBuildStarted(MapPoint center)
        {
            _stopwatch.Start();
            System.Console.WriteLine("Tile build begin: center:{0}", center);
        }

        public void OnTileBuildFinished(Tile tile)
        {
            _stopwatch.Stop();
            System.Console.WriteLine("Tile of size {0} is loaded in {1} ms", tile.Size, _stopwatch.ElapsedMilliseconds);
            _logger.Report("DemoTileListener.OnTileBuildFinished: before GC");
            GC.Collect();
            _logger.Report("DemoTileListener.OnTileBuildFinished: after GC");
            _stopwatch.Reset();
        }
        private void OnTileActivated(Tile tile)
        {
            System.Console.WriteLine("Tile activated: center:{0}", tile.MapCenter);
        }

        private void OnTileDeactivated(Tile tile)
        {
            System.Console.WriteLine("Tile deactivated: center:{0}", tile.MapCenter);
        }   
    }
}
