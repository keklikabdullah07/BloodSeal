using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class TapRipple : Node2D, IPoolable
    {
        private Tween _activeTween;

        public void Play(Vector2 pos)
        {
            GlobalPosition = pos;
            Scale = Vector2.One;
            Modulate = Colors.White;

            _activeTween?.Kill();
            _activeTween = CreateTween().SetParallel(true);
            _activeTween.TweenProperty(this, "scale", new Vector2(2.2f, 2.2f), 0.25f)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.Out);
            _activeTween.TweenProperty(this, "modulate:a", 0f, 0.25f)
                 .SetEase(Tween.EaseType.In);
            _activeTween.Chain().TweenCallback(Callable.From(ReturnToPool));
        }

        private void ReturnToPool()
        {
            if (FXManager.Instance != null)
            {
                FXManager.Instance.ReleaseTapRipple(this);
            }
            else
            {
                QueueFree();
            }
        }

        public void OnSpawnFromPool()
        {
            Visible = true;
        }

        public void OnReturnToPool()
        {
            _activeTween?.Kill();
            _activeTween = null;
        }
    }
}
