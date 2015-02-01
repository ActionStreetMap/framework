using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.Tiling
{
    /// <summary> Defines logic for tile activation, deactivation and destroy. </summary>
    public interface ITileActivator
    {
        /// <summary> Activate given tile. It means show tile if it is hidden (deactivated) before. </summary>
        /// <param name="tile">Tile.</param>
        void Activate(Tile tile);

        /// <summary> Deactivate given tile. It means hide tile if it is shown (activated). </summary>
        /// <param name="tile">Tile</param>
        void Deactivate(Tile tile);

        /// <summary> Destroy tile and its childred. </summary>
        /// <param name="tile">Tile.</param>
        void Destroy(Tile tile);
    }
}
