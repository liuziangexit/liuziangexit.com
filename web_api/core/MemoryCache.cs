using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/16/2019
 * 
 * MemoryCache
 *
 * TODO:
 * Basic CRUD
 * LRU policy
 *
 */

namespace WebApi.Core
{
    sealed class MemoryCache<KeyType, ValueType> where ValueType : class
    {

        //interface

        public bool Add(KeyType key, ValueType value)
        {
            if (!this.Memory.TryAdd(key, value))
                return false;
            this.AsyncRunner.Async(() =>
                addToSource(key, value));
            return true;
        }

        public bool Remove(KeyType key)
        {
            ValueType removedValue;
            if (!this.Memory.TryRemove(key, out removedValue))
                return false;
            this.AsyncRunner.Async(() =>
                removeFromSource(key));
            return true;
        }

        public ValueType GetValue(KeyType key)
        {
            ValueType value = null;
            if (!TryGetValue(key, out value))
                throw new KeyNotFoundException();
            return value;
        }

        public bool TryGetValue(KeyType key, out ValueType value)
        {
            if (!this.Memory.TryGetValue(key, out value))
            {
                var valueFromSource = getFromSource(key);
                if (valueFromSource != null)
                    this.Memory.AddOrUpdate(key, valueFromSource,
                    (KeyType arg0, ValueType arg1) => valueFromSource);
                value = valueFromSource;
            }
            return value != null;
        }

        public bool Update(KeyType key, ValueType newValue)
        {
            ValueType currentValue = null;
            if (!this.Memory.TryGetValue(key, out currentValue))
                return false;
            while (!this.Memory.TryUpdate(key, newValue, currentValue))
                if (!this.Memory.TryGetValue(key, out currentValue))
                    return false;
            this.AsyncRunner.Async(() =>
                updateToSource(key, newValue));
            return true;
        }

        public void Stop()
        {
            this.AsyncRunner.Stop();
        }

        public delegate void AddToSource(KeyType key, ValueType value);
        public delegate void RemoveFromSource(KeyType key);
        public delegate ValueType GetFromSource(KeyType key);
        public delegate void UpdateToSource(KeyType key, ValueType value);

        public AddToSource addToSource;
        public RemoveFromSource removeFromSource;
        public GetFromSource getFromSource;
        public UpdateToSource updateToSource;

        //implementation

        public ConcurrentDictionary<KeyType, ValueType> Memory = new ConcurrentDictionary<KeyType, ValueType>();
        private FixedThreadPool AsyncRunner = new FixedThreadPool(1);

    }
}
