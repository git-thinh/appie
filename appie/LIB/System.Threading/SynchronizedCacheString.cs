using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace appie
{
    // https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim(v=vs.110).aspx
    public class SynchronizedCacheString
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Dictionary<string, string> cacheData = new Dictionary<string, string>();

        public int Count
        {
            get
            {
                using (_lock.Read())
                    return cacheData.Count;
            }
        }

        public void Remove(string key)
        {
            using (_lock.Write())
                if (cacheData.ContainsKey(key))
                    cacheData.Remove(key);
        }

        public bool ContainsKey(string key)
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

        public string Get(string key)
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
            return null;
        }

        public bool TryGetValue(string key, ref string value)
        {
            _lock.EnterReadLock();
            try
            {
                if (cacheData.ContainsKey(key))
                {
                    value = cacheData[key];
                    return true;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return false;
        }

        public string[] Keys
        {
            get
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
        }

        public void ReadFile(string file_path)
        {
            _lock.EnterWriteLock();
            try
            {
                if (File.Exists(file_path))
                {
                    using (var file = File.OpenRead(file_path))
                    {
                        cacheData = ProtoBuf.Serializer.Deserialize<Dictionary<string, string>>(file);
                        file.Close();
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveValueEmpty()
        {
            _lock.EnterWriteLock();
            try
            {
                cacheData = cacheData.Where(x => !string.IsNullOrEmpty(x.Value)).ToDictionary(x => x.Key, x => x.Value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void WriteFile(string file_path, bool clean_data_after_write = false)
        {
            _lock.EnterWriteLock();
            try
            {
                if (File.Exists(file_path))
                {
                    // Using Protobuf-net, I suddenly got an exception about an unknown wire-type
                    // https://stackoverflow.com/questions/2152978/using-protobuf-net-i-suddenly-got-an-exception-about-an-unknown-wire-type
                    using (var file = new FileStream(file_path, FileMode.Truncate))
                    {
                        // write
                        ProtoBuf.Serializer.Serialize<Dictionary<string, string>>(file, cacheData);
                        // SetLength after writing your data:
                        // file.SetLength(file.Position);
                    }
                }
                else
                {
                    using (var file = new FileStream(file_path, FileMode.OpenOrCreate))
                        ProtoBuf.Serializer.Serialize<Dictionary<string, string>>(file, cacheData);
                }

                if (clean_data_after_write) cacheData.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Add(string key, string value)
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

        public bool AddWithTimeout(string key, string value, int timeout)
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

        public AddOrUpdateStatus AddOrUpdate(string key, string value)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                string result = null;
                if (cacheData.TryGetValue(key, out result))
                {
                    if (result == value)
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

        public void Delete(string key)
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
