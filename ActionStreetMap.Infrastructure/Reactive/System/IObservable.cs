// defined from .NET Framework 4.0 and NETFX_CORE

#if !NETFX_CORE

using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    public interface IObservable<T>
    {
        IDisposable Subscribe(IObserver<T> observer);
    }
}

#endif