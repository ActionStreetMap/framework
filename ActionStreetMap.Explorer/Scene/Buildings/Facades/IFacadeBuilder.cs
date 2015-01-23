
using ActionStreetMap.Core.Scene.Buildings;

namespace ActionStreetMap.Explorer.Scene.Buildings.Facades
{
    /// <summary>
    ///     Defines facade builder logic.
    /// </summary>
    public interface IFacadeBuilder
    {
        /// <summary>
        ///     Name of facade builder.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Builds MeshData which contains information how to construct facade.
        /// </summary>
        /// <param name="building">Building.</param>
        /// <param name="style">Style.</param>
        /// <returns>MeshData.</returns>
        MeshData Build(Building building, BuildingStyle style);
    }
}
