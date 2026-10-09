using Godot;
using BloodSeal.Core;
using BloodSeal.Combat;

namespace BloodSeal.UI
{
    public partial class FloatingText : Node2D, IPoolable
    {
        [Export] public Label LabelNode;
        private Tween _activeTween;

        public override void _Ready()
        {
            if (LabelNode == null)
            {
                LabelNode = GetNodeOrNull<Label>("Label");
            }
        }

        public void Setup(string text, Color color, float scale = 1.0f)
        {
            if (LabelNode == null)
            {
                LabelNode = GetNodeOrNull<Label>("Label");
            }

            if (LabelNode != null)
            {
                LabelNode.Text = text;
                LabelNode.Modulate = color;
            }
            Scale = Vector2.One * scale;
            Modulate = Colors.White;

            _activeTween?.Kill();
            _activeTween = CreateTween().SetParallel(true);
            _activeTween.TweenProperty(this, "position:y", Position.Y - 60f, 0.7f)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.Out);
            _activeTween.TweenProperty(this, "modulate:a", 0f, 0.7f)
                 .SetEase(Tween.EaseType.In);
            _activeTween.Chain().TweenCallback(Callable.From(ReturnToPool));
        }

        private void ReturnToPool()
        {
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ReleaseText(this);
            }
            else
            {
                QueueFree();
            }
        }

        public void OnSpawnFromPool()
        {
            Modulate = Colors.White;
        }

        public void OnReturnToPool()
        {
            _activeTween?.Kill();
            _activeTween = null;
        }
    }
}
