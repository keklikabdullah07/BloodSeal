using System;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class MockPoolableItem : IPoolable
    {
        public bool IsSpawned { get; private set; }
        public int SpawnCount { get; private set; }
        public int ReturnCount { get; private set; }

        public void OnSpawnFromPool()
        {
            IsSpawned = true;
            SpawnCount++;
        }

        public void OnReturnToPool()
        {
            IsSpawned = false;
            ReturnCount++;
        }
    }

    public class ObjectPoolTests
    {
        [Fact]
        public void Prewarm_ShouldCreateInitialAvailableItems()
        {
            int factoryCalls = 0;
            var pool = new ObjectPool<MockPoolableItem>(() =>
            {
                factoryCalls++;
                return new MockPoolableItem();
            }, initialCapacity: 10);

            Assert.Equal(10, factoryCalls);
            Assert.Equal(10, pool.TotalCount);
            Assert.Equal(10, pool.AvailableCount);
            Assert.Equal(0, pool.ActiveCount);
        }

        [Fact]
        public void Acquire_ShouldReuseAvailableItem_AndTriggerOnSpawnFromPool()
        {
            var pool = new ObjectPool<MockPoolableItem>(() => new MockPoolableItem(), initialCapacity: 5);

            var item = pool.Acquire();

            Assert.NotNull(item);
            Assert.True(item.IsSpawned);
            Assert.Equal(1, item.SpawnCount);
            Assert.Equal(4, pool.AvailableCount);
            Assert.Equal(1, pool.ActiveCount);
            Assert.Equal(5, pool.TotalCount);
        }

        [Fact]
        public void Release_ShouldReturnItemToPool_AndTriggerOnReturnToPool()
        {
            var pool = new ObjectPool<MockPoolableItem>(() => new MockPoolableItem(), initialCapacity: 5);
            var item = pool.Acquire();

            pool.Release(item);

            Assert.False(item.IsSpawned);
            Assert.Equal(5, pool.AvailableCount);
            Assert.Equal(0, pool.ActiveCount);
        }

        [Fact]
        public void Release_DoubleRelease_ShouldBeSafelyIgnored()
        {
            var pool = new ObjectPool<MockPoolableItem>(() => new MockPoolableItem(), initialCapacity: 2);
            var item = pool.Acquire();

            pool.Release(item);
            pool.Release(item); // Double release

            Assert.Equal(2, pool.AvailableCount);
            Assert.Equal(0, pool.ActiveCount);
        }

        [Fact]
        public void Acquire_WhenExhausted_ShouldAutoExpand()
        {
            int factoryCalls = 0;
            var pool = new ObjectPool<MockPoolableItem>(() =>
            {
                factoryCalls++;
                return new MockPoolableItem();
            }, initialCapacity: 2);

            var item1 = pool.Acquire();
            var item2 = pool.Acquire();
            var item3 = pool.Acquire(); // 3rd item should instantiate newly

            Assert.Equal(3, factoryCalls);
            Assert.Equal(3, pool.ActiveCount);
            Assert.Equal(0, pool.AvailableCount);
            Assert.Equal(3, pool.TotalCount);
        }
    }
}
