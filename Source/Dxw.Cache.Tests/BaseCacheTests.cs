namespace Dxw.Cache.Tests
{
    using Dxw.Core.Times;
    using Moq;

    public abstract class BaseCacheTests<TKey, TITem>
    {
        public BaseCacheTests()
        {
            this.TimeSourceMock = new Mock<ITimeSource>();
        }

        protected Mock<ITimeSource> TimeSourceMock { get; }
    }
}
