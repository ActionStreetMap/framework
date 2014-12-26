
using System;
using System.Collections.Generic;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Osm.Entities;

namespace ActionStreetMap.Osm.Index.Search
{
    /// <summary>
    ///     Provides the way to find elements by given text parameters in default implementation of <see cref="IElementSource"/>.
    /// </summary>
    public class SearchEngine
    {
        private readonly IElementSourceProvider _elementSourceProvider;

        /// <summary>
        ///     Creates instance of <see cref="SearchEngine"/>
        /// </summary>
        /// <param name="elementSourceProvider">Element source provider.</param>
        [Dependency]
        public SearchEngine(IElementSourceProvider elementSourceProvider)
        {
            _elementSourceProvider = elementSourceProvider;
        }

        /// <summary>
        ///     Searches all elements with given key and similiar value in current active element source.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        /// <returns>Element collection.</returns>
        public IEnumerable<Element> SearchByTag(string key, string value)
        {
            var elementSource = GetElementSource();
            foreach (var pair in elementSource.KvStore.Search(new KeyValuePair<string, string>(key, value)))
            {
                var kvOffset = elementSource.KvIndex.GetOffset(pair);
                var usageOffset = elementSource.KvStore.GetUsage(kvOffset);
                var offsets = elementSource.KvUsage.Get(usageOffset);
                foreach (var offset in offsets)
                    yield return elementSource.ElementStore.Get(offset);
            }
        }

        private ElementSource GetElementSource()
        {
            var elementSource = _elementSourceProvider.Get() as ElementSource;
            if (elementSource == null)
                throw new NotSupportedException(Strings.SearchNotSupported);
            return elementSource;
        }
    }
}
