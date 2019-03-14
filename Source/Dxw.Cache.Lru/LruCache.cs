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
        private readonly Dictionary<TKey, Node<TKey, TItem>> entries = new Dictionary<TKey, Node<TKey, TItem>>();
        private readonly object lockObj = new object();

        private readonly int maxCapacity;
        private readonly TimeSpan defaultDuration;
        private readonly ITimeSource timeSource;

        private int count;

        /// <summary>
        /// Linked list pointers (to order nodes by relevance)
        /// </summary>
        private Node<TKey, TItem> head;
        private Node<TKey, TItem> tail;

        public LruCache(
            ITimeSource timeSource,
            TimeSpan? defaultDuration = null,
            int? maxCapacity = null)
        {
            this.timeSource = timeSource;
            this.defaultDuration = defaultDuration ?? DefaultDuration;
            this.maxCapacity = maxCapacity ?? DefaultMaxCapacity;
        }

        private bool Full => this.count == this.maxCapacity;

        /// <summary>
        /// Adds (or updates) an element with default duration
        /// </summary>
        public void Add(TKey key, TItem item) => this.TryAdd(key, item);

        /// <summary>
        /// Adds (or updates) an element with a specific duration overriding the default duration.
        /// </summary>
        public void Add(TKey key, TItem item, TimeSpan duration) => this.TryAdd(key, item, duration);

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

            item = node.Value;

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
            if (this.count == 0)
            {
                return;
            }

            lock (this.lockObj)
            {
                var current = this.tail;
                var now = this.timeSource.GetNow();
                while (current?.Expired(now) == true)
                {
                    this.Remove(current);
                    current = current.Prev;
                }
            }
        }

        /// <summary>
        /// Adds new element to cache.
        /// If cache is full the least relevant item is removed.
        /// If item with the same key exists it is overwritten.
        /// Element with given key is moved to the head.
        /// </summary>
        private void TryAdd(TKey key, TItem value, TimeSpan? duration = null)
        {
            if (!this.entries.TryGetValue(key, out var node))
            {
                lock (this.lockObj)
                {
                    if (!this.entries.TryGetValue(key, out node))
                    {
                        if (this.Full)
                        {
                            node = this.tail;
                            this.entries.Remove(this.tail.Key);

                            node.Key = key;
                            node.Value = value;
                            node.Duration = duration;
                        }
                        else
                        {
                            this.count++;
                            node = new Node<TKey, TItem>
                            {
                                Key = key,
                                Value = value,
                                Duration = duration
                            };
                        }

                        this.entries.Add(key, node);
                    }
                }
            }
            else
            {
                lock (node)
                {
                    node.Value = value;
                }
            }

            this.MoveToHead(node);

            if (this.tail == null)
            {
                this.tail = this.head;
            }
        }

        /// <summary>
        /// Node is cut from its current position in the list and moved to the head.
        /// </summary>
        private void MoveToHead(Node<TKey, TItem> node)
        {
            node.Touch(this.timeSource.GetNow(), this.defaultDuration);

            if (node == this.head)
            {
                return;
            }

            lock (this.lockObj)
            {
                this.RemoveFromPosition(node);
                this.AddToHead(node);
            }
        }

        /// <summary>
        /// Node is inserted to the very beginning (head) of the list
        /// </summary>
        private void AddToHead(Node<TKey, TItem> node)
        {
            node.Prev = null;
            node.Next = this.head;

            if (this.head != null)
            {
                this.head.Prev = node;
            }

            this.head = node;
        }

        /// <summary>
        /// Cuts given node from its current position and sets sibling pointers accordingly
        /// </summary>
        private void RemoveFromPosition(Node<TKey, TItem> node)
        {
            var next = node.Next;
            var prev = node.Prev;

            if (next != null)
            {
                next.Prev = node.Prev;
            }

            if (prev != null)
            {
                prev.Next = node.Next;
            }

            if (node == this.head)
            {
                this.head = next;
            }

            if (node == this.tail)
            {
                this.tail = prev;
            }
        }

        /// <summary>
        /// Removes node from both linked list and lookup dicionary.
        /// </summary>
        private void Remove(Node<TKey, TItem> node)
        {
            this.RemoveFromPosition(node);
            this.entries.Remove(node.Key);
            this.count--;
        }
    }
}
