using System;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Entities;

namespace ActionStreetMap.Maps.Sources
{
    /// <summary>
    ///     This implementation uses API v6 <see cref="http://wiki.openstreetmap.org/wiki/OSM_Protocol_Version_0.6"/> 
    ///     to get map data from remote server.
    /// </summary>
    public sealed class RemoteXmlElementSource: IElementSource, IConfigurable
    {
        // http://api.openstreetmap.org/api/0.6/map?bbox=left,bottom,right,top

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
