using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public class CancellationToken
    {
        readonly ICancelable source;
        /// <summary />
        public static CancellationToken Empty = new CancellationToken(new BooleanDisposable());
        /// <summary />
        public CancellationToken(ICancelable source)
        {
            if (source == null) throw new ArgumentNullException("source");

            this.source = source;
        }
        /// <summary />
        public bool IsCancellationRequested
        {
            get
            {
                return source.IsDisposed;
            }
        }
        /// <summary />
        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }
    }
}