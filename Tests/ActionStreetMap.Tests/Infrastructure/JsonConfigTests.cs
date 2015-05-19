using System;
using System.Linq;
using ActionStreetMap.Infrastructure.Config;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Infrastructure
{
    [TestFixture]
    public class JsonConfigTests
    {
        private IConfigSection _stubSection;

        [TestFixtureSetUp]
        public void Initialize()
        {
            var config = TestHelper.GetJsonConfig(TestHelper.ConfigTestRootFile);
            _stubSection = config.GetSection("stubs");
        }

        #region Elementary

        [Test]
        public void CanReadStringValue()
        {
            var value = _stubSection.GetString("string", null);
            Assert.AreEqual("string_value", value);
        }

        [Test]
        public void CanReadIntValue()
        {
            var value = _stubSection.GetInt("int",-1);
            Assert.AreEqual(55, value);
        }

        [Test]
        public void CanReadFloatValue()
        {
            var value = _stubSection.GetFloat("float",-1);
            Assert.IsTrue(Compare(5.12f, value));
        }

        [Test]
        public void CanReadArray()
        {
            // ARRANGE
            var config = new JsonConfigSection("{\"array\":[{\"k\":1},{\"k\":2},{\"k\":3}]}");

            // ACT
            var array = config.GetSections("array").ToList();

            // ASSERT
            Assert.AreEqual(3, array.Count);
        }

        [Test]
        public void CanReadSection()
        {
            // ARRANGE
            var config = new JsonConfigSection("{\"node\":{\"k\":1}}");

            // ACT
            var node = config.GetSection("node");

            // ASSERT
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.GetInt("k", -1));
        }

        [Test]
        public void CanReadValueFromString()
        {
            // ARRANGE
            var config = new JsonConfigSection("{\"k\":\"ggg\"}");

            // ACT
            var value = config.GetString("k", null);

            // ASSERT
            Assert.AreEqual("ggg", value);
        }

        #endregion

        #region Helpers

        public static bool Compare(float a, float b)
        {
            return Math.Abs(a - b) < float.Epsilon;
        }

        #endregion
    }
}