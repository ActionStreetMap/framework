using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public class MultipleAssignmentDisposable : IDisposable, ICancelable
    {
        static readonly BooleanDisposable True = new BooleanDisposable(true);

        object gate = new object();
        IDisposable current;
        /// <summary />
        public bool IsDisposed
        {
            get
            {
                lock (gate)
                {
                    return current == True;
                }
            }
        }
        /// <summary />
        public IDisposable Disposable
        {
            get
            {
                lock (gate)
                {
                    return (current == True)
                        ? ActionStreetMap.Infrastructure.Reactive.Disposable.Empty
                        : current;
                }
            }
            set
            {
                var shouldDispose = false;
                lock (gate)
                {
                    shouldDispose = (current == True);
                    if (!shouldDispose)
                    {
                        current = value;
                    }
                }
                if (shouldDispose && value != null)
                {
                    value.Dispose();
                }
            }
        }
        /// <summary />
        public void Dispose()
        {
            IDisposable old = null;

            lock (gate)
            {
                if (current != True)
                {
                    old = current;
                    current = True;
                }
            }

            if (old != null) old.Dispose();
        }
    }
}