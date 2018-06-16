using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading
{
    public class SynchronizedQueue<T> 
    {
        readonly Object locker = new object();
        Queue<T> queue = new Queue<T>();

        public new void Enqueue(T value)
        {
            lock (locker)
                queue.Enqueue(value);
        }

        public void EnqueueItems(T[] values)
        {
            lock (locker)
                for (int i = 0; i < values.Length; i++)
                    queue.Enqueue(values[i]);
        }

        public new int Count
        {
            get
            {
                lock (locker)
                    return queue.Count;
            }
        }

        public new T Dequeue()
        {
            lock (locker)
            {
                if (this.Count == 0)
                    return default(T);
                return queue.Dequeue();
            }
        }
    }
}
