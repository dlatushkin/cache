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
        public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(30);
        public static readonly int DefaultMaxCapacity = 2;

        /// <summary>
        /// Lookup dictionary for fast search by key
        /// </summary>
        private readonly Dictionary<TKey, LinkedListNode<Node<TKey, TItem>>> entries =
            new Dictionary<TKey, LinkedListNode<Node<TKey, TItem>>>();

        private readonly object lockObj = new object();

        private readonly int maxCapacity;
        private readonly TimeSpan defaultDuration;
        private readonly ITimeSource timeSource;

        /// <summary>
        /// Linked list pointers (to order nodes by relevance)
        /// </summary>
        private LinkedList<Node<TKey, TItem>> usedList = new LinkedList<Node<TKey, TItem>>();

        public LruCache(
            ITimeSource timeSource,
            TimeSpan? defaultDuration = null,
            int? maxCapacity = null)
        {
            this.timeSource = timeSource;
            this.defaultDuration = defaultDuration ?? DefaultDuration;
            this.maxCapacity = maxCapacity ?? DefaultMaxCapacity;
        }

        private bool Full => this.usedList.Count == this.maxCapacity;

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

            this.MoveToHead(node);

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

            this.Remove(node);
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
            if (this.usedList.First == null)
            {
                return;
            }

            lock (this.lockObj)
            {
                var current = this.usedList.Last;
                var now = this.timeSource.GetNow();
                while (current?.Value.Expired(now) == true)
                {
                    this.Remove(current);
                    current = current.Previous;
                }
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
                if (this.Full)
                {
                    node = this.usedList.Last;
                    this.entries.Remove(node.Value.Key);

                    node.Value.Key = key;
                    node.Value.Value = value;
                    node.Value.Duration = duration;
                }
                else
                {
                    node = this.usedList.AddFirst(new Node<TKey, TItem>
                    {
                        Key = key,
                        Value = value,
                        Duration = duration
                    });
                }

                this.entries.Add(key, node);
            }
            else
            {
                node.Value.Value = value;
                node.Value.Duration = duration;
            }

            this.MoveToHead(node);
        }

        /// <summary>
        /// Node is cut from its current position in the list and moved to the head.
        /// </summary>
        private void MoveToHead(LinkedListNode<Node<TKey, TItem>> node)
        {
            node.Value.Touch(this.timeSource.GetNow(), this.defaultDuration);

            if (node == this.usedList.First)
            {
                return;
            }

            this.usedList.Remove(node);
            this.usedList.AddFirst(node);
        }

        /// <summary>
        /// Removes node from both linked list and lookup dicionary.
        /// </summary>
        private void Remove(LinkedListNode<Node<TKey, TItem>> node)
        {
            this.usedList.Remove(node);
            this.entries.Remove(node.Value.Key);
        }
    }
}
