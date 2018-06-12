using System.Collections.Generic;
using System.Linq;

// SmartThreadPool
namespace System.Threading
{
    internal class SynchronizedDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly object _lock;

        public SynchronizedDictionary()
        {
            _lock = new object();
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public int Count
        {
            get { 
                lock (_lock)
                    return _dictionary.Count;
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_lock)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        public void Remove(TKey key)
        {
            lock (_lock)
            {
                if (_dictionary.ContainsKey(key))
                    _dictionary.Remove(key);
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (!_dictionary.ContainsKey(key))
                    _dictionary.Add(key, value);
            }
        }

        public object SyncRoot
        {
            get { return _lock; }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_lock)
                {
                    if (_dictionary.ContainsKey(key))
                        return _dictionary[key];
                    return default(TValue);
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_dictionary.ContainsKey(key))
                        _dictionary[key] = value;
                }
            }
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary.Keys;
                }
            }
        }

        public TKey[] KeysArray
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary.Keys.ToArray();                        ;
                }
            }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary.Values;
                }
            }
        }
        public void Clear()
        {
            lock (_lock)
            {
                _dictionary.Clear();
            }
        }
    }
}
