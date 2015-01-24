
using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary>
    ///     Represents object pool which is used to reduced amount of memory allocations.
    /// </summary>
    public interface IObjectPool
    {
        #region Lists
        /// <summary> Returns list from pool or creates new one. </summary>
        List<T> NewList<T>();

        /// <summary> Returns list from pool or creates new one. </summary>
        List<T> NewList<T>(int capacity);

        /// <summary> Stores list in pool. </summary>
        void StoreList<T>(List<T> list, bool isClean = false);
        #endregion

        #region Arrays
        /// <summary> Returns array from pool or creates new one. </summary>
        T[,] NewArray<T>(int length1, int length2);

        /// <summary> Stores array in pool. </summary>
        void StoreArray<T>(T[,] array);

        /// <summary> Returns array from pool or creates new one. </summary>
        T[, ,] NewArray<T>(int length1, int length2, int length3);

        /// <summary> Stores array in pool. </summary>
        void StoreArray<T>(T[, ,] array);
        #endregion

        /// <summary>  Reduces internal buffers. </summary>
        void Shrink();
    }
}
