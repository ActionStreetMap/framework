using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Bootstrappers;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Explorer.Tiling;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Tiles
{
    [TestFixture (Description = "This test class uses mocks.")]
    public class TileModelEditorTests
    {
        private ITileModelEditor _tileEditor;
        private IContainer _container;
        private static Mock<IModelBuilder> _buildingModelBuilder;
        private static Mock<IElementSourceEditor> _elementSourceEditor;
        private static Mock<IElementSourceProvider> _elementSourceProvider;

        [SetUp]
        public void Setup()
        {
            _elementSourceProvider = new Mock<IElementSourceProvider>();
            _elementSourceProvider.Setup(p => p.Get(It.IsAny<BoundingBox>()))
                .Returns(Observable.Empty<IElementSource>());

            _buildingModelBuilder = new Mock<IModelBuilder>();
            _buildingModelBuilder.Setup(b => b.Name).Returns("building");

            _elementSourceEditor = new Mock<IElementSourceEditor>();

            _container = new Container();
            TestHelper.GetGameRunner(_container, false)
                      .RegisterPlugin<EditorBootstrapperPlugin>("editor")
                      .Bootstrap()
                      .RunGame(TestHelper.BerlinTestFilePoint);
            _tileEditor = _container.Resolve<ITileModelEditor>();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void CanAddBuilding()
        {
            // ARRANGE
            var building = CreateBuilding();

            // ACT
            _tileEditor.AddBuilding(building);

            // ASSERT
            AssertMocks(Times.Once(), Times.Once(), Times.Once(), Times.Never());
        }

        [Test (Description = "Cehcks whether additional logic is executed only once.")]
        public void AddTwoBuildings()
        {
            // ARRANGE & ACT
            _tileEditor.AddBuilding(CreateBuilding());

            _elementSourceProvider.Setup(p => p.Get(It.IsAny<BoundingBox>()))
                .Returns(Observable.Return(ReflectionUtils
                    .GetFieldValue<IElementSource>(_tileEditor, "_currentElementSource")));

            _tileEditor.AddBuilding(CreateBuilding());

            // ASSERT
            AssertMocks(Times.Exactly(2), Times.AtLeast(1), Times.Once(), Times.Never());
        }

        #region AssertHelpers

        private void AssertMocks(Times buildingModelTimes, Times setElementSourceTimes, 
            Times addElementSourceTimes, Times commitElementSource)
        {
            _buildingModelBuilder.Verify(b =>
               b.BuildArea(It.IsAny<Tile>(), It.IsAny<Rule>(), It.IsAny<Area>()),
               buildingModelTimes, "Building model builder is not called.");

            _elementSourceEditor.VerifySet(e => e.ElementSource = It.IsNotNull<IElementSource>(),
                setElementSourceTimes, "Element source should be set.");

            _elementSourceProvider.Verify(p => p.Add(It.IsNotNull<IElementSource>()), 
                addElementSourceTimes, "Element source should be added to element source provider.");

            _elementSourceEditor.Verify(e => e.Commit(), commitElementSource, "Commit: element source.");
        }

        #endregion

        #region Helpers

        private Building CreateBuilding()
        {
            return new Building()
            {
                Footprint = new List<Vector2d>()
                {
                    new Vector2d(0, 0),
                    new Vector2d(0, 50),
                    new Vector2d(50, 50),
                    new Vector2d(50, 0),
                }
            };
        }

        #endregion

        #region Nested classes

        private class EditorBootstrapperPlugin : BootstrapperPlugin
        {
            public override string Name { get { return "editor"; } }

            public override bool Run()
            {
                Container.RegisterInstance<IModelBuilder>(_buildingModelBuilder.Object, "building");
                Container.RegisterInstance<IElementSourceEditor>(_elementSourceEditor.Object);
                Container.RegisterInstance<IElementSourceProvider>(_elementSourceProvider.Object);
                return true;
            }
        }

        #endregion
    }
}
