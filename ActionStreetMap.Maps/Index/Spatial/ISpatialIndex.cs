using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionStreetMap.Maps.Index.Spatial
{
    public interface ISpatialIndex<T>
    {
        IObservable<T> Search(BoundingBox query);
    }
}
