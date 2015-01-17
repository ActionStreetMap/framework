using System;

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public interface IScheduler
    {
        /// <summary />
        DateTimeOffset Now { get; }
        
        // interface is changed from official Rx for avoid iOS AOT problem(state is dangerous).
        /// <summary />
        IDisposable Schedule(Action action);
        /// <summary />
        IDisposable Schedule(TimeSpan dueTime, Action action);
    }

    // Scheduler Extension
    public static partial class Scheduler
    {
        // configurable defaults
        /// <summary />
        public static class DefaultSchedulers
        {
            static IScheduler constantTime;
            /// <summary />
            public static IScheduler ConstantTimeOperations
            {
                get
                {
                    return constantTime ?? (constantTime = Scheduler.Immediate);
                }
                set
                {
                    constantTime = value;
                }
            }

            static IScheduler tailRecursion;
            /// <summary />
            public static IScheduler TailRecursion
            {
                get
                {
                    return tailRecursion ?? (tailRecursion = Scheduler.Immediate);
                }
                set
                {
                    tailRecursion = value;
                }
            }

            static IScheduler iteration;
            /// <summary />
            public static IScheduler Iteration
            {
                get
                {
                    return iteration ?? (iteration = Scheduler.CurrentThread);
                }
                set
                {
                    iteration = value;
                }
            }

            static IScheduler timeBasedOperations;
            /// <summary />
            public static IScheduler TimeBasedOperations
            {
                get
                {
                    return timeBasedOperations ?? (timeBasedOperations = Scheduler.CurrentThread); // MainThread as default for TimeBased Operation
                }
                set
                {
                    timeBasedOperations = value;
                }
            }

            static IScheduler asyncConversions;
            /// <summary />
            public static IScheduler AsyncConversions
            {
                get
                {
                    return asyncConversions ?? (asyncConversions = Scheduler.ThreadPool);
                }
                set
                {
                    asyncConversions = value;
                }
            }

            /// <summary />
            public static void SetDotNetCompatible()
            {
                ConstantTimeOperations = Scheduler.Immediate;
                TailRecursion = Scheduler.Immediate;
                Iteration = Scheduler.CurrentThread;
                TimeBasedOperations = Scheduler.ThreadPool;
                AsyncConversions = Scheduler.ThreadPool;
            }
        }

        // utils
        /// <summary />
        public static DateTimeOffset Now
        {
            get { return DateTimeOffset.UtcNow; }
        }
        /// <summary />
        public static TimeSpan Normalize(TimeSpan timeSpan)
        {
            return timeSpan >= TimeSpan.Zero ? timeSpan : TimeSpan.Zero;
        }
        /// <summary />
        public static IDisposable Schedule(this IScheduler scheduler, DateTimeOffset dueTime, Action action)
        {
            return scheduler.Schedule(dueTime - scheduler.Now, action);
        }
        /// <summary />
        public static IDisposable Schedule(this IScheduler scheduler, Action<Action> action)
        {
            // InvokeRec1
            var group = new CompositeDisposable(1);
            var gate = new object();

            Action recursiveAction = null;
            recursiveAction = () => action(() =>
            {
                var isAdded = false;
                var isDone = false;
                var d = default(IDisposable);
                d = scheduler.Schedule(() =>
                {
                    lock (gate)
                    {
                        if (isAdded)
                            group.Remove(d);
                        else
                            isDone = true;
                    }
                    recursiveAction();
                });

                lock (gate)
                {
                    if (!isDone)
                    {
                        group.Add(d);
                        isAdded = true;
                    }
                }
            });

            group.Add(scheduler.Schedule(recursiveAction));

            return group;
        }

        /// <summary />
        public static IDisposable Schedule(this IScheduler scheduler, TimeSpan dueTime, Action<Action<TimeSpan>> action)
        {
            // InvokeRec2

            var group = new CompositeDisposable(1);
            var gate = new object();

            Action recursiveAction = null;
            recursiveAction = () => action(dt =>
            {
                var isAdded = false;
                var isDone = false;
                var d = default(IDisposable);
                d = scheduler.Schedule(dt, () =>
                {
                    lock (gate)
                    {
                        if (isAdded)
                            group.Remove(d);
                        else
                            isDone = true;
                    }
                    recursiveAction();
                });

                lock (gate)
                {
                    if (!isDone)
                    {
                        group.Add(d);
                        isAdded = true;
                    }
                }
            });

            group.Add(scheduler.Schedule(dueTime, recursiveAction));

            return group;
        }

        /// <summary />
        public static IDisposable Schedule(this IScheduler scheduler, DateTimeOffset dueTime, Action<Action<DateTimeOffset>> action)
        {
            // InvokeRec3

            var group = new CompositeDisposable(1);
            var gate = new object();

            Action recursiveAction = null;
            recursiveAction = () => action(dt =>
            {
                var isAdded = false;
                var isDone = false;
                var d = default(IDisposable);
                d = scheduler.Schedule(dt, () =>
                {
                    lock (gate)
                    {
                        if (isAdded)
                            group.Remove(d);
                        else
                            isDone = true;
                    }
                    recursiveAction();
                });

                lock (gate)
                {
                    if (!isDone)
                    {
                        group.Add(d);
                        isAdded = true;
                    }
                }
            });

            group.Add(scheduler.Schedule(dueTime, recursiveAction));

            return group;
        }
    }
}