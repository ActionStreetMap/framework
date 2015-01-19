using System;
using System.Collections.Generic;
using System.Text;

namespace ActionStreetMap.Infrastructure.Reactive
{
    public interface ICancelable : IDisposable
    {
        bool IsDisposed { get; }
    }
}
