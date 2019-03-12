namespace Dxw.Cache.Tests
{
    using System;
    using Dxw.Cache.Lru;
    using Shouldly;
    using Xunit;

    public class LruStringExpirationTests : BaseCacheTests<string, string>
    {
        [Fact]
        public void DefaultDurationAddItemDefaultDuration_ItemExpires()
        {
            // Arrange
            IPurgeableCash<string, string> cache = new LruCache<string, string>(this.TimeSourceMock.Object);
            var now = default(DateTime);
            this.TimeSourceMock.Setup(s => s.GetNow()).Returns(() => now);

            // Act
            cache.Add("KeyA", "ValueA");
            cache.Purge();
            now = now.Add(LruCache<string, string>.DefaultDuration);
            cache.Purge();

            // Assert
            cache.TryGet("KeyA", out var val).ShouldBeFalse();
        }

        [Fact]
        public void CustomDurationAddItemDefaultDuration_ItemExpires()
        {
            // Arrange
            var customDuration = TimeSpan.FromSeconds(60);
            IPurgeableCash<string, string> cache =
                new LruCache<string, string>(this.TimeSourceMock.Object, customDuration);
            var now = default(DateTime);
            this.TimeSourceMock.Setup(s => s.GetNow()).Returns(() => now);

            // Act
            cache.Add("KeyA", "ValueA");

            // Assert
            var val = default(string);
            cache.Purge();
            cache.TryGet("KeyA", out val).ShouldBeTrue();
            now = now.Add(LruCache<string, string>.DefaultDuration);
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
            IPurgeableCash<string, string> cache =
                new LruCache<string, string>(this.TimeSourceMock.Object);
            var now = default(DateTime);
            this.TimeSourceMock.Setup(s => s.GetNow()).Returns(() => now);

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

            now = now.Add(LruCache<string, string>.DefaultDuration);
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
