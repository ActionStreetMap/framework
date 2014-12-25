
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

        [Dependency]
        public SearchEngine(IElementSourceProvider elementSourceProvider
            /*KeyValueIndex index, KeyValueStore kvStore, ElementStore elementStore*/)
        {
            _elementSourceProvider = elementSourceProvider;
        }

        /// <summary>
        ///     Lookups elements with given key/value in current active element source.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        /// <returns>Elements.</returns>
        public IEnumerable<Element> LookupByTag(string key, string value)
        {
            var elementSource = GetElementSource();

            //var offset = elementSource.KvIndex.GetOffset(new KeyValuePair<string, string>(key, value));

            return null;
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
