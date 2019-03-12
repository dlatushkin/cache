using Dxw.Core.Times;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dxw.Cache.Tests
{
    public abstract class BaseCacheTests<TKey, TITem>
    {
        //protected readonly IPurgeableCash<TKey, TITem> _expiringCache;
        protected readonly Mock<ITimeSource> _timeSourceMock;

        public BaseCacheTests()
        {
            _timeSourceMock = new Mock<ITimeSource>();
            //_expiringCache = CreateCache();
        }

        //protected abstract IPurgeableCash<TKey, TITem> CreateCache();
    }
}
