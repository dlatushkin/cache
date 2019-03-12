namespace Dxw.Cache.Lru
{
    using System;

    /// <summary>
    /// Represents node of linked list to order items by relevance (by time of last touch)
    /// </summary>
    public class Node<TKey, TItem>
    {
        private DateTime expiresAt;

        public Node<TKey, TItem> Next { get; set; }

        public Node<TKey, TItem> Prev { get; set; }

        public TKey Key { get; set; }

        public TItem Value { get; set; }

        public TimeSpan? Duration { get; set; }

        public bool Expired(DateTime now) => this.expiresAt <= now;

        public void Touch(DateTime now, TimeSpan defaultDuration)
        {
            this.expiresAt = now.Add(this.Duration ?? defaultDuration);
        }
    }
}
