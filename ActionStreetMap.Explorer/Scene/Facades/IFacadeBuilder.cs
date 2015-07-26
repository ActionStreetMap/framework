using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Geometry;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    /// <summary> Defines facade builder logic. </summary>
    public interface IFacadeBuilder
    {
        /// <summary> Name of facade builder. </summary>
        string Name { get; }

        /// <summary> Builds MeshData which contains information how to construct facade. </summary>
        /// <param name="building">Building.</param>
        /// <returns>MeshData.</returns>
        List<MeshData> Build(Building building);
    }
}
