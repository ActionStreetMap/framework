using System.Collections.Generic;
using ActionStreetMap.Core.Scene.World.Roads;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Models.Roads;
using ActionStreetMap.Models.Utils;
using UnityEngine;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs.Builders
{
    public class TestRoadBuilder: RoadBuilder
    {
        [Dependency]
        public TestRoadBuilder(IResourceProvider resourceProvider, IObjectPool objectPool)
            : base(resourceProvider, objectPool)
        {

        }

        protected override void CreateRoadMesh(Road road, RoadStyle style, List<Vector3> points, List<int> triangles, List<Vector2> uv)
        {
        }

        protected override void CreateJunctionMesh(RoadJunction junction, RoadStyle style, int[] polygonTriangles)
        {
        }
    }
}
