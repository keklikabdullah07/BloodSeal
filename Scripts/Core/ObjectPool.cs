#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }

    public class ObjectPool<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly Action<T>? _onAcquire;
        private readonly Action<T>? _onRelease;
        private readonly Stack<T> _available = new();
        private readonly HashSet<T> _active = new();

        public int TotalCount => _available.Count + _active.Count;
        public int AvailableCount => _available.Count;
        public int ActiveCount => _active.Count;

        public ObjectPool(Func<T> factory, Action<T>? onAcquire = null, Action<T>? onRelease = null, int initialCapacity = 0)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onAcquire = onAcquire;
            _onRelease = onRelease;

            if (initialCapacity > 0)
            {
                Prewarm(initialCapacity);
            }
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = _factory();
                _onRelease?.Invoke(item);
                if (item is IPoolable poolable)
                {
                    poolable.OnReturnToPool();
                }
                _available.Push(item);
            }
        }

        public T Acquire()
        {
            var item = _available.Count > 0 ? _available.Pop() : _factory();
            _active.Add(item);

            _onAcquire?.Invoke(item);
            if (item is IPoolable poolable)
            {
                poolable.OnSpawnFromPool();
            }

            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;

            // Prevent double-releasing if already in the pool
            if (!_active.Remove(item) && _available.Contains(item))
            {
                return;
            }

            _onRelease?.Invoke(item);
            if (item is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }

            _available.Push(item);
        }

        public void Clear()
        {
            _available.Clear();
            _active.Clear();
        }
    }
}
