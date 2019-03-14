namespace Dxw.Cache.Tests.CacheTests
{
    using System;
    using Dxw.Cache.Tests.BaseTests;
    using Shouldly;
    using Xunit;

    public abstract class CacheStringExpirationTests : BaseCacheTests<string, string>
    {
        [Fact]
        public void DefaultDurationAddItemDefaultDuration_ItemExpires()
        {
            // Arrange
            var (timeMock, cache) = this.CreateICleanableCache();
            var now = default(DateTime);
            timeMock.Setup(s => s.GetNow()).Returns(() => now);

            // Act
            cache.Add("KeyA", "ValueA");
            cache.Purge();
            now = now.Add(DefaultDuration);
            cache.Purge();

            // Assert
            cache.TryGet("KeyA", out var val).ShouldBeFalse();
        }

        [Fact]
        public void CustomDurationAddItemDefaultDuration_ItemExpires()
        {
            // Arrange
            var customDuration = TimeSpan.FromSeconds(60);
            var (timeMock, cache) = this.CreateICleanableCache(customDuration);
            var now = default(DateTime);
            timeMock.Setup(s => s.GetNow()).Returns(() => now);

            // Act
            cache.Add("KeyA", "ValueA");

            // Assert
            var val = default(string);
            cache.Purge();
            cache.TryGet("KeyA", out val).ShouldBeTrue();
            now = now.Add(DefaultDuration);
            cache.Purge();
            cache.TryGet("KeyA", out val).ShouldBeTrue();
            now = now.Add(customDuration);
            cache.Purge();
            cache.TryGet("KeyA", out val).ShouldBeFalse();
        }

        [Fact]
        public void DefaultDurationAddItemCustomDuration_ItemExpires()
        {
            // Arrange
            var customDuration = TimeSpan.FromSeconds(60);
            var (timeMock, cache) = this.CreateICleanableCache();
            var now = default(DateTime);
            timeMock.Setup(s => s.GetNow()).Returns(() => now);

            // Act
            cache.Add("KeyA", "ValueA");
            cache.Add("KeyB", "ValueB", customDuration);

            // Assert
            var valA = default(string);
            var valB = default(string);
            cache.Purge();
            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeTrue(),
                () => valA.ShouldBe("ValueA"),
                () => cache.TryGet("KeyB", out valB).ShouldBeTrue(),
                () => valB.ShouldBe("ValueB"));

            now = now.Add(DefaultDuration);
            cache.Purge();
            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeFalse(),
                () => cache.TryGet("KeyB", out valB).ShouldBeTrue(),
                () => valB.ShouldBe("ValueB"));

            now = now.Add(customDuration);
            cache.Purge();
            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeFalse(),
                () => cache.TryGet("KeyB", out valB).ShouldBeFalse());
        }
    }
}
