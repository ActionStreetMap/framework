using System;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Core.Utilities
{
    /// <summary>
    ///     Provides extensions for parallel computations
    /// </summary>
    internal static class ParallelExtensions
    {
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
