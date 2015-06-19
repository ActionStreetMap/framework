using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Maps.Index
{
    [TestFixture]
    public class ElementSourceProviderTests
    {
        private IContainer _container;
        private IElementSourceProvider _elementSourceProvider;

        [SetUp]
        public void Setup()
        {
            _container = new Container();
            var gameRunner = TestHelper.GetGameRunner(_container);
            _elementSourceProvider = _container.Resolve<IElementSourceProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void CanCreateElementSource()
        {
            // ACT
            var observable = _elementSourceProvider.Get(CreateDefaultBoundingBox());
            
            // ASSERT
            Assert.IsNotNull(observable.Wait());
        }

        [Test]
        public void CanAddElementSource()
        {
            // ARRANGE
            var boundingBox = CreateDefaultBoundingBox();
            Mock<IElementSource> elementSource = new Mock<IElementSource>();
            elementSource.Setup(e => e.BoundingBox).Returns(boundingBox);

            // ACT
            _elementSourceProvider.Add(elementSource.Object);

            // ASSERT
            Assert.AreEqual(2, _elementSourceProvider.Get(boundingBox).Count());
        }

        [Test(Description = "Non readonly element sources should be first.")]
        public void CanReturnNonReadOnlyFirst()
        {
            // ARRANGE
            var boundingBox = CreateDefaultBoundingBox();
            Mock<IElementSource> elementSource = new Mock<IElementSource>();
            elementSource.Setup(e => e.BoundingBox).Returns(boundingBox);

            // ACT
            _elementSourceProvider.Add(elementSource.Object);

            // ASSERT
            Assert.IsFalse(_elementSourceProvider.Get(boundingBox).Wait().IsReadOnly);
        }

        private BoundingBox CreateDefaultBoundingBox()
        {
            return new BoundingBox(TestHelper.TestMinPoint, TestHelper.TestMaxPoint);
        }
    }
}