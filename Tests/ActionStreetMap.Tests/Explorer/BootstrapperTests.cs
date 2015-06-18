using System.Linq;
using ActionStreetMap.Explorer.Scene;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Infrastructure.Dependencies;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer
{
    [TestFixture]
    public class BootstrapperTests
    {
        private Container _container;

        [SetUp]
        public void SetUp()
        {
            _container = new Container();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void CanResolveModelBuilders()
        {
            // ARRANGE
            // this should fill container
            var root = TestHelper.GetGameRunner(_container);
            root.RunGame(TestHelper.BerlinTestFilePoint);

            // ACT
            var modelBuilders = _container.ResolveAll<IModelBuilder>().ToList();

            // ASSERT
            // NOTE change this value if you add/remove model builders
            Assert.AreEqual(9, modelBuilders.Count);
        }
    }
}