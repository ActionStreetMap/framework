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
        /// <param name="bbox">Bounding box.</param>
        IObservable<Element> SearchByTag(string key, string value, BoundingBox bbox);

        /// <summary> Searches all elements with given text in tagss in current active element source. </summary>
        /// <param name="text">text to search.</param>
        /// <param name="bbox">Bounding box.</param>
        /// <param name="zoomLevel">Zoom level.</param>
        IObservable<Element> SearchByText(string text, BoundingBox bbox, int zoomLevel);
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
        public IObservable<Element> SearchByTag(string key, string value, BoundingBox bbox)
        {
            return Observable.Create<Element>(o =>
            {
                var elementSource = GetElementSource(bbox);
                foreach (var pair in elementSource.KvStore.Search(new KeyValuePair<string, string>(key, value)))
                {
                    var kvOffset = elementSource.KvIndex.GetOffset(pair);
                    var usageOffset = elementSource.KvStore.GetUsage(kvOffset);
                    var offsets = elementSource.KvUsage.Get(usageOffset);
                    foreach (var offset in offsets)
                    {
                        var element = elementSource.ElementStore.Get(offset, bbox);
                        if (element != null)
                            o.OnNext(element);
                    }
                }
                o.OnCompleted();
                return Disposable.Empty;
            });
        }

        /// <inheritdoc />
        public IObservable<Element> SearchByText(string text, BoundingBox bbox, int zoomLevel)
        {
            var elementSource = GetElementSource(bbox);
            return elementSource
                .Get(bbox, zoomLevel)
                .Where(e => e.Tags.Any(t => t.Key.Contains(text) || t.Value.Contains(text)));
        }

        private ElementSource GetElementSource(BoundingBox bbox)
        {
            var elementSource = _elementSourceProvider.Get(bbox).Wait() as ElementSource;
            if (elementSource == null)
                throw new NotSupportedException(Strings.SearchNotSupported);
            return elementSource;
        }
    }
}
