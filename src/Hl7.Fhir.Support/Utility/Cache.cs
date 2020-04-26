/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Utility
{
    public class Cache<K, V>
    {
        private readonly ConcurrentDictionary<K, CacheItem<V>> _cached;
        private readonly int _minimumCacheSize;
        private readonly CacheSettings _settings;
        private readonly Func<K, CacheItem<V>> _retriever;

        public Cache(Func<K, V> retrieveFunction) : this(retrieveFunction, CacheSettings.CreateDefault())  { }

        public Cache(Func<K, V> retrieveFunction, CacheSettings settings)
        {
            if (settings is null) throw new ArgumentNullException(nameof(settings));

            _cached = new ConcurrentDictionary<K, CacheItem<V>>();
            _retriever = retrieveFunction != null ? 
                key => new CacheItem<V>(retrieveFunction(key))
                : default(Func<K,CacheItem<V>>);
            _settings = settings.Clone();
            _minimumCacheSize = (int)Math.Floor(_settings.MaxCacheSize * 0.9);
        }

        public V GetValue(K key)
        {
            var cachedItem = _retriever != null ?
                _cached.GetOrAdd(key, _retriever)
                : _cached.GetOrAdd(key, default(CacheItem<V>));
            enforceMaxItems();

            return cachedItem.Value;
        }

        public V GetValueOrAdd(K key, V value)
        {
            var cachedItem = _cached.GetOrAdd(key, new CacheItem<V>(value));
            enforceMaxItems();

            return cachedItem.Value;
        }

        private void enforceMaxItems()
        {
            var currentCount = _cached.Count();
            if (currentCount > _settings.MaxCacheSize)
            {
                // first copy the key value pairs in an array. Otherwise we could have a race condition. See for more information:
                // https://stackoverflow.com/questions/11692389/getting-argument-exception-in-concurrent-dictionary-when-sorting-and-displaying
                var copy = _cached.ToArray();
                var oldestItems = copy.OrderByDescending(entry => entry.Value.LastAccessed).Skip(_minimumCacheSize);
                foreach (var item in oldestItems)
                {
                    _cached.TryRemove(item.Key, out _);
                }
            }
        }
    }

    internal class CacheItem<V>
    {
        public CacheItem(V value) => _value = value;

        public DateTimeOffset LastAccessed { get; private set; } = DateTimeOffset.Now;

        private V _value;
        internal V Value
        {
            get
            {
                LastAccessed = DateTimeOffset.Now;
                return _value;
            }
            set
            {
                _value = value;
                if (value == null)
                {
                    throw new ArgumentException($"Do not set the {nameof(Value)} to null.");
                }
            }
        }
    }

    public class CacheSettings
    {
        public int MaxCacheSize { get; set; } = 500;
        public static CacheSettings CreateDefault() => new CacheSettings();

        public CacheSettings() { }

        public CacheSettings(CacheSettings other)
        {
            if (other == null) throw Error.ArgumentNull(nameof(other));
            other.CopyTo(this);
        }

        private void CopyTo(CacheSettings other)
        {
            if (other == null) throw Error.ArgumentNull(nameof(other));
            other.MaxCacheSize = MaxCacheSize;
        }

        public CacheSettings Clone() => new CacheSettings(this);
    }

}