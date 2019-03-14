namespace Dxw.Cache.Lru
{
    using System;
    using System.Collections.Generic;
    using Dxw.Core.Times;

    /// <summary>
    /// LRU implementation of Thread-safe cache with limited number of items where elements are automatically removed if not accessed.
    /// </summary>
    public class LruCache<TKey, TItem> : ICleanableCache<TKey, TItem>
    {
        /// <summary>
        /// Lookup dictionary for fast search by key
        /// </summary>
        private readonly Dictionary<TKey, LinkedListNode<Node<TKey, TItem>>> entries =
            new Dictionary<TKey, LinkedListNode<Node<TKey, TItem>>>();

        private readonly object lockObj = new object();

        /// <summary>
        /// Linked list pointers (to order nodes by relevance)
        /// </summary>
        private LruList<TKey, TItem> usedList;

        public LruCache(
            ITimeSource timeSource,
            TimeSpan? defaultDuration = null,
            int? maxCapacity = null)
        {
            this.usedList = new LruList<TKey, TItem>(timeSource, defaultDuration, maxCapacity);
        }

        /// <summary>
        /// Adds (or updates) an element with default duration
        /// </summary>
        public void Add(TKey key, TItem item) => this.AddOrUpdate(key, item, null);

        /// <summary>
        /// Adds (or updates) an element with a specific duration overriding the default duration.
        /// </summary>
        public void Add(TKey key, TItem item, TimeSpan duration) => this.AddOrUpdate(key, item, duration);

        /// <summary>
        /// Gets an element with a specific key and resets its last-accessed property
        /// </summary>
        public bool TryGet(TKey key, out TItem item)
        {
            item = default(TItem);

            if (!this.entries.TryGetValue(key, out var node))
            {
                return false;
            }

            this.usedList.MoveToHead(node);

            item = node.Value.Value;

            return true;
        }

        /// <summary>
        /// Removes an element with a specific key
        /// </summary>
        public bool Remove(TKey key)
        {
            if (!this.entries.TryGetValue(key, out var node))
            {
                return false;
            }

            this.usedList.Remove(node);
            this.entries.Remove(node.Value.Key);
            return true;
        }

        /// <summary>
        /// Purges expired items.
        /// Linked list is ordered by expiration time by moving the most recently touched nodes to head
        /// hence the performance under lock is maximal.
        /// May be used in manualy or by timer event.
        /// </summary>
        public void Purge()
        {
            foreach (var key in this.usedList.RemoveExpired())
            {
                this.entries.Remove(key);
            }
        }

        /// <summary>
        /// Adds new element to cache.
        /// If cache is full the least relevant item is removed.
        /// If item with the same key exists it is overwritten.
        /// Element with given key is moved to the head.
        /// </summary>
        private void AddOrUpdate(TKey key, TItem value, TimeSpan? duration = null)
        {
            if (!this.entries.TryGetValue(key, out var node))
            {
                node = this.usedList.AddOrUpdate(key, value, duration, out var removedInfo);
                if (removedInfo.removed)
                {
                    this.entries.Remove(removedInfo.key);
                }

                this.entries.Add(key, node);
            }
            else
            {
                node.Value.Value = value;
                node.Value.Duration = duration;
            }

            this.usedList.MoveToHead(node);
        }
    }
}
