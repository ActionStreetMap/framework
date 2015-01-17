using System;
using System.Threading;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public sealed class ScheduledDisposable : ICancelable
    {
        private readonly IScheduler scheduler;
        private volatile IDisposable disposable;
        private int isDisposed = 0;

        /// <summary />
        public ScheduledDisposable(IScheduler scheduler, IDisposable disposable)
        {
            this.scheduler = scheduler;
            this.disposable = disposable;
        }
        /// <summary />
        public IScheduler Scheduler
        {
            get { return scheduler; }
        }
        /// <summary />
        public IDisposable Disposable
        {
            get { return disposable; }
        }
        /// <summary />
        public bool IsDisposed
        {
            get { return isDisposed != 0; }
        }
        /// <summary />
        public void Dispose()
        {
            Scheduler.Schedule(DisposeInner);
        }

        private void DisposeInner()
        {
            if (Interlocked.Increment(ref isDisposed) == 0)
            {
                disposable.Dispose();
            }
        }
    }
}
