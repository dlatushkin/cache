using System;

namespace Dxw.Cache.Lru
{
    public class Node<TKey, TItem>
    {
        private DateTime _expiresAt;

        public Node<TKey, TItem> Next { get; set; }

        public Node<TKey, TItem> Prev { get; set; }

        public TKey Key { get; set; }

        public TItem Value { get; set; }

        public TimeSpan? Duration { get; set; }

        public bool Expired(DateTime now) => _expiresAt <= now;

        public void Touch(DateTime now, TimeSpan defaultDuration)
        {
            _expiresAt = now.Add(Duration ?? defaultDuration);
        }
    }
}
