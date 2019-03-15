namespace Dxw.Cache.Lru
{
    using System;
    using System.Collections.Generic;
    using Dxw.Core.Times;

    /// <summary>
    /// LRU list implementation
    /// </summary>
    internal class LruList<TKey, TItem>
    {
        public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(30);
        public static readonly int DefaultMaxCapacity = 2;

        private readonly ITimeSource timeSource;
        private readonly TimeSpan defaultDuration;
        private readonly int maxCapacity;

        private readonly LinkedList<Slot<TKey, TItem>> linkedList = new LinkedList<Slot<TKey, TItem>>();

        public LruList(
            ITimeSource timeSource,
            TimeSpan? defaultDuration = null,
            int? maxCapacity = null)
        {
            this.timeSource = timeSource;
            this.defaultDuration = defaultDuration ?? DefaultDuration;
            this.maxCapacity = maxCapacity ?? DefaultMaxCapacity;
        }

        public bool Empty => this.linkedList.First == null;

        public bool Full => this.linkedList.Count == this.maxCapacity;

        public LinkedListNode<Slot<TKey, TItem>> AddOrUpdate(
            TKey key,
            TItem value,
            TimeSpan? duration,
            out (bool removed, TKey key) removedInfo)
        {
            LinkedListNode<Slot<TKey, TItem>> node;

            if (this.Full)
            {
                node = this.linkedList.Last;
                removedInfo = (true, node.Value.Key);

                node.Value.Key = key;
                node.Value.Value = value;
                node.Value.Duration = duration;
            }
            else
            {
                node = this.linkedList.AddFirst(new Slot<TKey, TItem>
                {
                    Key = key,
                    Value = value,
                    Duration = duration
                });
                removedInfo = (false, default(TKey));
            }

            return node;
        }

        public void MoveToHead(LinkedListNode<Slot<TKey, TItem>> node)
        {
            node.Value.Touch(this.timeSource.GetNow(), this.defaultDuration);

            if (node == this.linkedList.First)
            {
                return;
            }

            this.linkedList.Remove(node);
            this.linkedList.AddFirst(node);
        }

        public void Remove(LinkedListNode<Slot<TKey, TItem>> node) => this.linkedList.Remove(node);

        public IEnumerable<TKey> RemoveExpired()
        {
            if (!this.Empty)
            {
                var now = this.timeSource.GetNow();
                var current = this.linkedList.Last;
                while (current?.Value.Expired(now) == true)
                {
                    this.linkedList.Remove(current);
                    yield return current.Value.Key;
                    current = current.Previous;
                }
            }
        }
    }
}
