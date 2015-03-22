using ActionStreetMap.Explorer.Utils;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.Utils
{
    [TestFixture]
    class GradientTests
    {
        [Test]
        public void CanParseGradientFromMapCssString()
        {
            // ARRANGE
            string gradientString = "gradient(#0fffff, #099999 50%, #033333 70%, #000000)";

            // ACT
            var gradient = GradientUtils.ParseGradient(gradientString);

            // ASSERT
            Assert.IsNotNull(gradient);
        }
    }
}
