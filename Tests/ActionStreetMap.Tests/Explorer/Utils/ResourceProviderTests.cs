using ActionStreetMap.Explorer.Scene.Utils;
using ActionStreetMap.Infrastructure.Config;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Utils
{
    [TestFixture]
    public class ResourceProviderTests
    {
        [Test]
        public void CanParseGradients()
        {
            // ARRANGE
            var resourceProvider = new UnityResourceProvider(TestHelper.GetFileSystemService());
            resourceProvider.Trace = new ConsoleTrace();
            var configMock = new Mock<IConfigSection>();
            configMock.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(TestHelper.TestGradientPath);
            resourceProvider.Configure(configMock.Object);

            // ACT
            var gradient = resourceProvider.GetGradient(0);

            // ASSERT
            Assert.IsNotNull(gradient);
        }
    }
}
