using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Maps.Data.Storage;
using ActionStreetMap.Maps.Entities;
using Moq;

namespace ActionStreetMap.Tests.Maps.Index
{
    internal static class Utils
    {
        public static Mock<IFileSystemService> GetFileSystemServiceMock(string directory)
        {
            // NOTE this class uses concrete types, that's why it has such huge arrange
            var keyValueStream = new MemoryStream(new byte[256]);
            keyValueStream.WriteByte(4);
            keyValueStream.WriteByte(7);

            var index = new KeyValueIndex(100, 3);
            var kvUsage = new KeyValueUsage(new MemoryStream(1000));
            var keyValueStore = new KeyValueStore(index, kvUsage, keyValueStream);

            var elementStoreStream = new MemoryStream(new byte[10000]);
            var elementStore = new ElementStore(keyValueStore, elementStoreStream);
            var tree = new RTree<uint>();

            var node = new Node()
            {
                Id = 1,
                Coordinate = new GeoCoordinate(52.0, 13.0),
                Tags = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } }
            };
            var nodeOffset = elementStore.Insert(node);
            tree.Insert(nodeOffset, new PointEnvelop(node.Coordinate));
            var way = new Way()
            {
                Id = 2,
                Coordinates = new List<GeoCoordinate>() { new GeoCoordinate(52.1, 13.1), new GeoCoordinate(52.2, 13.2) },
                Tags = new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } }
            };
            var wayOffset = elementStore.Insert(way);
            tree.Insert(wayOffset, new Envelop(way.Coordinates.First(), way.Coordinates.Last()));

            var fileSystemService = new Mock<IFileSystemService>();

            var kvIndexMemoryStream = new MemoryStream();
            KeyValueIndex.Save(index, kvIndexMemoryStream);
            kvIndexMemoryStream = new MemoryStream(kvIndexMemoryStream.GetBuffer());
            fileSystemService.Setup(fs => fs.ReadStream(string.Format(Consts.KeyValueIndexPathFormat, directory)))
                .Returns(kvIndexMemoryStream);

            fileSystemService.Setup(fs => fs.ReadStream(string.Format(Consts.KeyValueStorePathFormat, directory)))
                .Returns(keyValueStream);

            fileSystemService.Setup(fs => fs.ReadStream(string.Format(Consts.ElementStorePathFormat, directory)))
                .Returns(elementStoreStream);

            var treeStream = new MemoryStream();
            SpatialIndex.Save(tree, treeStream);
            treeStream = new MemoryStream(treeStream.GetBuffer());

            fileSystemService.Setup(fs => fs.ReadStream(string.Format(Consts.SpatialIndexPathFormat, directory)))
                .Returns(treeStream);

            return fileSystemService;
        }
    }
}
