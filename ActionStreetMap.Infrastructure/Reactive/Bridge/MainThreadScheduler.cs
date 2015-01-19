namespace ActionStreetMap.Infrastructure.Reactive
{
    public static partial class Scheduler
    {
        public static IScheduler MainThread = new CurrentThreadScheduler();
    }
}
