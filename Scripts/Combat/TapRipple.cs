using Godot;

namespace BloodSeal.Combat
{
    public partial class TapRipple : Node2D
    {
        public override void _Ready()
        {
            var tween = CreateTween().SetParallel(true);
            tween.TweenProperty(this, "scale", new Vector2(2.2f, 2.2f), 0.25f)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(this, "modulate:a", 0f, 0.25f)
                 .SetEase(Tween.EaseType.In);
            tween.Chain().TweenCallback(Callable.From(QueueFree));
        }
    }
}
