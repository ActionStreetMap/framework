using System;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Tests.Explorer
{
    public class TestScheduler: IScheduler
    {
        public DateTimeOffset Now
        {
            get { return Scheduler.Now; }
        }

        public IDisposable Schedule(Action action)
        {
            return Disposable.Empty;
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action)
        {
            return Disposable.Empty;
        }
    }
}
