// defined from .NET Framework 4.0 and NETFX_CORE

using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public interface IObservable<T>
    {
        /// <summary />
        IDisposable Subscribe(IObserver<T> observer);
    }
}