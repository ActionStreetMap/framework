using System;
using System.Diagnostics;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Polygons;
using ActionStreetMap.Core.Terrain;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Tests.Expiremental;

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
            messageBus.AsObservable<TileDestroyMessage>().Do(m => OnTileDestroyed(m.Tile)).Subscribe();
        }

        private void OnTileDestroyed(Tile tile)
        {
            Console.WriteLine("Tile destroyed: center:{0}", tile.MapCenter);
        }

        public void OnTileFound(Tile tile, MapPoint position)
        {
            //Console.WriteLine("Tile {0} found for {1}", tile.MapCenter, position);
        }

        public void OnTileBuildStarted(MapPoint center)
        {
            _stopwatch.Start();
            Console.WriteLine("Tile build begin: center:{0}", center);
        }

        public void OnTileBuildFinished(Tile tile)
        {
            _stopwatch.Stop();
            Console.WriteLine("Tile build end: {0}x{1} size is loaded in {2} ms", tile.Width, tile.Height, _stopwatch.ElapsedMilliseconds);
            _logger.Report("DemoTileListener.OnTileBuildFinished: before GC");
            GC.Collect();
            _logger.Report("DemoTileListener.OnTileBuildFinished: after GC");
            _stopwatch.Reset();
            
            VisualizeTile(tile);
        }

        private void VisualizeTile(Tile tile)
        {
            var gridBuilder = new MeshGridBuilder(new ConsoleTrace());
            var cells = gridBuilder.Build(tile);

            var rowCount = cells.GetLength(0);
            var columnCount = cells.GetLength(1);
            var scale = 10000;
            for (int j = 0; j < rowCount; j++)
                for (int i = 0; i < columnCount; i++)
                {
                    var cell = cells[i, j];
                    var polygon = cell.Roads.Contours.Select(c => c.Select(v => new IntPoint(v.X*scale, v.Y*scale)).ToList()).ToList();
                    SVGBuilder.SaveToFile(polygon, String.Format("cell_{0}_{1}.svg", i, j), 0.005);
                }
        }
    }
}
