using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Core
{
    public interface IPositionObserver<T>: IObserver<T>
    {
        T Current { get; }
    }
}
