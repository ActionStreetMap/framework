using System;
using System.Collections;

namespace ActionStreetMap.Infrastructure.Reactive
{
    // should be use Interlocked.CompareExchange for Threadsafe?
    // but CompareExchange cause ExecutionEngineException on iOS.
    // AOT...
    // use lock instead

    /// <summary />
    public class SingleAssignmentDisposable : IDisposable, ICancelable
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
                var old = default(IDisposable);
                bool alreadyDisposed;
                lock (gate)
                {
                    alreadyDisposed = disposed;
                    old = current;
                    if (!alreadyDisposed)
                    {
                        if (value == null) return;
                        current = value;
                    }
                }

                if (alreadyDisposed && value != null)
                {
                    value.Dispose();
                    return;
                }

                if (old != null) throw new InvalidOperationException("Disposable is already set");
            }
        }

        /// <summary />
        public void Dispose()
        {
            IDisposable old = null;

            lock (gate)
            {
                if (!disposed)
                {
                    disposed = true;
                    old = current;
                    current = null;
                }
            }

            if (old != null) old.Dispose();
        }
    }
}