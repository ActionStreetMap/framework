using System;
using ActionStreetMap.Maps.Data;
using ActionStreetMap.Maps.Data.Import;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Maps.Index
{
    [TestFixture]
    public class ElementSourceEditorTests
    {
        private IElementSource _elementSource;

        [SetUp]
        public void Setup()
        {
            var indexSettings = TestHelper.GetIndexSettings();
            var fileSystemService = TestHelper.GetFileSystemService();
            var indexBuilder = new InMemoryIndexBuilder("xml", fileSystemService.ReadStream(TestHelper.BerlinXmlData),
               indexSettings, TestHelper.GetObjectPool(), new ConsoleTrace());
            indexBuilder.Build();

            _elementSource = new ElementSource(indexBuilder.BoundingBox, indexBuilder.KvUsage,
               indexBuilder.KvIndex, indexBuilder.KvStore, indexBuilder.Store, indexBuilder.Tree);
        }

        [TearDown]
        public void TearDown()
        {
            _elementSource.Dispose();
        }

        [Test]
        public void CanInsertElementIntoElementSource()
        {
            throw new NotImplementedException();
        }
    }
}
