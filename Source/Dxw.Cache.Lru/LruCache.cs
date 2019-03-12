namespace Dxw.Cache.Lru
{
    using System;
    using System.Collections.Generic;
    using Dxw.Core.Times;

    /// <summary>
    /// LRU implementation of Thread-safe cache with limited number of items where elements are automatically removed if not accessed.
    /// </summary>
    public class LruCache<TKey, TItem> : IPurgeableCash<TKey, TItem>
    {
        public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(30);
        public static readonly int DefaultMaxCapacity = 2;

        private readonly Dictionary<TKey, Node<TKey, TItem>> entries = new Dictionary<TKey, Node<TKey, TItem>>();
        private readonly object lockObj = new object();

        private readonly int maxCapacity;
        private readonly TimeSpan defaultDuration;
        private readonly ITimeSource timeSource;

        private int count;

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

        public void Add(TKey key, TItem item) => this.TryAdd(key, item);

        public void Add(TKey key, TItem item, TimeSpan duration) => this.TryAdd(key, item, duration);

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

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

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

        private void TryAdd(TKey key, TItem value, TimeSpan? duration = null)
        {
            Node<TKey, TItem> node;

            if (!this.entries.TryGetValue(key, out node))
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

        private void Remove(Node<TKey, TItem> node)
        {
            this.RemoveFromPosition(node);
            this.entries.Remove(node.Key);
            this.count--;
        }
    }
}
