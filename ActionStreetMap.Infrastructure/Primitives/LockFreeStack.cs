using System;
using System.Threading;

namespace ActionStreetMap.Infrastructure.Primitives
{
    /// <summary> Lock free stack based on CAS pattern. </summary>
    /// <typeparam name="T"></typeparam>
    public class LockFreeStack<T>
    {
        private volatile Node _head;

        private class Node
        {
            internal T Value;
            internal volatile Node Next;
        }  
     
        /// <summary> Pushes value to stack. </summary>
        public void Push(T value)
        {
            var n = new Node {Value = value};
            Node h;
            do
            {
                h = _head;
                n.Next = h;
            } 
            while (Interlocked.CompareExchange(ref _head, n, h) != h);
        }

        /// <summary> Pops value from stack. </summary>
        /// <remarks> If stack is empty then returns default.</remarks>
        public T Pop()
        {
            Node n;
            do
            {
                n = _head;
                if (n == null) 
                    return default(T);
            } 
            while (Interlocked.CompareExchange(ref _head, n.Next, n) != n);
            return n.Value;
        }
    }
}
