using Godot;

namespace BloodSeal.UI
{
    public partial class FloatingText : Node2D
    {
        [Export] public Label LabelNode;

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

            var tween = CreateTween().SetParallel(true);
            tween.TweenProperty(this, "position:y", Position.Y - 60f, 0.7f)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(this, "modulate:a", 0f, 0.7f)
                 .SetEase(Tween.EaseType.In);
            tween.Chain().TweenCallback(Callable.From(QueueFree));
        }
    }
}
