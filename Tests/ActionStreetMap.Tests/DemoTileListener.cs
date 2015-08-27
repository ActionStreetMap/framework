using System;
using System.Diagnostics;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Scene.Terrain;
using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Tests.Expiremental;
using Path = System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ActionStreetMap.Core.Geometry.Clipping.IntPoint>>;

namespace ActionStreetMap.Tests
{
    public class DemoTileListener
    {
        private PerformanceLogger _logger = new PerformanceLogger();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public DemoTileListener(IMessageBus messageBus, PerformanceLogger logger)
        {
            _logger = logger;
            messageBus.AsObservable<TileLoadStartMessage>().Do(m => OnTileBuildStarted(m.TileCenter)).Subscribe();
            messageBus.AsObservable<TileLoadFinishMessage>().Do(m => OnTileBuildFinished(m.Tile)).Subscribe();
            messageBus.AsObservable<TileDestroyMessage>().Do(m => OnTileDestroyed(m.Tile)).Subscribe();
        }

        private void OnTileDestroyed(Tile tile)
        {
            Console.WriteLine("Tile destroyed: center:{0}", tile.MapCenter);
        }

        public void OnTileBuildStarted(Vector2d center)
        {
            _stopwatch.Start();
            Console.WriteLine("Tile build begin: center:{0}", center);
        }

        public void OnTileBuildFinished(Tile tile)
        {
            _stopwatch.Stop();
            Console.WriteLine("Tile build end: {0}x{1} size is loaded in {2} ms", 
                tile.Rectangle.Width, tile.Rectangle.Height, _stopwatch.ElapsedMilliseconds);
            _logger.Report("DemoTileListener.OnTileBuildFinished: before GC");
            GC.Collect();
            _logger.Report("DemoTileListener.OnTileBuildFinished: after GC");
            _stopwatch.Reset();

            ProcessTile(tile);
        }

        private void ProcessTile(Tile tile)
        {
            /*var clipper = new Clipper();
            clipper.AddPaths(tile.Canvas.Water
               .Select(a => a.Points.Select(p => new IntPoint(p.X * MeshCellBuilder.Scale, p.Y * MeshCellBuilder.Scale)).ToList()).ToList(),
               PolyType.ptSubject, true);
            var solution = new Paths();
            clipper.Execute(ClipType.ctUnion, solution);
            clipper.Clear();

            SVGBuilder.SaveToFile(solution, "water.svg", 0.001);*/
        }
    }
}
;