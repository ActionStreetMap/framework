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
                        .Add("asphalt", new TextureGroup(4096, 4096, 2)
                            .Add(501, 1514, 500, 500)
                            .Add(1002, 1514, 500, 500))
                        .Add("background", new TextureGroup(4096, 4096, 1)
                            .Add(2541, 0, 500, 500))
                        .Add("bark", new TextureGroup(4096, 4096, 4)
                            .Add(2040, 712, 500, 500)
                            .Add(2541, 501, 500, 500)
                            .Add(3042, 0, 500, 500)
                            .Add(2004, 1213, 500, 500))
                        .Add("barrier", new TextureGroup(4096, 4096, 2)
                            .Add(3006, 2874, 500, 293)
                            .Add(3507, 2004, 500, 375))
                        .Add("brick", new TextureGroup(4096, 4096, 4)
                            .Add(3543, 0, 500, 500)
                            .Add(3042, 501, 500, 500)
                            .Add(2541, 1002, 500, 500)
                            .Add(2505, 1503, 500, 500))
                        .Add("canvas", new TextureGroup(4096, 4096, 1)
                            .Add(1503, 3749, 500, 333))
                        .Add("concrete", new TextureGroup(4096, 4096, 5)
                            .Add(3042, 1002, 500, 500)
                            .Add(3543, 501, 500, 500)
                            .Add(3006, 1503, 500, 500)
                            .Add(3543, 1002, 500, 500)
                            .Add(3507, 1503, 500, 500))
                        .Add("floor", new TextureGroup(4096, 4096, 6)
                            .Add(501, 513, 500, 1000)
                            .Add(1002, 513, 500, 1000)
                            .Add(1539, 0, 500, 1000)
                            .Add(0, 2014, 500, 500)
                            .Add(0, 2515, 500, 500)
                            .Add(501, 2015, 500, 500))
                        .Add("grass", new TextureGroup(4096, 4096, 5)
                            .Add(0, 3016, 500, 500)
                            .Add(1002, 2015, 500, 500)
                            .Add(501, 2516, 500, 500)
                            .Add(1503, 1745, 500, 500)
                            .Add(0, 3517, 500, 500))
                        .Add("ground", new TextureGroup(4096, 4096, 1)
                            .Add(1002, 2516, 500, 500))
                        .Add("metal", new TextureGroup(4096, 4096, 7)
                            .Add(501, 3017, 500, 500)
                            .Add(2004, 1714, 500, 500)
                            .Add(1503, 2246, 500, 500)
                            .Add(1002, 3017, 500, 500)
                            .Add(501, 3518, 500, 500)
                            .Add(2004, 2215, 500, 500)
                            .Add(1503, 2747, 500, 500))
                        .Add("panel", new TextureGroup(4096, 4096, 1)
                            .Add(2505, 2004, 500, 500))
                        .Add("road_brick", new TextureGroup(4096, 4096, 2)
                            .Add(1002, 3518, 500, 500)
                            .Add(2004, 2716, 500, 500))
                        .Add("roof_tiles", new TextureGroup(4096, 4096, 1)
                            .Add(2004, 3217, 500, 423))
                        .Add("sand", new TextureGroup(4096, 4096, 3)
                            .Add(2505, 3006, 500, 399)
                            .Add(1503, 3248, 500, 500)
                            .Add(2505, 2505, 500, 500))
                        .Add("stone", new TextureGroup(4096, 4096, 3)
                            .Add(0, 513, 500, 1500)
                            .Add(1503, 1001, 500, 743)
                            .Add(2040, 0, 500, 711))
                        .Add("tree", new TextureGroup(4096, 4096, 3)
                            .Add(0, 0, 512, 512)
                            .Add(513, 0, 512, 512)
                            .Add(1026, 0, 512, 512))
                        .Add("wood", new TextureGroup(4096, 4096, 3)
                            .Add(3006, 2505, 500, 368)
                            .Add(2004, 3641, 500, 348)
                            .Add(3006, 2004, 500, 500)));

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
