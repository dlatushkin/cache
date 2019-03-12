using Dxw.Cache.Lru;
using Dxw.Core.Times;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dxw.Cache.Tests
{
    public class ActiveCacheTests
    {
        [Fact]
        public async Task AddItem_PurgedAfterExpiration()
        {
            // Arrange
            var customDuration = TimeSpan.FromSeconds(1);
            IExpiringCache<string, string> cache =
                new ActiveLruCash<string, string>(
                    new DateTimeSource(),
                    purgeInterval: TimeSpan.FromMilliseconds(500),
                    defaultDuration: customDuration);

            // Act
            cache.Add("KeyA", "ValueA");

            // Assert
            var valA = default(string);
            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeTrue(),
                () => valA.ShouldBe("ValueA"));

            await Task.Delay(customDuration.Add(TimeSpan.FromSeconds(1)));

            cache.TryGet("KeyA", out valA).ShouldBeFalse();
        }
    }
}
