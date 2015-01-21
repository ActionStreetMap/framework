using System;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Core.Utilities
{
    /// <summary>
    ///     Provides extensions for parallel computations.
    /// </summary>
    public static class ParallelExtensions
    {
        /// <summary>
        ///     Represents the completion of an observable sequence whether it’s empty or no.
        /// </summary>
        public static IObservable<Unit> AsCompletion<T>(this IObservable<T> observable)
        {
            return observable.Select(_ => Unit.Default)
                .IgnoreElements()
                .Concat(Observable.Return(Unit.Default));
        }

        /// <summary>
        ///     Doing work after the sequence is complete and not as things come in.
        /// </summary>
        public static IObservable<TRet> ContinueAfter<T, TRet>(this IObservable<T> observable, Func<IObservable<TRet>> continuation)
        {
            return observable.Select(_ => default(TRet))
              .IgnoreElements()
              .Concat(continuation());
        }

        /// <summary>
        ///     Parallelize processing of quad matrix.
        /// </summary>
        /// <param name="matrix">Source matrix.</param>
        /// <param name="action">Action.</param>
        public static void Parallel<T>(this T[,] matrix, Action<int, int> action)
        {
            System.Diagnostics.Debug.Assert(matrix.GetLength(0) == matrix.GetLength(1));
            Observable.WhenAll(GetChunks(matrix.GetLength(0), (start, end) =>
            {
                action(start, end);
                return new Unit();
            })).Wait();
        }

        /// <summary>
        ///     Parallelize processing of quad matrix.
        /// </summary>
        /// <param name="matrix">Source matrix.</param>
        /// <param name="func">Function.</param>
        public static TK[] Parallel<T, TK>(this T[,] matrix, Func<int, int, TK> func)
        {
            System.Diagnostics.Debug.Assert(matrix.GetLength(0) == matrix.GetLength(1));
            return Observable.WhenAll(GetChunks(matrix.GetLength(0), func)).Wait();
        }

        private static IObservable<T>[] GetChunks<T>(int count, Func<int, int, T> func)
        {
            int parallelDegree = Environment.ProcessorCount;
            int maxSize = (int)Math.Ceiling(count / (double)parallelDegree);
            int k = 0;
            var chunks = new IObservable<T>[parallelDegree];
            for (int i = 0; i < parallelDegree; i++)
            {
                var start = k;
                var end = k + maxSize;
                chunks[i] = Observable.Start(() => func(start, end > count ? count : end));
                k += maxSize;
            }
            return chunks;
        }
    }
}
