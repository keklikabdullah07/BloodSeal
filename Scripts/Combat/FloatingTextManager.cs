using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class FloatingTextManager : Node2D
    {
        public static FloatingTextManager Instance { get; private set; }
        [Export] public PackedScene FloatingTextScene;

        private NodePool<UI.FloatingText> _pool;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            if (FloatingTextScene != null)
            {
                _pool = new NodePool<UI.FloatingText>(FloatingTextScene, this, 30);
            }
        }

        private UI.FloatingText AcquireText()
        {
            if (_pool != null) return _pool.Acquire();
            if (FloatingTextScene != null)
            {
                var text = FloatingTextScene.Instantiate<UI.FloatingText>();
                AddChild(text);
                return text;
            }
            return null;
        }

        public void ReleaseText(UI.FloatingText text)
        {
            if (text == null) return;
            if (_pool != null)
            {
                _pool.Release(text);
            }
            else
            {
                text.QueueFree();
            }
        }

        public void SpawnDamage(Vector2 pos, float amount, bool isCrit)
        {
            var text = AcquireText();
            if (text == null) return;
            text.GlobalPosition = pos + new Vector2(GD.Randf() * 30 - 15, GD.Randf() * 20 - 10);
            Color col = isCrit ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.9f, 0.6f);
            string prefix = isCrit ? "CRIT! " : "";
            text.Setup($"{prefix}{amount:F0}", col, isCrit ? 1.4f : 1.0f);
        }

        public void SpawnGold(Vector2 pos, double amount)
        {
            var text = AcquireText();
            if (text == null) return;
            text.GlobalPosition = pos;
            text.Setup($"+{Core.BigNumberFormatter.Format(amount)} 🪙", new Color(1f, 0.85f, 0.2f), 1.15f);
            Core.AudioManager.Instance?.PlayCoin();
        }

        public void SpawnEnrage(Vector2 pos, float multiplier)
        {
            var text = AcquireText();
            if (text == null) return;
            text.GlobalPosition = pos;
            text.Setup($"ENRAGE x{multiplier:F1}!", new Color(1f, 0.15f, 0.15f), 1.5f);
            Core.AudioManager.Instance?.PlayEnrage();
        }

        public void SpawnMessage(Vector2 pos, string message, Color color, float scale = 1.2f)
        {
            var text = AcquireText();
            if (text == null) return;
            text.GlobalPosition = pos;
            text.Setup(message, color, scale);
        }
    }
}
