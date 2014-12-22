using System;
using System.IO;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Osm.Index;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Osm.Index
{
    [TestFixture]
    public class ElementSourceProviderTests
    {
        [Test]
        public void CanCreateElementSource()
        {
            // ARRANGE
            var configSection = new Mock<IConfigSection>();

            var stream = new MemoryStream(Encoding.UTF8.GetBytes("52.529760,13.387997 52.533688,13.388490"));
            var fileSystemService = new Mock<IFileSystemService>();
            fileSystemService.Setup(fs => fs.GetFiles(It.IsAny<string>(), Consts.HeaderFileName))
                .Returns(new[] {Consts.HeaderFileName});
            fileSystemService.Setup(fs => fs.ReadStream(It.IsAny<string>()))
                .Returns(stream);

            var provider = new ElementSourceProvider(fileSystemService.Object);
            provider.Trace = new Mock<ITrace>().Object;

            // ACT
            provider.Configure(configSection.Object);
            var elementSource1 = provider.Get(new BoundingBox(
                new GeoCoordinate(52.52f, 13.38f), new GeoCoordinate(52.54f, 13.39f)));
            var elementSource2 = provider.Get(new BoundingBox(
                new GeoCoordinate(52.1f, 13.2f), new GeoCoordinate(52.2f, 13.3f)));

            // ARRANGE
            Assert.IsNotNull(elementSource1);
            Assert.IsNull(elementSource2);
        }
    }
}
