using System.Collections.Generic;
using ActionStreetMap.Core.Scene.World.Roads;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Models.Roads;
using ActionStreetMap.Models.Terrain;
using ActionStreetMap.Models.Utils;
using UnityEngine;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs.Builders
{
    public class TestTerrainBuilder: TerrainBuilder
    {
        [Dependency]
        public TestTerrainBuilder(IGameObjectFactory gameObjectFactory, IResourceProvider resourceProvider,
            IRoadGraphBuilder roadGraphBuilder, IRoadBuilder roadBuilder, IObjectPool objectPool)
            : base(gameObjectFactory, resourceProvider, roadGraphBuilder, roadBuilder, objectPool)
        {
        }

        protected override IGameObject CreateTerrainGameObject(IGameObject parent, TerrainSettings settings, 
            Vector3 size, List<int[,]> detailMapList)
        {
            return new TestGameObject();
        }
    }
}
