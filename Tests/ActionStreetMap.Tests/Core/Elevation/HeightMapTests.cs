using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Elevation;
using ActionStreetMap.Core.Scene.Models;
using Moq;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.Elevation
{
    [TestFixture]
    public class HeightMapTests
    {
        [TestCase(257, 100, 0, 0)]
        [TestCase(513, 500, 52, 13)]
        [TestCase(1025, 1000, 52, 13)]
        public void CanCreateHeightMapData(int resolution, float tileSize, double latitude, double longitude)
        {
            // ARRANGE 
            var center = new GeoCoordinate(latitude, longitude);
            var bbox = BoundingBox.CreateBoundingBox(center, tileSize / 2);
            var latitudeOffset = (bbox.MaxPoint.Latitude - bbox.MinPoint.Latitude) / resolution;
            var longitudeOffset = (bbox.MaxPoint.Longitude - bbox.MinPoint.Longitude) / resolution;
            var elevationProvider = new Mock<IElevationProvider>();
            elevationProvider.Setup(p => p.GetElevation(It.IsAny<double>(), It.IsAny<double>()))
                .Returns<double, double>((lat, lon) =>
            {
                var i = (int) Math.Round((lat - latitudeOffset/2 - bbox.MinPoint.Latitude) / latitudeOffset);
                var j = (int) Math.Round((lon - longitudeOffset / 2 - bbox.MinPoint.Longitude) / longitudeOffset);

                return i + j;
            });

            var provider = new HeightMapProvider(elevationProvider.Object);
            
            // ACT
            var heightMap = provider.Get(new Tile(center, new MapPoint(), tileSize), resolution);
                
            // ASSERT
            Assert.IsNotNull(heightMap);
            Assert.IsNotNull(heightMap.Data);
            
            var data = heightMap.Data;
            Assert.AreEqual(resolution, data.GetLength(0));
            Assert.AreEqual(resolution, data.GetLength(1));

            var cornerValue = (resolution - 1);
            Assert.AreEqual(0, data[0, 0]);
            Assert.AreEqual(cornerValue, data[resolution - 1, 0]);
            Assert.AreEqual(cornerValue, data[0, resolution - 1]);
            Assert.AreEqual(cornerValue * 2, data[resolution - 1, resolution - 1]);
        }

        [TestCase(257, 100)]
        [TestCase(513, 500)]
        [TestCase(1025, 1000)]
        public void CanLookupInTestData(int resolution, float tileSize)
        {
            // ARRANGE
            var center = new MapPoint(0, 0);
            var bottomLeft = new MapPoint(center.X - tileSize / 2, center.Y - tileSize / 2);
            var topRight = new MapPoint(center.X + tileSize / 2, center.Y + tileSize / 2);
            
            var data = new float[resolution, resolution];

            for (int j = 0; j < resolution; j++)
                for(int i =0 ; i< resolution; i++)
                    data[j, i] = i + j;

            var maxElevation = resolution + resolution - 2;
            var heightMap = new HeightMap()
            {
                Data = data,
                IsFlat = false,
                RightUpperCorner = new MapPoint(center.X + tileSize / 2, center.Y + tileSize / 2),
                LeftBottomCorner = new MapPoint(center.X - tileSize / 2, center.Y - tileSize / 2),
                AxisOffset = tileSize / resolution,
                MaxElevation = maxElevation,
                Resolution = resolution,
                Size = tileSize
            };

            // ACT && ASSERT

            var cornerValue = resolution - 1;
            // center
            // ??
            //Assert.AreEqual((resolution + resolution - 2) / 2, heightMap.LookupHeight(center));
            
            // left upper corner
            Assert.AreEqual(cornerValue, heightMap.LookupHeight(new MapPoint(bottomLeft.X, topRight.Y)));
            // left bottom corner
            Assert.AreEqual(0, heightMap.LookupHeight(bottomLeft));
            
            // right upper corner
            Assert.AreEqual(cornerValue * 2, heightMap.LookupHeight(topRight));
            // right bottom corner
            Assert.AreEqual(cornerValue, heightMap.LookupHeight(new MapPoint(topRight.X, bottomLeft.Y)));
        }
    }
}
