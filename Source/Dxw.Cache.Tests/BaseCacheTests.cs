using Dxw.Core.Times;
using Moq;

namespace Dxw.Cache.Tests
{
    public abstract class BaseCacheTests<TKey, TITem>
    {
        protected readonly Mock<ITimeSource> _timeSourceMock;

        public BaseCacheTests()
        {
            _timeSourceMock = new Mock<ITimeSource>();
        }
    }
}
