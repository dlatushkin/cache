using Dxw.Cache.Lru;
using Shouldly;
using System;
using Xunit;

namespace Dxw.Cache.Tests
{
    public class LruStringBaseTests : BaseCacheTests<string, string>
    {
        [Fact]
        public void AddItem_GetSameItem()
        {
            // Arrange
            IExpiringCache<string, string> cache = new LruExpiringCache<string, string>(_timeSourceMock.Object);

            // Act
            cache.Add("KeyA", "ValueA");
            cache.Add("KeyB", "ValueB");

            // Assert
            var valA = default(string);
            var valB = default(string);
            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeTrue(),
                () => valA.ShouldBe("ValueA"),
                () => cache.TryGet("KeyB", out valB).ShouldBeTrue(),
                () => valB.ShouldBe("ValueB"));
        }

        [Fact]
        public void RewriteItem_GetRewritten()
        {
            // Arrange
            IExpiringCache<string, string> cache = new LruExpiringCache<string, string>(_timeSourceMock.Object);

            // Act
            cache.Add("KeyA", "ValueA");

            // Assert
            var valA = default(string);
            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeTrue(),
                () => valA.ShouldBe("ValueA"));

            cache.Add("KeyA", "ValueB");
            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeTrue(),
                () => valA.ShouldBe("ValueB"));
        }
    }
}
