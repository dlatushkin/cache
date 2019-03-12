namespace Dxw.Cache
{
    using System;

    /// <summary>
    /// Thread-safe cache with limited number of items where elements are automatically removed if not accessed.
    /// </summary>
    public interface IExpiringCache<TKey, TItem>
    {
        /// <summary>
        /// Adds (or updates) an element with default duration
        /// </summary>
        void Add(TKey key, TItem item);

        /// <summary>
        /// Adds (or updates) an element with a specific duration overriding the default duration.
        /// </summary>
        void Add(TKey key, TItem item, TimeSpan duration);

        /// <summary>
        /// Gets an element with a specific key and resets its last-accessed property
        /// </summary>
        bool TryGet(TKey key, out TItem item);

        /// <summary>
        /// Removes an element with a specific key
        /// </summary>
        bool Remove(TKey key);
    }
}