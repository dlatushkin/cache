namespace Dxw.Cache.Tests.BaseTests
{
    using System;
    using Dxw.Core.Times;
    using Moq;

    public abstract class BaseCacheTests<TKey, TITem>
    {
        protected Mock<ITimeSource> CreateTimeSourceMock() => new Mock<ITimeSource>();

        protected(Mock<ITimeSource> TimeMock, ICleanableCache<TKey, TITem> Cache) CreateICleanableCache(
                    TimeSpan? defaultDuration = null,
                    int? maxCapacity = null)
        {
            var timeMock = new Mock<ITimeSource>();
            var cache = this.CreateICleanableCache(timeMock.Object, defaultDuration, maxCapacity);
            return (timeMock,  cache);
        }

        protected abstract ICleanableCache<TKey, TITem> CreateICleanableCache(
            ITimeSource timeSource,
            TimeSpan? defaultDuration = null,
            int? maxCapacity = null);
    }
}
