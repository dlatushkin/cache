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

# Problems fixed

## Jacob notes
> The cache is only partially thread-safe:
> There are some public operations that are not protected by a lock (i.e. Remove).
> Some linked-list management is happening outside a lock – could potentially lead to corruption.

All public operations protected by lock. It's choosen because all operations requires write operations and in this case
Monitor is faster than ReaderWriterLockXXX

> The background timer seems to be running forever and not get disposed, even though the elapsed-event is not set

Timer is stopped right after "elapsed" unsubscribe operation

> Error in TryAdd: Under certain race conditions the value is not updated.

All public methods protected with lock as mentioned above

> Minor issues:
> a. With the added complexity of maintaining both a linked list and a dictionary, one could expect this to be isolated into a separate class

[LruList class](/Source/Dxw.Cache.Lru/LruList.cs) is implemented to encapsulate least recent used nodes.
> b. Spelling mistakes: “IPurgeableCash”, “ActiveLruCash”

Corrected
> c. Implementation details are public, i.e.: Node-class.

Node class is renamed to [Slot](/Source/Dxw.Cache.Lru/Slot.cs) (to avoid name colision with native LinkedListNode class) and made internal

## Denis findings and improvements
- Unit tests are restructured and refactored to make possible testing of other cache implementations easier
- During node update by key custom duration (if any) wasn't updated. Fixed.
- Timer is made autoreset = false to avoid queuing events if cleanup tokes too long
