using System;
using ActionStreetMap.Infrastructure.Primitives;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary> ObjectTypePool, naive implementation. </summary>
    internal class ObjectTypePool<T>
    {
        private readonly Func<T> _factoryMethod;
        private readonly LockFreeStack<T> _objectStack;

        /// <summary> Creates <see cref="ObjectTypePool{T}"/>. </summary>
        /// <param name="factoryMethod">Factory method.</param>
        public ObjectTypePool(Func<T> factoryMethod)
        {
            _objectStack = new LockFreeStack<T>();
            _factoryMethod = factoryMethod;
        }

        public T New()
        {
            var value = _objectStack.Pop();
            return value != null ? value : _factoryMethod.Invoke();
        }

        public void Store(T instance)
        {
            _objectStack.Push(instance);
        }
    }
}
