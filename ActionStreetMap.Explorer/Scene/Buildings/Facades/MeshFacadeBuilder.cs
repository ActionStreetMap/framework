using System;
using ActionStreetMap.Core.Scene.Buildings;
using ActionStreetMap.Explorer.Geometry;

namespace ActionStreetMap.Explorer.Scene.Buildings.Facades
{
    internal class MeshFacadeBuilder: IFacadeBuilder
    {
        public string Name { get { return "mesh"; } }

        public MeshData Build(Building building, BuildingStyle style)
        {
            throw new NotImplementedException();
        }
    }
}
