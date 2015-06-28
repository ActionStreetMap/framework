using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Tests
{
    /// <summary> This plugin overrides registration of non-testable classes. </summary>
    public class TestBootstrapperPlugin: BootstrapperPlugin
    {
        public override string Name { get { return "test"; } }

        public override bool Run()
        {
            Scheduler.MainThread = new TestScheduler();

            Container.RegisterInstance(new BehaviourProvider()
                .Register("terrain_modify", typeof(TestModelBehaviour))
                .Register("terrain_draw", typeof(TestModelBehaviour))
                .Register("building_modify_facade", typeof(TestModelBehaviour))
                .Register("mesh_destroy", typeof(TestModelBehaviour)));

            return true;
        }

        /// <summary> Dummy model behavior. </summary>
        private class TestModelBehaviour : IModelBehaviour
        {
            public string Name { get; private set; }
            public TestModelBehaviour(string name) { Name = name; }
            public void Apply(IGameObject gameObject, Model model) { }
        }
    }
}
