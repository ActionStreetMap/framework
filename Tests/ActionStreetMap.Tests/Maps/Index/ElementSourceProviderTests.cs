using System;
using System.IO;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data;
using Moq;
using NUnit.Framework;
using ActionStreetMap.Explorer.Infrastructure;

namespace ActionStreetMap.Tests.Maps.Index
{
    [TestFixture]
    public class ElementSourceProviderTests
    {
        [Test]
        public void CanCreateElementSource()
        {
            // ARRANGE
            var directory = "local";
            var configSection = new Mock<IConfigSection>();
            configSection.Setup(c => c.GetString(directory, It.IsAny<string>()))
                .Returns(directory);
            var fileSystemService = Utils.GetFileSystemServiceMock(directory);
            var pathResolver = new Mock<IPathResolver>();
            pathResolver.Setup(p => p.Resolve(It.IsAny<string>())).Returns<string>(s => s);

            fileSystemService.Setup(fs => fs.GetFiles(It.IsAny<string>(), MapConsts.HeaderFileName))
                .Returns(new[] { directory + @"\" + MapConsts.HeaderFileName });
            fileSystemService.Setup(fs => fs.ReadStream(It.IsAny<string>()))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("52.0,13.0 52.4,13.4")));

            var provider = new ElementSourceProvider(pathResolver.Object, fileSystemService.Object,
                TestHelper.GetObjectPool());
            provider.Trace = new Mock<ITrace>().Object;

            // ACT
            provider.Configure(configSection.Object);
            
            var elementSource1 = provider.Get(new BoundingBox(
                new GeoCoordinate(52.0f, 13.0f), new GeoCoordinate(52.1f, 13.1f)));

            // ARRANGE
            Assert.IsNotNull(elementSource1.Wait());
        }
    }
}
