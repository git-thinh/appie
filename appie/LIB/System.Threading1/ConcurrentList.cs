using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    // https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim(v=vs.110).aspx
    public class ConcurrentList<K>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private List<K> cacheData = new List<K>();

        public int Count
        {
            get
            {
                using (_lock.Read())
                    return cacheData.Count;
            }
        }

        public K[] ToArray()
        {
            using (_lock.Read())
                return cacheData.ToArray();
        }

        public void RemoveAt(int index)
        {
            using (_lock.Write())
                if (index >= 0 && index < cacheData.Count)
                    cacheData.RemoveAt(index);
        }

        public void Truncate(Func<K, bool> predicate = null, bool distinct = false)
        {
            _lock.EnterWriteLock();
            try
            {
                if (predicate == null)
                {
                    if (distinct)
                        cacheData = cacheData.Distinct().ToList();
                }
                else
                {
                    if (distinct)
                        cacheData = cacheData.Where(predicate).ToList();
                    else
                        cacheData = cacheData.Where(predicate).Distinct().ToList();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public K[] Where(Func<K, bool> predicate, bool distinct = false, int take = 0)
        {
            _lock.EnterReadLock();
            try
            {
                if (distinct)
                {
                    if (take > 0)
                        return cacheData.Where(predicate).Take(take).ToArray();
                    else
                        return cacheData.Where(predicate).ToArray();
                }
                else
                {
                    if (take > 0)
                        return cacheData.Where(predicate).Distinct().Take(take).ToArray();
                    else
                        return cacheData.Where(predicate).Distinct().ToArray();
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public K[] Take(int take)
        {
            _lock.EnterReadLock();
            try
            {
                if (take > 0)
                    return cacheData.Take(take).ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return new K[] { };
        }
        public K[] Slice(int number)
        {
            _lock.EnterReadLock();
            try
            {
                if (number > 0)
                {
                    K[] a = cacheData.Take(number).ToArray();
                    cacheData = cacheData.Where(x => !a.Any(o => o.Equals(x))).ToList();
                    return a;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return new K[] { };
        }

        public bool Contain(K key)
        {
            _lock.EnterReadLock();
            try
            {
                return cacheData.IndexOf(key) != -1;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public K this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    if (index < cacheData.Count && index >= 0)
                        return cacheData[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
                return default(K);
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    if (index < cacheData.Count && index >= 0)
                        cacheData[index] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
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

        public void Add(K key)
        {
            _lock.EnterWriteLock();
            try
            {
                cacheData.Add(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddRange(K[] keys, bool distinct = false)
        {
            _lock.EnterWriteLock();
            try
            {
                cacheData.AddRange(keys);
                if (distinct)
                    cacheData = cacheData.Distinct().ToList();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddRange(List<K> keys)
        {
            _lock.EnterWriteLock();
            try
            {
                cacheData.AddRange(keys);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool AddWithTimeout(K key, int timeout)
        {
            if (_lock.TryEnterWriteLock(timeout))
            {
                try
                {
                    cacheData.Add(key);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Remove(K key)
        {
            _lock.EnterWriteLock();
            try
            {
                int pos = cacheData.IndexOf(key);
                if (pos != -1)
                    cacheData.RemoveAt(pos);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        //public void RemoveAt(int index)
        //{
        //    _lock.EnterWriteLock();
        //    try
        //    {
        //        if (index >= 0 && index < cacheData.Count)
        //            cacheData.RemoveAt(index);
        //    }
        //    finally
        //    {
        //        _lock.ExitWriteLock();
        //    }
        //}

        public enum AddOrUpdateStatus
        {
            Added,
            Updated,
            Unchanged
        };

        ~ConcurrentList()
        {
            if (_lock != null) _lock.Dispose();
        }
    }
}
