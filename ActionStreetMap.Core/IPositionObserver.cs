using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Core
{
    /// <summary> Defines position observe interface. </summary>
    /// <typeparam name="T">Position type.</typeparam>
    public interface IPositionObserver<T>: IObserver<T>
    {
        /// <summary> Gets current position. </summary>
        T CurrentPosition { get; }
    }
}
