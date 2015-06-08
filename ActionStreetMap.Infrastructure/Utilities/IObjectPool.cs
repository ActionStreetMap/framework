using System;
using System.Collections.Generic;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary> Represents object pool which is used to reduced amount of memory allocations. </summary>
    public interface IObjectPool
    {
        #region Objects

        /// <summary> Returns object from pool. </summary>
        T NewObject<T>();

        /// <summary> Stores object to pool. </summary>
        void StoreObject<T>(T instance);

        #endregion

        #region Lists
        /// <summary> Returns list from pool or creates new one. </summary>
        List<T> NewList<T>();

        /// <summary> Returns list from pool or creates new one. </summary>
        List<T> NewList<T>(int capacity);

        /// <summary> Stores list in pool. </summary>
        void StoreList<T>(List<T> list, bool isClean = false);
        #endregion

        /// <summary> Initializes internal data structure for given type to speed up lookup. </summary>
        IObjectPool RegisterObjectType<T>(Func<T> factoryMethod);

        /// <summary> Initializes internal data structure for given type to speed up lookup. </summary>
        IObjectPool RegisterListType<T>(int capacity);

        /// <summary> Reduces internal buffers. </summary>
        void Shrink();
    }
}
