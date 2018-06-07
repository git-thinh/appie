using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    public class ConcurrentDictionary<K, V>
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<K, V> innerCache = new Dictionary<K, V>();

        public int Count
        { get { return innerCache.Count; } }

        public bool ContainsKey(K key)
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache.ContainsKey(key);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public Dictionary<K, V> GetDictionary()
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public K[] GetAllKeys()
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache.Keys.ToArray();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public V[] GetAllValues()
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache.Values.ToArray();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public V Read(K key)
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache[key];
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public void Add(K key, V value)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Add(key, value);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public bool AddWithTimeout(K key, V value, int timeout)
        {
            if (cacheLock.TryEnterWriteLock(timeout))
            {
                try
                {
                    innerCache.Add(key, value);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public AddOrUpdateStatus AddOrUpdate(K key, V value)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                //V result = null;
                V result = default(V);
                if (innerCache.TryGetValue(key, out result))
                {
                    //if (result == value)
                    if (result.Equals(value))
                    {
                        return AddOrUpdateStatus.Unchanged;
                    }
                    else
                    {
                        cacheLock.EnterWriteLock();
                        try
                        {
                            innerCache[key] = value;
                        }
                        finally
                        {
                            cacheLock.ExitWriteLock();
                        }
                        return AddOrUpdateStatus.Updated;
                    }
                }
                else
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        innerCache.Add(key, value);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                    return AddOrUpdateStatus.Added;
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }

        public void Delete(K key)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Remove(key);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public enum AddOrUpdateStatus
        {
            Added,
            Updated,
            Unchanged
        };

        ~ConcurrentDictionary()
        {
            if (cacheLock != null) cacheLock.Dispose();
        }
    }
}
