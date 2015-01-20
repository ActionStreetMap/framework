using System.Collections.Generic;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Scene.Builders;
using UnityEngine;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Tests.Explorer.Tiles.Stubs.ModelBuilders
{
    class TestBarrierModelBuilder: BarrierModelBuilder
    {
        [Dependency]
        public TestBarrierModelBuilder(IObjectPool objectPool)
            : base(objectPool)
        {

        }

        protected override void BuildObject(IGameObject gameObjectWrapper, Rule rule, 
            List<Vector3> p, List<int> t, List<Vector2> u)
        {
            // Do nothing
        }
    }
}
