
namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public interface ISubject<TSource, TResult> : IObserver<TSource>, IObservable<TResult>
    {
    }
    /// <summary />
    public interface ISubject<T> : ISubject<T, T>, IObserver<T>, IObservable<T>
    {
    }
}