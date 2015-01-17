using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public interface ICancelable : IDisposable
    {
        /// <summary />
        bool IsDisposed { get; }
    }
}
