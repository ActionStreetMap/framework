using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public interface IConnectableObservable<T> : IObservable<T>
    {
        /// <summary />
        IDisposable Connect();
    }
    /// <summary />
    public static partial class Observable
    {
        class ConnectableObservable<T> : IConnectableObservable<T>
        {
            readonly IObservable<T> source;
            readonly ISubject<T> subject;

            public ConnectableObservable(IObservable<T> source, ISubject<T> subject)
            {
                this.source = source;
                this.subject = subject;
            }

            public IDisposable Connect()
            {
                var subscription = source.Subscribe(subject);
                return subscription;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return subject.Subscribe(observer);
            }
        }
    }
}