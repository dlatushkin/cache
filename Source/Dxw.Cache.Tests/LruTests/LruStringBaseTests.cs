namespace Dxw.Cache.Tests.LruTests
{
    using System;
    using Dxw.Cache.Lru;
    using Dxw.Cache.Tests.CacheTests;
    using Dxw.Core.Times;

    public class LruStringBaseTests : CacheStringBaseTests
    {
        protected override ICleanableCache<string, string> CreateICleanableCache(
            ITimeSource timeSource,
            TimeSpan? defaultDuration = null,
            int? maxCapacity = null) => new LruCache<string, string>(timeSource, defaultDuration, maxCapacity);
    }
}
