using ActionStreetMap.Core.Scene.Roads;

namespace ActionStreetMap.Models.Roads
{
    /// <summary>
    ///     Defines road style provider logic.
    /// </summary>
    public interface IRoadStyleProvider
    {
        /// <summary>
        ///     Gets road style for given road.
        /// </summary>
        /// <param name="road">Road.</param>
        /// <returns>Road style.</returns>
        RoadStyle Get(Road road);

        /// <summary>
        ///     Gets road style for given road junction.
        /// </summary>
        /// <param name="junction">Road junction.</param>
        /// <returns>Road style.</returns>
        RoadStyle Get(RoadJunction junction);
    }
}