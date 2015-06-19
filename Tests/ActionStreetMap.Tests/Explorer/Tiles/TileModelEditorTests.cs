using System.Collections.Generic;
using ActionStreetMap.Core;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Explorer.Scene.Builders;
using ActionStreetMap.Explorer.Tiling;
using ActionStreetMap.Infrastructure.Bootstrap;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Tiles
{
    [TestFixture]
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
            _buildingModelBuilder.Verify(b => 
                b.BuildArea(It.IsAny<Tile>(), It.IsAny<Rule>(), It.Is<Area>(w => w.Id == 0)), 
                Times.Once(), "Building model builder is not called.");

            _elementSourceEditor.VerifySet(e => e.ElementSource = It.IsNotNull<IElementSource>(),
                Times.Once, "Element source should be set.");

            _elementSourceProvider.Verify(p => p.Add(It.IsNotNull<IElementSource>()), Times.Once,
                "Element source should be added to element source provider.");
        }

        #region Helpers

        private Building CreateBuilding()
        {
            return new Building()
            {
                Id = 0,
                Footprint = new List<MapPoint>()
                {
                    new MapPoint(0, 0),
                    new MapPoint(0, 50),
                    new MapPoint(50, 50),
                    new MapPoint(50, 0),
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
