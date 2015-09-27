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
                        .Add("asphalt", new TextureGroup(4098, 3749, 2)
                            .Add(4098, 6036, 1500, 1500)
                            .Add(9201, 2462, 1000, 1000))
                        .Add("background", new TextureGroup(4098, 3749, 1)
                            .Add(8710, 3869, 1000, 1000))
                        .Add("bark", new TextureGroup(4098, 3749, 4)
                            .Add(5599, 7037, 1000, 1000)
                            .Add(7200, 5536, 1000, 1000)
                            .Add(8710, 4870, 1000, 1000)
                            .Add(9711, 3463, 1000, 1000))
                        .Add("barrier", new TextureGroup(4098, 3749, 2)
                            .Add(0, 0, 3714, 2175)
                            .Add(9201, 1501, 1280, 960))
                        .Add("brick", new TextureGroup(4098, 3749, 4)
                            .Add(4098, 4435, 1600, 1600)
                            .Add(4098, 2434, 2000, 2000)
                            .Add(2049, 6107, 2000, 2000)
                            .Add(6099, 2434, 1600, 1600))
                        .Add("canvas", new TextureGroup(4098, 3749, 1)
                            .Add(7700, 3002, 1300, 866))
                        .Add("concrete", new TextureGroup(4098, 3749, 5)
                            .Add(0, 4058, 2048, 2048)
                            .Add(0, 6107, 2048, 2048)
                            .Add(6600, 7037, 1000, 1000)
                            .Add(9711, 4464, 1000, 1000)
                            .Add(8201, 5871, 1000, 1000))
                        .Add("floor", new TextureGroup(4098, 3749, 6)
                            .Add(6985, 8038, 500, 1000)
                            .Add(7601, 7873, 500, 1000)
                            .Add(8102, 7873, 500, 1000)
                            .Add(7601, 6872, 1000, 1000)
                            .Add(9202, 5871, 1000, 1000)
                            .Add(8602, 6872, 1000, 1000))
                        .Add("grass", new TextureGroup(4098, 3749, 5)
                            .Add(6099, 4035, 1500, 1500)
                            .Add(5699, 5536, 1500, 1500)
                            .Add(7208, 0, 1500, 1500)
                            .Add(8709, 0, 1500, 1500)
                            .Add(7700, 1501, 1500, 1500))
                        .Add("ground", new TextureGroup(4098, 3749, 1)
                            .Add(9603, 6872, 1000, 1000))
                        .Add("metal", new TextureGroup(4098, 3749, 7)
                            .Add(0, 8156, 1000, 1000)
                            .Add(0, 9157, 1000, 1000)
                            .Add(1001, 8156, 1000, 1000)
                            .Add(2049, 8108, 1000, 1000)
                            .Add(1001, 9157, 1000, 1000)
                            .Add(2002, 9109, 1000, 1000)
                            .Add(3050, 8108, 1000, 1000))
                        .Add("panel", new TextureGroup(4098, 3749, 1)
                            .Add(2049, 4058, 2048, 2048))
                        .Add("road_brick", new TextureGroup(4098, 3749, 2)
                            .Add(4051, 7537, 1000, 1000)
                            .Add(3003, 9109, 1000, 1000))
                        .Add("roof_tiles", new TextureGroup(4098, 3749, 1)
                            .Add(10210, 0, 750, 634))
                        .Add("sand", new TextureGroup(4098, 3749, 3)
                            .Add(10202, 2462, 900, 719)
                            .Add(4051, 8538, 1000, 1000)
                            .Add(5052, 8038, 1000, 1000))
                        .Add("stone", new TextureGroup(4098, 3749, 3)
                            .Add(6053, 8038, 931, 1500)
                            .Add(7700, 3869, 1009, 1500)
                            .Add(2559, 2176, 1055, 1500))
                        .Add("tree", new TextureGroup(4098, 3749, 3)
                            .Add(10210, 635, 512, 512)
                            .Add(10482, 1148, 512, 512)
                            .Add(10482, 1661, 512, 512))
                        .Add("wood", new TextureGroup(4098, 3749, 3)
                            .Add(0, 2176, 2558, 1881)
                            .Add(3715, 0, 3492, 2433)
                            .Add(5052, 9039, 1000, 1000)));

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
