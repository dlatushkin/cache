using Dxw.Cache.Lru;
using Shouldly;
using System;
using Xunit;

namespace Dxw.Cache.Tests
{
    public class LruStringExpirationTests : BaseCacheTests<string, string>
    {
        [Fact]
        public void DefaultDurationAddItemDefaultDuration_ItemExpires()
        {
            // Arrange
            IPurgeableCash<string, string> cache = new LruExpiringCache<string, string>(_timeSourceMock.Object);
            var now = default(DateTime);
            _timeSourceMock.Setup(s => s.GetNow()).Returns(() => now);

            // Act
            cache.Add("KeyA", "ValueA");
            cache.Purge();
            now = now.Add(LruExpiringCache<string, string>.DefaultDuration);
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
                new LruExpiringCache<string, string>(_timeSourceMock.Object, customDuration);
            var now = default(DateTime);
            _timeSourceMock.Setup(s => s.GetNow()).Returns(() => now);

            // Act
            cache.Add("KeyA", "ValueA");

            // Assert
            var val = default(string);
            cache.Purge();
            cache.TryGet("KeyA", out val).ShouldBeTrue();
            now = now.Add(LruExpiringCache<string, string>.DefaultDuration);
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
                new LruExpiringCache<string, string>(_timeSourceMock.Object);
            var now = default(DateTime);
            _timeSourceMock.Setup(s => s.GetNow()).Returns(() => now);

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

            now = now.Add(LruExpiringCache<string, string>.DefaultDuration);
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