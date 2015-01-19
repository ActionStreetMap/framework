using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public static void Parallel(this float[,] matrix, Action<int, int> action)
        {
            System.Diagnostics.Debug.Assert(matrix.GetLength(0) == matrix.GetLength(1));

            int count = matrix.GetLength(0);
            int parallelDegree = Environment.ProcessorCount;
            int maxSize = (int)Math.Ceiling(count / (double)parallelDegree);
            int k = 0;
            IObservable<Unit>[] chunks = new IObservable<Unit>[parallelDegree];
            for (int i = 0; i < parallelDegree; i++)
            {
                var start = k;
                var end = k + maxSize - 1;
                chunks[i] = Observable.Start(() => action(start, end >= count ? count : end));
                k += maxSize;
            }
            Observable.WhenAll(chunks).Wait();
        }
    }
}
