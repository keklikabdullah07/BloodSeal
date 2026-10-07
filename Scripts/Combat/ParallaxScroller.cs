using Godot;

namespace BloodSeal.Combat
{
    public partial class ParallaxScroller : Node2D
    {
        [Export] public float ScrollSpeed = 75f;
        private CanvasItem _target;

        public override void _Ready()
        {
            _target = GetParentOrNull<CanvasItem>() ?? this;
        }

        public override void _Process(double delta)
        {
#pragma warning disable CS0618
            if (GetParent() is ParallaxBackground pb)
            {
                pb.ScrollOffset = new Vector2(pb.ScrollOffset.X - ScrollSpeed * (float)delta, pb.ScrollOffset.Y);
            }
#pragma warning restore CS0618
        }
    }
}
