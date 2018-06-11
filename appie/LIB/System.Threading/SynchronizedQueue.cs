using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading
{
    public class SynchronizedQueue<T> : Queue<T>
    {
        readonly Object locker = new object();

        public new void Enqueue(T value)
        {
            lock (locker)
                base.Enqueue(value);
        }

        public new int Count
        {
            get
            {
                lock (locker)
                    return base.Count;
            }
        }

        public new T Dequeue()
        {
            lock (locker)
            {
                if (this.Count == 0)
                    return default(T);
                return this.Dequeue();
            }
        }
    }
}
