using System;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Sources
{
    /// <summary>
    ///     This implementation uses Overpass API <see cref="http://wiki.openstreetmap.org/wiki/Overpass_API"/> to get
    ///     map data.
    /// </summary>
    public sealed class OverpassElementSource: IElementSource, IConfigurable
    {
        /// <inheritdoc />
        public IObservable<Element> Get(BoundingBox bbox)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
