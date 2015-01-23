using ActionStreetMap.Core.Scene.Buildings;

namespace ActionStreetMap.Explorer.Scene.Buildings
{
    /// <summary>
    ///     Specifies buildings style provider logic.
    /// </summary>
    public interface IBuildingStyleProvider
    {
        /// <summary>
        ///     Gets building style for given building.
        /// </summary>
        /// <param name="building">Building.</param>
        /// <returns>Style.</returns>
        BuildingStyle Get(Building building);
    }
}