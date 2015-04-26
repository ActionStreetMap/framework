using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Data.Search
{
    /// <summary> Provides the way to find elements by given text parameters. </summary>
    public interface ISearchEngine
    {
        /// <summary> Searches all elements with given key and similiar value in current active element source. </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        IObservable<Element> SearchByTag(string key, string value);

        /// <summary> Searches all elements with given text in tagss in current active element source. </summary>
        /// <param name="text">text to search.</param>
        /// <param name="bbox">Bounding box.</param>
        IObservable<Element> SearchByText(string text, BoundingBox bbox);
    }

    /// <summary>
    ///     Implementation of <see cref="ISearchEngine"/> which depends on default implementation of <see cref="IElementSource"/>.
    /// </summary>
    internal class SearchEngine: ISearchEngine
    {
        private readonly IElementSourceProvider _elementSourceProvider;

        /// <summary> Creates instance of <see cref="SearchEngine"/>. </summary>
        /// <param name="elementSourceProvider">Element source provider.</param>
        [Dependency]
        public SearchEngine(IElementSourceProvider elementSourceProvider)
        {
            _elementSourceProvider = elementSourceProvider;
        }

        /// <inheritdoc />
        public IObservable<Element> SearchByTag(string key, string value)
        {
            return Observable.Create<Element>(o =>
            {
                var elementSource = GetElementSource();
                foreach (var pair in elementSource.KvStore.Search(new KeyValuePair<string, string>(key, value)))
                {
                    var kvOffset = elementSource.KvIndex.GetOffset(pair);
                    var usageOffset = elementSource.KvStore.GetUsage(kvOffset);
                    var offsets = elementSource.KvUsage.Get(usageOffset);
                    foreach (var offset in offsets)
                        o.OnNext(elementSource.ElementStore.Get(offset));
                }
                o.OnCompleted();
                return Disposable.Empty;
            });
        }

        /// <inheritdoc />
        public IObservable<Element> SearchByText(string text, BoundingBox bbox)
        {
            var elementSource = GetElementSource();
            return elementSource
                .Get(bbox)
                .Where(e => e.Tags.Any(t => t.Key.Contains(text) || t.Value.Contains(text)));
        }

        private ElementSource GetElementSource()
        {
            var elementSource = _elementSourceProvider.Get().Wait() as ElementSource;
            if (elementSource == null)
                throw new NotSupportedException(Strings.SearchNotSupported);
            return elementSource;
        }
    }
}
