using System.Collections.Generic;
using ActionStreetMap.Core.Scene.World.Roads;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Models.Roads;
using ActionStreetMap.Models.Utils;
using UnityEngine;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs.Builders
{
    public class TestRoadBuilder: RoadBuilder
    {
        [Dependency]
        public TestRoadBuilder(IResourceProvider resourceProvider) : base(resourceProvider)
        {

        }

        protected override void CreateRoadMesh(Road road, RoadStyle style, List<Vector3> points, List<int> triangles, List<Vector2> uv)
        {
        }

        protected override void CreateJunctionMesh(RoadJunction junction, RoadStyle style)
        {
        }
    }
}
