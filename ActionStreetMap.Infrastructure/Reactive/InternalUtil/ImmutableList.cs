using System;

namespace ActionStreetMap.Infrastructure.Reactive.InternalUtil
{
    /// <summary />
    public class ImmutableList<T>
    {
        T[] data;
        /// <summary />
        public ImmutableList()
        {
            data = new T[0];
        }
        /// <summary />
        public ImmutableList(T[] data)
        {
            this.data = data;
        }
        /// <summary />
        public ImmutableList<T> Add(T value)
        {
            var newData = new T[data.Length + 1];
            Array.Copy(data, newData, data.Length);
            newData[data.Length] = value;
            return new ImmutableList<T>(newData);
        }
        /// <summary />
        public ImmutableList<T> Remove(T value)
        {
            var i = IndexOf(value);
            if (i < 0)
                return this;
            var newData = new T[data.Length - 1];
            Array.Copy(data, 0, newData, 0, i);
            Array.Copy(data, i + 1, newData, i, data.Length - i - 1);
            return new ImmutableList<T>(newData);
        }
        /// <summary />
        public int IndexOf(T value)
        {
            for (var i = 0; i < data.Length; ++i)
                if (data[i].Equals(value))
                    return i;
            return -1;
        }
        /// <summary />
        public T[] Data
        {
            get { return data; }
        }
    }
}