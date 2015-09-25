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
                .RegisterAtlas("main",
                    new TextureAtlas()
                        .Add("background", new TextureGroup(1, 1))
                        .Add("water", new TextureGroup(1, 1))
                        .Add("road_car", new TextureGroup(1, 1))
                        .Add("road_pedestrian", new TextureGroup(1, 1))
                        .Add("brick", new TextureGroup(1, 1))
                        .Add("bronze", new TextureGroup(1, 1))
                        .Add("canvas", new TextureGroup(1, 1))
                        .Add("concrete", new TextureGroup(1, 1))
                        .Add("copper", new TextureGroup(1, 1))
                        .Add("glass", new TextureGroup(1, 1))
                        .Add("gold", new TextureGroup(1, 1))
                        .Add("plants", new TextureGroup(1, 1))
                        .Add("metal", new TextureGroup(1, 1))
                        .Add("panel", new TextureGroup(1, 1))
                        .Add("plaster", new TextureGroup(1, 1))
                        .Add("roof_tiles", new TextureGroup(1, 1))
                        .Add("silver", new TextureGroup(1, 1))
                        .Add("slate", new TextureGroup(1, 1))
                        .Add("stone", new TextureGroup(1, 1))
                        .Add("tar_paper", new TextureGroup(1, 1))
                        .Add("wood", new TextureGroup(1, 1))
                        .Add("ground", new TextureGroup(1, 1))
                        .Add("grass", new TextureGroup(1, 1))
                        .Add("asphalt", new TextureGroup(1, 1))
                        .Add("sand", new TextureGroup(1, 1))
                        .Add("barrier", new TextureGroup(1, 1)));
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
