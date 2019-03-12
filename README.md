# LRU cache implementation for upwork evaluation project.

> A generic cache with a limited number of elements (capacity) and where elements are automatically removed after they have not been accessed for a certain amount of time (duration).

[ActiveLruCash](/Source/Dxw.Cache.Lru/ActiveLruCash.cs) implements active automatically removal

> The class is expected to have a constructor with default-duration and max-capacity.

[LruCache](/Source/Dxw.Cache.Lru/LruCache.cs) implemented this requirement
Tested in [LruStringCapacityTests](/Source/Dxw.Cache.Tests/LruStringCapacityTests.cs) and [LruStringExpirationTests](/Source/Dxw.Cache.Tests/LruStringExpirationTests.cs)

> When inserting new elements causing the cache to exceed its max capacity the “least accessed” element (the element that has not been accessed for the longest amount of time) should be removed first

Tested in [LruStringCapacityTests](/Source/Dxw.Cache.Tests/LruStringCapacityTests.cs)

> The Add()-method can update an element if the key already exists in the cache

Tested in [LruStringBaseTests](/Source/Dxw.Cache.Tests/LruStringBaseTests.cs)

> The implementation is preferred to not be implementing IDisposable and thus be able to handle any internal cleanup (if any) without requiring a Dispose() call.

WeakReference approach is utilized (see [WeakEventManager](/Source/Dxw.Cache.Lru/WeakEventManager.cs)).

> ### Observe
> #### Thread safety

The cache is thread safe (lock is applied)
> #### Efficiency

Lookup by key is performed by dictionary. Element order is preserved dynamicall via moving to linked list head.
> #### Testability

Cache dependencies is interfaces (single one in this case ITimeSource) so unit tests were easy to implement
> #### Apply SOLID principles

Appplied
> #### Prove functionality using unit tests

See [Dxw.Cache.Tests](/Source/Dxw.Cache.Tests) project

> The coding style is expected to follow Microsoft’s .Net coding and naming guidelines.

Stylecop is applied to all projects

