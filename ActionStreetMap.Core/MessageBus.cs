using System;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Core
{
    /// <summary>
    ///    Represens message bus to send messages between various components.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        ///     Sends message.
        /// </summary>
        /// <typeparam name="T">Message type.</typeparam>
        /// <param name="message">Message.</param>
        void Send<T>(T message);

        /// <summary>
        ///     Returns observable message pipe of given type
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Observable message pipe.</returns>
        IObservable<T> AsObservable<T>();
    }

    /// <summary>
    ///     Creates reactive extensions based message bus.
    /// </summary>
    public sealed class MessageBus: IMessageBus, IDisposable
    {
        private readonly Subject<object> _messageSubject = new Subject<object>();

        /// <inheritdoc />
        public void Send<T>(T message)
        {
            _messageSubject.OnNext(message);
        }

        /// <inheritdoc />
        public IObservable<T> AsObservable<T>()
        {
            return _messageSubject.OfType<object, T>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _messageSubject.Dispose();
        }
    }
}
