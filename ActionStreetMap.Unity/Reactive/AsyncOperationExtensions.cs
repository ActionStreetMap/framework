using System;
using System.Collections;
using UnityEngine;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public static partial class AsyncOperationExtensions
    {
        /// <summary />
        public static IObservable<AsyncOperation> AsObservable(this AsyncOperation asyncOperation, IProgress<float> progress = null)
        {
            return ObservableUnity.FromCoroutine<AsyncOperation>((observer, cancellation) => AsObservableCore(asyncOperation, observer, progress, cancellation));
        }

        static IEnumerator AsObservableCore(AsyncOperation asyncOperation, IObserver<AsyncOperation> observer, IProgress<float> reportProgress, CancellationToken cancel)
        {
            while (!asyncOperation.isDone && !cancel.IsCancellationRequested)
            {
                if (reportProgress != null)
                {
                    try
                    {
                        reportProgress.Report(asyncOperation.progress);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                }
                yield return null;
            }

            if (cancel.IsCancellationRequested) yield break;

            observer.OnNext(asyncOperation);
            observer.OnCompleted();
        }
    }
}