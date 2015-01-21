using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Scene.Models;
using ActionStreetMap.Core.Unity;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Tests.Explorer.Tiles
{
    /// <summary>
    ///     This plugin overrides registration of non-testable classes
    /// </summary>
    public class TestBootstrapperPlugin: BootstrapperPlugin
    {
        private readonly TestModelBehaviour _solidModelBehaviour = new TestModelBehaviour("solid");
        private readonly TestModelBehaviour _waterModelBehaviour = new TestModelBehaviour("water");

        public override string Name
        {
            get { return "test"; }
        }

        public override bool Run()
        {
            Scheduler.MainThread = new TestScheduler();

            Container.Register(Component.For<ITrace>().Use<ConsoleTrace>());
            Container.RegisterInstance<IModelBehaviour>(_solidModelBehaviour, "solid");
            Container.RegisterInstance<IModelBehaviour>(_waterModelBehaviour, "water");

            return true;
        }

        /// <summary>
        ///     Dummy model behavior
        /// </summary>
        private class TestModelBehaviour : IModelBehaviour
        {
            public string Name { get; private set; }
            public TestModelBehaviour(string name) { Name = name; }
            public void Apply(IGameObject gameObject, Model model) { }
        }
    }
}
