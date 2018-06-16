using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    // https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim(v=vs.110).aspx
    public class ConcurrentQueue<K>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Queue<K> cacheData = new Queue<K>();

        public int Count
        {
            get
            {
                using (_lock.Read())
                    return cacheData.Count;
            }
        }

        public K Dequeue()
        {
            using (_lock.Write())
               return cacheData.Dequeue();
        } 

        public void Enqueue(K value)
        {
            using (_lock.Write())
                cacheData.Enqueue(value);
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                cacheData.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        } 

        ~ConcurrentQueue()
        {
            if (_lock != null) _lock.Dispose();
        }
    }
}
