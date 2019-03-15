namespace Dxw.Cache.Lru
{
    using System;
    using System.Diagnostics;
    using System.Timers;
    using Dxw.Core.Timers;
    using Dxw.Core.Times;

    /// <summary>
    /// Cache with automatic purging by timer
    /// </summary>
    public class ActiveLruCache<TKey, TItem> : LruCache<TKey, TItem>, IElapsedListener
    {
        private static readonly TimeSpan DefaultPurgeInterval = TimeSpan.FromSeconds(5);

        private readonly Timer timer = new Timer();

        public ActiveLruCache(
            ITimeSource timeSource,
            TimeSpan? purgeInterval = null,
            TimeSpan? defaultDuration = null,
            int? maxCapacity = null)
            : base(timeSource, defaultDuration, maxCapacity)
        {
            this.timer = new Timer
            {
                AutoReset = false,
                Interval = (purgeInterval ?? DefaultPurgeInterval).TotalMilliseconds
            };
            var weakEventManager = WeakEventManager.Register(this.timer, this);
            this.timer.Start();
        }

        public void Elapsed() => this.Cleanup();
    }
}
