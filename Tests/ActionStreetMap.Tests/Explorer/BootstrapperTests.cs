using System.Linq;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Infrastructure.Dependencies;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer
{
    [TestFixture]
    public class BootstrapperTests
    {
        [Test]
        public void CanResolveModelBuilders()
        {
            // ARRANGE
            var container = new Container();

            // this should fill container
            var root = TestHelper.GetGameRunner(container);
            root.RunGame(TestHelper.BerlinTestFilePoint);

            // ACT
            var modelBuilders = container.ResolveAll<IModelBuilder>().ToList();

            // ASSERT
            // NOTE change this value if you add/remove model builders
            Assert.AreEqual(10, modelBuilders.Count);
        }
    }
}