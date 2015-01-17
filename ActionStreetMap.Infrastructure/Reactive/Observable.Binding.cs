using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    public static partial class Observable
    {
        /// <summary />
        public static IConnectableObservable<T> Multicast<T>(this IObservable<T> source, ISubject<T> subject)
        {
            return new ConnectableObservable<T>(source, subject);
        }
        /// <summary />
        public static IConnectableObservable<T> Publish<T>(this IObservable<T> source)
        {
            return source.Multicast(new Subject<T>());
        }
        /// <summary />
        public static IConnectableObservable<T> Publish<T>(this IObservable<T> source, T initialValue)
        {
            return source.Multicast(new BehaviorSubject<T>(initialValue));
        }
        /// <summary />
        public static IConnectableObservable<T> PublishLast<T>(this IObservable<T> source)
        {
            return source.Multicast(new AsyncSubject<T>());
        }
        /// <summary />
        public static IConnectableObservable<T> Replay<T>(this IObservable<T> source)
        {
            return source.Multicast(new ReplaySubject<T>());
        }
        /// <summary />
        public static IConnectableObservable<T> Replay<T>(this IObservable<T> source, IScheduler scheduler)
        {
            return source.Multicast(new ReplaySubject<T>(scheduler));
        }
        /// <summary />
        public static IConnectableObservable<T> Replay<T>(this IObservable<T> source, int bufferSize)
        {
            return source.Multicast(new ReplaySubject<T>(bufferSize));
        }
        /// <summary />
        public static IConnectableObservable<T> Replay<T>(this IObservable<T> source, int bufferSize, IScheduler scheduler)
        {
            return source.Multicast(new ReplaySubject<T>(bufferSize, scheduler));
        }
        /// <summary />
        public static IConnectableObservable<T> Replay<T>(this IObservable<T> source, TimeSpan window)
        {
            return source.Multicast(new ReplaySubject<T>(window));
        }
        /// <summary />
        public static IConnectableObservable<T> Replay<T>(this IObservable<T> source, TimeSpan window, IScheduler scheduler)
        {
            return source.Multicast(new ReplaySubject<T>(window, scheduler));
        }
        /// <summary />
        public static IConnectableObservable<T> Replay<T>(this IObservable<T> source, int bufferSize, TimeSpan window, IScheduler scheduler)
        {
            return source.Multicast(new ReplaySubject<T>(bufferSize, window, scheduler));
        }
        /// <summary />
        public static IObservable<T> RefCount<T>(this IConnectableObservable<T> source)
        {
            var connection = default(IDisposable);
            var gate = new object();
            var refCount = 0;

            return Observable.Create<T>(observer =>
            {
                var subscription = source.Subscribe(observer);

                lock (gate)
                {
                    if (++refCount == 1)
                    {
                        connection = source.Connect();
                    }
                }

                return Disposable.Create(() =>
                {
                    subscription.Dispose();
                    lock (gate)
                    {
                        if (--refCount == 0)
                        {
                            connection.Dispose(); // connection isn't null.
                        }
                    }
                });
            });
        }
    }
}