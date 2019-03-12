using System;
using System.Collections.Generic;
using Dxw.Core.Times;

namespace Dxw.Cache.Lru
{
    /// <summary>
    /// LRU implementation of Thread-safe cache with limited number of items where elements are automatically removed if not accessed.
    /// </summary>
    public class LruExpiringCache<TKey, TItem>: IExpiringCache<TKey, TItem>
    {
        private readonly Dictionary<TKey, Node<TKey, TItem>> _entries = new Dictionary<TKey, Node<TKey, TItem>>();
        private readonly object _entriesLock = new object();
        private int _count;

        private readonly int _maxCapacity;
        private readonly TimeSpan _defaultDuration;
        private readonly ITimeSource _timeSource;

        private Node<TKey, TItem> _head;
        private Node<TKey, TItem> _tail;

        public LruExpiringCache(TimeSpan defaultDuration, int maxCapacity, ITimeSource timeSource)
        {
            _defaultDuration = defaultDuration;
            _maxCapacity = maxCapacity;
            _timeSource = timeSource;
        }

        public void Add(TKey key, TItem item) => TryAdd(key, item);

        public void Add(TKey key, TItem item, TimeSpan duration) => TryAdd(key, item, duration);

        public bool TryGet(TKey key, out TItem item)
        {
            item = default(TItem);

            if (!_entries.TryGetValue(key, out var node))
            {
                return false;
            }

            MoveToHead(node);

            item = node.Value;

            return true;
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        private void TryAdd(TKey key, TItem value, TimeSpan? duration = null)
        {
            Node<TKey, TItem> node;

            if (!_entries.TryGetValue(key, out node))
            {
                lock (_entriesLock)
                {
                    if (!_entries.TryGetValue(key, out node))
                    {
                        if (Full)
                        {
                            node = _tail;
                            _entries.Remove(_tail.Key);

                            node.Key = key;
                            node.Value = value;
                        }
                        else
                        {
                            _count++;
                            node = new Node<TKey, TItem>
                            {
                                Key = key,
                                Value = value
                            };
                        };

                        _entries.Add(key, node);
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

            MoveToHead(node);

            if (_tail == null)
            {
                _tail = _head;
            }
        }

        private void MoveToHead(Node<TKey, TItem> node)
        {
            node.Touch(_timeSource.GetNow(), _defaultDuration);

            if (node == _head)
            {
                return;
            }

            lock (_entriesLock)
            {
                RemoveFromLL(node);
                AddToHead(node);
            }
        }

        private void Purge()
        {
            if (_count == 0)
            {
                return;
            }

            lock (_entriesLock)
            {
                var current = _tail;
                var now = _timeSource.GetNow();
                while (current?.Expired(now) == true)
                {
                    Remove(current);
                    current = current.Prev;
                }
            }
        }

        private void AddToHead(Node<TKey, TItem> node)
        {
            node.Prev = null;
            node.Next = _head;

            if (_head != null)
            {
                _head.Prev = node;
            }

            _head = node;
        }

        private void RemoveFromLL(Node<TKey, TItem> node)
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

            if (node == _head)
            {
                _head = next;
            }

            if (node == _tail)
            {
                _tail = prev;
            }
        }

        private void Remove(Node<TKey, TItem> node)
        {
            RemoveFromLL(node);
            _entries.Remove(node.Key);
            _count--;
        }

        private bool Full => _count == _maxCapacity;
    }
}
