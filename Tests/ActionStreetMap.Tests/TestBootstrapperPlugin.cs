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
                            .Add(501, 2082, 500, 500)
                            .Add(1002, 2082, 500, 500))
                        .Add("background", new TextureGroup(4096, 4096, 1)
                            .Add(2541, 3596, 500, 500))
                        .Add("bark", new TextureGroup(4096, 4096, 4)
                            .Add(2040, 2884, 500, 500)
                            .Add(2541, 3095, 500, 500)
                            .Add(3042, 3596, 500, 500)
                            .Add(2004, 2383, 500, 500))
                        .Add("barrier", new TextureGroup(4096, 4096, 2)
                            .Add(3006, 929, 500, 293)
                            .Add(3507, 1717, 500, 375))
                        .Add("brick", new TextureGroup(4096, 4096, 4)
                            .Add(3543, 3596, 500, 500)
                            .Add(3042, 3095, 500, 500)
                            .Add(2541, 2594, 500, 500)
                            .Add(2505, 2093, 500, 500))
                        .Add("canvas", new TextureGroup(4096, 4096, 1)
                            .Add(1503, 14, 500, 333))
                        .Add("concrete", new TextureGroup(4096, 4096, 5)
                            .Add(3042, 2594, 500, 500)
                            .Add(3543, 3095, 500, 500)
                            .Add(3006, 2093, 500, 500)
                            .Add(3543, 2594, 500, 500)
                            .Add(3507, 2093, 500, 500))
                        .Add("floor", new TextureGroup(4096, 4096, 6)
                            .Add(501, 2583, 500, 1000)
                            .Add(1002, 2583, 500, 1000)
                            .Add(1539, 3096, 500, 1000)
                            .Add(0, 1582, 500, 500)
                            .Add(0, 1081, 500, 500)
                            .Add(501, 1581, 500, 500))
                        .Add("grass", new TextureGroup(4096, 4096, 5)
                            .Add(0, 580, 500, 500)
                            .Add(1002, 1581, 500, 500)
                            .Add(501, 1080, 500, 500)
                            .Add(1503, 1851, 500, 500)
                            .Add(0, 79, 500, 500))
                        .Add("ground", new TextureGroup(4096, 4096, 1)
                            .Add(1002, 1080, 500, 500))
                        .Add("metal", new TextureGroup(4096, 4096, 7)
                            .Add(501, 579, 500, 500)
                            .Add(2004, 1882, 500, 500)
                            .Add(1503, 1350, 500, 500)
                            .Add(1002, 579, 500, 500)
                            .Add(501, 78, 500, 500)
                            .Add(2004, 1381, 500, 500)
                            .Add(1503, 849, 500, 500))
                        .Add("panel", new TextureGroup(4096, 4096, 1)
                            .Add(2505, 1592, 500, 500))
                        .Add("road_brick", new TextureGroup(4096, 4096, 2)
                            .Add(1002, 78, 500, 500)
                            .Add(2004, 880, 500, 500))
                        .Add("roof_tiles", new TextureGroup(4096, 4096, 1)
                            .Add(2004, 456, 500, 423))
                        .Add("sand", new TextureGroup(4096, 4096, 3)
                            .Add(2505, 691, 500, 399)
                            .Add(1503, 348, 500, 500)
                            .Add(2505, 1091, 500, 500))
                        .Add("stone", new TextureGroup(4096, 4096, 3)
                            .Add(0, 2083, 500, 1500)
                            .Add(1503, 2352, 500, 743)
                            .Add(2040, 3385, 500, 711))
                        .Add("tree", new TextureGroup(4096, 4096, 3)
                            .Add(0, 3584, 512, 512)
                            .Add(513, 3584, 512, 512)
                            .Add(1026, 3584, 512, 512))
                        .Add("wood", new TextureGroup(4096, 4096, 3)
                            .Add(3006, 1223, 500, 368)
                            .Add(2004, 107, 500, 348)
                            .Add(3006, 1592, 500, 500)));

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
