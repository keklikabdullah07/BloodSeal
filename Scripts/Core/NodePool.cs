using System;
using Godot;

namespace BloodSeal.Core
{
    public class NodePool<T> where T : Node
    {
        private readonly PackedScene _scene;
        private readonly Node _parent;
        private readonly ObjectPool<T> _pool;

        public int TotalCount => _pool.TotalCount;
        public int AvailableCount => _pool.AvailableCount;
        public int ActiveCount => _pool.ActiveCount;

        public NodePool(PackedScene scene, Node parent, int initialCapacity = 0)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            _pool = new ObjectPool<T>(
                factory: CreateInstance,
                onAcquire: OnAcquireItem,
                onRelease: OnReleaseItem,
                initialCapacity: initialCapacity
            );
        }

        private T CreateInstance()
        {
            var node = _scene.Instantiate<T>();
            _parent.AddChild(node);
            return node;
        }

        private static void OnAcquireItem(T item)
        {
            if (item is CanvasItem canvasItem)
            {
                canvasItem.Visible = true;
            }
            item.ProcessMode = Node.ProcessModeEnum.Inherit;
        }

        private static void OnReleaseItem(T item)
        {
            if (item is CanvasItem canvasItem)
            {
                canvasItem.Visible = false;
            }
            item.ProcessMode = Node.ProcessModeEnum.Disabled;
        }

        public void Prewarm(int count) => _pool.Prewarm(count);
        public T Acquire() => _pool.Acquire();
        public void Release(T item) => _pool.Release(item);
        public void Clear() => _pool.Clear();
    }
}
