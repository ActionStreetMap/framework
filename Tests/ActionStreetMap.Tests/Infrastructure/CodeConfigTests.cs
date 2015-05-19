using ActionStreetMap.Infrastructure.Config;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Infrastructure
{
    [TestFixture]
    public class CodeConfigTests
    {
        [Test]
        public void CanReadStringValue()
        {
            // ARRANGE
            var testKey = "someKey";
            var testValue = "someStringValue";
            var configSection = new CodeConfigSection()
                .Add(testKey, testValue);

            // ACT
            var result = configSection.GetString(testKey, null);

            //ASSERT
            Assert.AreEqual(testValue, result);
        }

        [Test]
        public void CanReadIntValue()
        {
            // ARRANGE
            var testKey = "someKey";
            var testValue = 1;
            var configSection = new CodeConfigSection()
                .Add(testKey, testValue);

            // ACT
            var result = configSection.GetInt(testKey, -1);

            //ASSERT
            Assert.AreEqual(testValue, result);
        }

        [Test]
        public void CanReadFloatValue()
        {
            // ARRANGE
            var testKey = "someKey";
            var testValue = 2.1f;
            var configSection = new CodeConfigSection()
                .Add(testKey, testValue);

            // ACT
            var result = configSection.GetFloat(testKey, -1.1f);

            //ASSERT
            Assert.AreEqual(testValue, result);
        }

        [Test]
        public void CanReadSection()
        {
            // ARRANGE
            var testValue1 = 2.1f;
            var testValue2 = "someString";
            var configSection = new CodeConfigSection()
                .Add("section1/value1", testValue1)
                .Add("section1/value2", testValue2)
                .Add("section2/section3/value3", 1);

            // ACT
            var section1 = configSection.GetSection("section1");

            //ASSERT
            Assert.AreEqual(testValue1, section1.GetFloat("value1", 0));
            Assert.AreEqual(testValue2, section1.GetString("value2", ""));
        }

        [Test]
        public void CanReadInnerSection()
        {
            // ARRANGE
            var testValue3 = 1;
            var configSection = new CodeConfigSection()
                .Add("section1/value1", 1)
                .Add("section1/value2", 2)
                .Add("section2/section3/value3", testValue3);

            // ACT
            var section = configSection.GetSection("section2/section3");

            //ASSERT
            Assert.AreEqual(testValue3, section.GetInt("value3", 0));
        }
    }
}
