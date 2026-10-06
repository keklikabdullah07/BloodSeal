using Godot;

namespace BloodSeal.Combat
{
    public partial class SlashEffect : Node2D
    {
        public override void _Ready()
        {
            var tween = CreateTween().SetParallel(true);
            tween.TweenProperty(this, "scale:x", 1.4f, 0.12f)
                 .SetTrans(Tween.TransitionType.Back)
                 .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(this, "position:x", Position.X + 25f, 0.12f);
            tween.TweenProperty(this, "modulate:a", 0f, 0.12f)
                 .SetEase(Tween.EaseType.In);
            tween.Chain().TweenCallback(Callable.From(QueueFree));
        }
    }
}
