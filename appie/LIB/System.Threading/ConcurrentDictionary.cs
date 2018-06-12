using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    // https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim(v=vs.110).aspx
    public class SynchronizedCacheString<K, V>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Dictionary<K, V> cacheData = new Dictionary<K, V>();

        public int Count
        {
            get
            { 
                using (_lock.Read())
                    return cacheData.Count;
            }
        }

        public void Remove(K key)
        {
            using(_lock.Write())
                if (cacheData.ContainsKey(key))
                    cacheData.Remove(key); 
        }

        public bool ContainsKey(K key)
        {
            _lock.EnterReadLock();
            try
            {
                return cacheData.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Dictionary<K, V> GetDictionary()
        {
            _lock.EnterReadLock();
            try
            {
                return cacheData;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public K[] GetAllKeys()
        {
            _lock.EnterReadLock();
            try
            {
                return cacheData.Keys.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public V[] GetAllValues()
        {
            _lock.EnterReadLock();
            try
            {
                return cacheData.Values.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public V Get(K key)
        {
            _lock.EnterReadLock();
            try
            {
                if (cacheData.ContainsKey(key)) return cacheData[key];
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return default(V);
        }

        public V this[K key]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    if (cacheData.ContainsKey(key))
                        return cacheData[key];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
                return default(V);
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    if (cacheData.ContainsKey(key))
                        cacheData[key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public void Add(K key, V value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!cacheData.ContainsKey(key)) cacheData.Add(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool AddWithTimeout(K key, V value, int timeout)
        {
            if (_lock.TryEnterWriteLock(timeout))
            {
                try
                {
                    if (!cacheData.ContainsKey(key)) cacheData.Add(key, value);
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

        public AddOrUpdateStatus AddOrUpdate(K key, V value)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                //V result = null;
                V result = default(V);
                if (cacheData.TryGetValue(key, out result))
                {
                    //if (result == value)
                    if (result.Equals(value))
                    {
                        return AddOrUpdateStatus.Unchanged;
                    }
                    else
                    {
                        _lock.EnterWriteLock();
                        try
                        {
                            cacheData[key] = value;
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }
                        return AddOrUpdateStatus.Updated;
                    }
                }
                else
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        cacheData.Add(key, value);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    return AddOrUpdateStatus.Added;
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void Delete(K key)
        {
            _lock.EnterWriteLock();
            try
            {
                if (cacheData.ContainsKey(key)) cacheData.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public enum AddOrUpdateStatus
        {
            Added,
            Updated,
            Unchanged
        };

        ~SynchronizedCacheString()
        {
            if (_lock != null) _lock.Dispose();
        }
    }
}
