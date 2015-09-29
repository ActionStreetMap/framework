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
                            .Add(505, 2074, 500, 500)
                            .Add(1010, 2074, 500, 500))
                        .Add("background", new TextureGroup(4096, 4096, 1)
                            .Add(2020, 2848, 500, 500))
                        .Add("bark", new TextureGroup(4096, 4096, 4)
                            .Add(3030, 3596, 500, 500)
                            .Add(1515, 2074, 500, 500)
                            .Add(2525, 2880, 500, 500)
                            .Add(2020, 2343, 500, 500))
                        .Add("barrier", new TextureGroup(4096, 4096, 2)
                            .Add(3030, 874, 500, 293)
                            .Add(3535, 1701, 500, 375))
                        .Add("brick", new TextureGroup(4096, 4096, 4)
                            .Add(3030, 3091, 500, 500)
                            .Add(3535, 3596, 500, 500)
                            .Add(2525, 2375, 500, 500)
                            .Add(3535, 3091, 500, 500))
                        .Add("canvas", new TextureGroup(4096, 4096, 1)
                            .Add(3535, 1363, 500, 333))
                        .Add("concrete", new TextureGroup(4096, 4096, 5)
                            .Add(3030, 2586, 500, 500)
                            .Add(3030, 2081, 500, 500)
                            .Add(3535, 2586, 500, 500)
                            .Add(3535, 2081, 500, 500)
                            .Add(0, 1574, 500, 500))
                        .Add("floor", new TextureGroup(4096, 4096, 6)
                            .Add(505, 2579, 500, 1000)
                            .Add(1010, 2579, 500, 1000)
                            .Add(1515, 2579, 500, 1000)
                            .Add(0, 1069, 500, 500)
                            .Add(505, 1569, 500, 500)
                            .Add(0, 564, 500, 500))
                        .Add("grass", new TextureGroup(4096, 4096, 5)
                            .Add(1010, 1569, 500, 500)
                            .Add(505, 1064, 500, 500)
                            .Add(0, 59, 500, 500)
                            .Add(1515, 1569, 500, 500)
                            .Add(1010, 1064, 500, 500))
                        .Add("ground", new TextureGroup(4096, 4096, 1)
                            .Add(505, 559, 500, 500))
                        .Add("metal", new TextureGroup(4096, 4096, 7)
                            .Add(2020, 1838, 500, 500)
                            .Add(505, 54, 500, 500)
                            .Add(1010, 559, 500, 500)
                            .Add(1515, 1064, 500, 500)
                            .Add(2525, 1870, 500, 500)
                            .Add(2020, 1333, 500, 500)
                            .Add(1515, 559, 500, 500))
                        .Add("panel", new TextureGroup(4096, 4096, 1)
                            .Add(1010, 54, 500, 500))
                        .Add("road_brick", new TextureGroup(4096, 4096, 2)
                            .Add(2525, 1365, 500, 500)
                            .Add(2020, 828, 500, 500))
                        .Add("roof_tiles", new TextureGroup(4096, 4096, 1)
                            .Add(2020, 400, 500, 423))
                        .Add("sand", new TextureGroup(4096, 4096, 3)
                            .Add(3030, 1172, 500, 399)
                            .Add(3030, 1576, 500, 500)
                            .Add(1515, 54, 500, 500))
                        .Add("stone", new TextureGroup(4096, 4096, 3)
                            .Add(0, 2079, 500, 1500)
                            .Add(2020, 3353, 500, 743)
                            .Add(2525, 3385, 500, 711))
                        .Add("tree", new TextureGroup(4096, 4096, 3)
                            .Add(0, 3584, 512, 512)
                            .Add(517, 3584, 512, 512)
                            .Add(1034, 3584, 512, 512))
                        .Add("wood", new TextureGroup(4096, 4096, 3)
                            .Add(2020, 27, 500, 368)
                            .Add(2525, 507, 500, 348)
                            .Add(2525, 860, 500, 500)));

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
