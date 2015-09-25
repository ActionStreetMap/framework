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
                        .Add("background", new TextureGroup())
                        .Add("water", new TextureGroup())
                        .Add("road_car", new TextureGroup())
                        .Add("road_pedestrian", new TextureGroup())
                        .Add("brick", new TextureGroup())
                        .Add("bronze", new TextureGroup())
                        .Add("canvas", new TextureGroup())
                        .Add("concrete", new TextureGroup())
                        .Add("copper", new TextureGroup())
                        .Add("glass", new TextureGroup())
                        .Add("gold", new TextureGroup())
                        .Add("plants", new TextureGroup())
                        .Add("metal", new TextureGroup())
                        .Add("panel", new TextureGroup())
                        .Add("plaster", new TextureGroup())
                        .Add("roof_tiles", new TextureGroup())
                        .Add("silver", new TextureGroup())
                        .Add("slate", new TextureGroup())
                        .Add("stone", new TextureGroup())
                        .Add("tar_paper", new TextureGroup())
                        .Add("wood", new TextureGroup())
                        .Add("ground", new TextureGroup())
                        .Add("grass", new TextureGroup())
                        .Add("asphalt", new TextureGroup())
                        .Add("sand", new TextureGroup())
                        .Add("barrier", new TextureGroup()));
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
