using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public class SerialDisposable : IDisposable, ICancelable
    {
        readonly object gate = new object();
        IDisposable current;
        bool disposed;

        /// <summary />
        public bool IsDisposed { get { lock (gate) { return disposed; } } }

        /// <summary />
        public IDisposable Disposable
        {
            get
            {
                return current;
            }
            set
            {
                var shouldDispose = false;
                var old = default(IDisposable);
                lock (gate)
                {
                    shouldDispose = disposed;
                    if (!shouldDispose)
                    {
                        old = current;
                        current = value;
                    }
                }
                if (old != null)
                {
                    old.Dispose();
                }
                if (shouldDispose && value != null)
                {
                    value.Dispose();
                }
            }
        }
        /// <summary />
        public void Dispose()
        {
            var old = default(IDisposable);

            lock (gate)
            {
                if (!disposed)
                {
                    disposed = true;
                    old = current;
                    current = null;
                }
            }

            if (old != null)
            {
                old.Dispose();
            }
        }
    }
}