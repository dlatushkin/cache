namespace Dxw.Cache.Tests.CacheTests
{
    using System;
    using Dxw.Cache.Lru;
    using Dxw.Cache.Tests.BaseTests;
    using Dxw.Core.Times;
    using Shouldly;
    using Xunit;

    public abstract class CacheStringBaseTests : BaseCacheTests<string, string>
    {
        [Fact]
        public void AddItem_GetSameItem()
        {
            // Arrange
            var (timeMock, cache) = this.CreateICleanableCache();

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
            var (timeMock, cache) = this.CreateICleanableCache();

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
