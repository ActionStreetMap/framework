using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Core.Tiling
{
    /// <summary> Defines behavior of tile loader. </summary>
    public interface ITileLoader
    {
        /// <summary> Loads given tile. This method triggers real loading and processing osm data. </summary>
        /// <param name="tile">Tile.</param>
        IObservable<Unit> Load(Tile tile);
    }
}
