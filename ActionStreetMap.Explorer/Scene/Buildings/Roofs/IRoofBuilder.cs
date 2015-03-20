using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Geometry;

namespace ActionStreetMap.Explorer.Scene.Buildings.Roofs
{
    /// <summary> Defines roof builder logic. </summary>
    public interface IRoofBuilder
    {
        /// <summary> Gets name of roof builder. </summary>
        string Name { get; }
        
        /// <summary> Checks whether this builder can build roof of given building. </summary>
        /// <param name="building">Building.</param>
        /// <returns>True if can build.</returns>
        bool CanBuild(Building building);

        /// <summary> Builds MeshData which contains information how to construct roof. </summary>
        /// <param name="building">Building.</param>
        /// <param name="style">Style.</param>
        /// <returns>MeshData.</returns>
        MeshData Build(Building building, BuildingStyle style);
    }
}
