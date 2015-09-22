using ActionStreetMap.Core.Tiling;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Explorer.Bootstrappers;
using ActionStreetMap.Explorer.Customization;
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

            CustomizationService
                .RegisterBehaviour("terrain_draw", typeof (TestModelBehaviour))
                .RegisterAtlas("main", new TextureAtlas()
                    .Add("pattern", new TextureGroup(100, 100)
                        .Add(50, 50, 10, 10)
                        .Add(10, 10, 5, 5)));

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
