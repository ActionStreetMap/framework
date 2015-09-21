using System;
using ActionStreetMap.Explorer.Customization;
using NUnit.Framework;
using UnityEngine;

namespace ActionStreetMap.Tests.Explorer.Customization
{
    [TestFixture]
    public class TexturePackTests
    {
        private const float Precision = 0.0000001f;

        [Test]
        public void CanMapSquare()
        {
            // ARRANGE
            var texturePack = new TextureGroup(1000, 1000);
            var region = texturePack
               .Add(500, 500, 100, 100)
               .Get(1);

            // ACT
            var uv = region.Map(new Vector2(0.5f, 0.5f));

            Assert.IsTrue(Math.Abs(0.55 - uv.x) < Precision);
            Assert.IsTrue(Math.Abs(0.55 - uv.y) < Precision);
        }

        [Test]
        public void CanMapRectangle()
        {
            // ARRANGE
            var texturePack = new TextureGroup(2000, 1000);
            var region = texturePack
               .Add(500, 500, 100, 100)
               .Get(1);

            // ACT
            var uv = region.Map(new Vector2(0.5f, 0.5f));

            Assert.IsTrue(Math.Abs(0.275 - uv.x) < Precision);
            Assert.IsTrue(Math.Abs(0.55 - uv.y) < Precision);
        }
    }
}
