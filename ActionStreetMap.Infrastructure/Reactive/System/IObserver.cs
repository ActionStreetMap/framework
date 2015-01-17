// defined from .NET Framework 4.0 and NETFX_CORE

using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public interface IObserver<T>
    {
        /// <summary />
        void OnCompleted();
        /// <summary />
        void OnError(Exception error);
        /// <summary />
        void OnNext(T value);
    }
}
