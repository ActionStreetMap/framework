using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public class BooleanDisposable : IDisposable, ICancelable
    {
        /// <summary />
        public bool IsDisposed { get; private set; }
        /// <summary />
        public BooleanDisposable()
        {
        }

        internal BooleanDisposable(bool isDisposed)
        {
            IsDisposed = isDisposed;
        }
        /// <summary />
        public void Dispose()
        {
            if (!IsDisposed) IsDisposed = true;
        }
    }
}