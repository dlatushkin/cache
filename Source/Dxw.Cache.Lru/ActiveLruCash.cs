namespace Dxw.Cache.Lru
{
    using System;
    using System.Timers;
    using Dxw.Core.Timers;
    using Dxw.Core.Times;

    public class ActiveLruCash<TKey, TItem> : LruCache<TKey, TItem>, IElapsedListener
    {
        private static readonly TimeSpan DefaultPurgeInterval = TimeSpan.FromSeconds(5);

        private readonly Timer timer = new Timer();

        public ActiveLruCash(
            ITimeSource timeSource,
            TimeSpan? purgeInterval = null,
            TimeSpan? defaultDuration = null,
            int? maxCapacity = null)
            : base(timeSource, defaultDuration, maxCapacity)
        {
            this.timer = new Timer
            {
                Interval = (purgeInterval ?? DefaultPurgeInterval).TotalMilliseconds
            };
            var weakEventManager = WeakEventManager.Register(this.timer, this);
            this.timer.Enabled = true;
        }

        public void Elapsed() => this.Purge();
    }
}
