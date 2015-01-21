using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionStreetMap.Infrastructure.Reactive
{
    public static partial class Observable
    {
        /// <summary>
        ///     Represents the completion of an observable sequence whether it’s empty or no.
        /// </summary>
        public static IObservable<Unit> AsCompletion<T>(this IObservable<T> observable)
        {
            return Observable.Create<Unit>(observer =>
            {
                Action onCompleted = () =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                };
                return observable.Subscribe(_ => { }, observer.OnError, onCompleted);
            });
        }

        /// <summary>
        ///     Doing work after the sequence is complete and not as things come in.
        /// </summary>
        public static IObservable<TRet> ContinueWith<T, TRet>(
          this IObservable<T> observable, Func<IObservable<TRet>> selector)
        {
            return observable.AsCompletion().SelectMany(_ => selector());
        }
    }
}
