using System;

namespace ActionStreetMap.Infrastructure.Reactive.InternalUtil
{
    /// <summary />
    public class ListObserver<T> : IObserver<T>
    {
        private readonly ImmutableList<IObserver<T>> _observers;
        /// <summary />
        public ListObserver(ImmutableList<IObserver<T>> observers)
        {
            _observers = observers;
        }
        /// <summary />
        public void OnCompleted()
        {
            var targetObservers = _observers.Data;
            for (int i = 0; i < targetObservers.Length; i++)
            {
                targetObservers[i].OnCompleted();
            }
        }
        /// <summary />
        public void OnError(Exception error)
        {
            var targetObservers = _observers.Data;
            for (int i = 0; i < targetObservers.Length; i++)
            {
                targetObservers[i].OnError(error);
            }
        }
        /// <summary />
        public void OnNext(T value)
        {
            var targetObservers = _observers.Data;
            for (int i = 0; i < targetObservers.Length; i++)
            {
                targetObservers[i].OnNext(value);
            }
        }

        internal IObserver<T> Add(IObserver<T> observer)
        {
            return new ListObserver<T>(_observers.Add(observer));
        }

        internal IObserver<T> Remove(IObserver<T> observer)
        {
            var i = Array.IndexOf(_observers.Data, observer);
            if (i < 0)
                return this;

            if (_observers.Data.Length == 2)
            {
                return _observers.Data[1 - i];
            }
            else
            {
                return new ListObserver<T>(_observers.Remove(observer));
            }
        }
    }

    /// <summary />
    public class EmptyObserver<T> : IObserver<T>
    {
        // .Instance cause iOS AOT error
        // public static readonly EmptyObserver<T> Instance = new EmptyObserver<T>();
        /// <summary />
        public EmptyObserver()
        {

        }
        /// <summary />
        public void OnCompleted()
        {
        }
        /// <summary />
        public void OnError(Exception error)
        {
        }
        /// <summary />
        public void OnNext(T value)
        {
        }
    }
    /// <summary />
    public class DisposedObserver<T> : IObserver<T>
    {
        // .Instance cause iOS AOT error
        // public static readonly DisposedObserver<T> Instance = new DisposedObserver<T>();
        /// <summary />
        public DisposedObserver()
        {

        }
        /// <summary />
        public void OnCompleted()
        {
            throw new ObjectDisposedException("");
        }
        /// <summary />
        public void OnError(Exception error)
        {
            throw new ObjectDisposedException("");
        }
        /// <summary />
        public void OnNext(T value)
        {
            throw new ObjectDisposedException("");
        }
    }
}