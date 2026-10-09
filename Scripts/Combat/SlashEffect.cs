using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class SlashEffect : Node2D, IPoolable
    {
        private Tween _activeTween;

        public void Play(Vector2 pos, bool isBerserk)
        {
            GlobalPosition = pos;
            if (isBerserk)
            {
                Scale = new Vector2(1.6f, 1.6f);
                Modulate = new Color(1.5f, 0.4f, 0.2f, 1f);
            }
            else
            {
                Scale = Vector2.One;
                Modulate = Colors.White;
            }

            _activeTween?.Kill();
            _activeTween = CreateTween().SetParallel(true);
            _activeTween.TweenProperty(this, "scale:x", (isBerserk ? 1.6f : 1.0f) * 1.4f, 0.12f)
                 .SetTrans(Tween.TransitionType.Back)
                 .SetEase(Tween.EaseType.Out);
            _activeTween.TweenProperty(this, "position:x", Position.X + 25f, 0.12f);
            _activeTween.TweenProperty(this, "modulate:a", 0f, 0.12f)
                 .SetEase(Tween.EaseType.In);
            _activeTween.Chain().TweenCallback(Callable.From(ReturnToPool));
        }

        private void ReturnToPool()
        {
            if (FXManager.Instance != null)
            {
                FXManager.Instance.ReleaseSlash(this);
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
