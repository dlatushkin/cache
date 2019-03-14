namespace Dxw.Cache.Tests.CacheTests
{
    using Dxw.Cache.Lru;
    using Dxw.Cache.Tests.BaseTests;
    using Shouldly;
    using Xunit;

    public abstract class CacheStringCapacityTests : BaseCacheTests<string, string>
    {
        [Fact]
        public void DefaultCapacityAddMoreItems_1stItemPushedOut()
        {
            // Arrange
            var (timeMock, cache) = this.CreateICleanableCache();

            // Act
            cache.Add("KeyA", "ValueA");
            cache.Add("KeyB", "ValueB");
            cache.Add("KeyC", "ValueC");

            // Assert
            var valA = default(string);
            var valB = default(string);
            var valC = default(string);

            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeFalse(),
                () => cache.TryGet("KeyB", out valB).ShouldBeTrue(),
                () => valB.ShouldBe("ValueB"),
                () => cache.TryGet("KeyC", out valC).ShouldBeTrue(),
                () => valC.ShouldBe("ValueC"));
        }

        [Fact]
        public void CustomCapacityAddMoreItems_2FirstItemPushedOut()
        {
            // Arrange
            var (timeMock, cache) = this.CreateICleanableCache(maxCapacity: 1);

            // Act
            cache.Add("KeyA", "ValueA");
            cache.Add("KeyB", "ValueB");
            cache.Add("KeyC", "ValueC");

            // Assert
            var valA = default(string);
            var valB = default(string);
            var valC = default(string);

            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeFalse(),
                () => cache.TryGet("KeyB", out valB).ShouldBeFalse(),
                () => cache.TryGet("KeyC", out valC).ShouldBeTrue(),
                () => valC.ShouldBe("ValueC"));
        }

        [Fact]
        public void DefaultCapacityTouch1stAddMoreItems_2ndItemPushedOut()
        {
            // Arrange
            var (timeMock, cache) = this.CreateICleanableCache();

            // Act
            cache.Add("KeyA", "ValueA");
            cache.Add("KeyB", "ValueB");
            cache.TryGet("KeyA", out var _); // just to touch keyA
            cache.Add("KeyC", "ValueC"); // keyC should push out keyB

            // Assert
            var valA = default(string);
            var valB = default(string);
            var valC = default(string);

            this.ShouldSatisfyAllConditions(
                () => cache.TryGet("KeyA", out valA).ShouldBeTrue(),
                () => valA.ShouldBe("ValueA"),
                () => cache.TryGet("KeyB", out valB).ShouldBeFalse(),
                () => cache.TryGet("KeyC", out valC).ShouldBeTrue(),
                () => valC.ShouldBe("ValueC"));
        }
    }
}
