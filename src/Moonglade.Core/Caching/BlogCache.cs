﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Moonglade.Core.Caching
{
    public enum CacheDivision
    {
        General,
        Post,
        Page,
        PostCountCategory
    }

    public class BlogCache : IBlogCache
    {
        /* Create Key-Value mapping for cache divisions to workaround
         * https://github.com/aspnet/Caching/issues/422
         * This blog will need cache keys for post ids or category ids and need to be cleared later
         * Key               | Value
         * ------------------+--------------------------------------
         * PostCountCategory | { "<guid>", "<guid>", ... }
         * Post              | { "<guid>", "<guid>", "<guid"> ... }
         * General           | { "avatar", ... }
         */
        public Dictionary<string, IList<string>> CacheDivision { get; }

        private readonly IMemoryCache _memoryCache;

        public BlogCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            CacheDivision = new Dictionary<string, IList<string>>();
        }

        public TItem GetOrCreate<TItem>(CacheDivision division, string key, Func<ICacheEntry, TItem> factory)
        {
            AddToDivision(division.ToString(), key);
            return _memoryCache.GetOrCreate(key, factory);
        }

        public Task<TItem> GetOrCreateAsync<TItem>(CacheDivision division, string key, Func<ICacheEntry, Task<TItem>> factory)
        {
            AddToDivision(division.ToString(), key);
            return _memoryCache.GetOrCreateAsync($"{division}-{key}", factory);
        }

        public void RemoveAllCache()
        {
            var keys =
                from kvp in CacheDivision
                let prefix = kvp.Key
                from val in kvp.Value
                select $"{prefix}-{val}";

            foreach (string key in keys)
            {
                _memoryCache.Remove(key);
            }
        }

        public void Remove(CacheDivision division)
        {
            var cacheKeys = CacheDivision[division.ToString()];
            if (null == cacheKeys || cacheKeys.Count <= 0) return;

            foreach (string key in cacheKeys)
            {
                _memoryCache.Remove(key);
            }
        }

        public void Remove(CacheDivision division, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            _memoryCache.Remove($"{division}-{key}");
        }

        private void AddToDivision(string divisionKey, string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(divisionKey) || string.IsNullOrWhiteSpace(cacheKey)) return;

            if (!CacheDivision.ContainsKey(divisionKey))
            {
                CacheDivision.Add(divisionKey, new[] { cacheKey }.ToList());
            }

            if (!CacheDivision[divisionKey].Contains(cacheKey))
            {
                CacheDivision[divisionKey].Add(cacheKey);
            }
        }
    }
}