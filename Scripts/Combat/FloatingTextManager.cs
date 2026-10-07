using Godot;

namespace BloodSeal.Combat
{
    public partial class FloatingTextManager : Node2D
    {
        public static FloatingTextManager Instance { get; private set; }
        [Export] public PackedScene FloatingTextScene;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public void SpawnDamage(Vector2 pos, float amount, bool isCrit)
        {
            if (FloatingTextScene == null) return;
            var text = FloatingTextScene.Instantiate<UI.FloatingText>();
            text.GlobalPosition = pos + new Vector2(GD.Randf() * 30 - 15, GD.Randf() * 20 - 10);
            Color col = isCrit ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.9f, 0.6f);
            string prefix = isCrit ? "CRIT! " : "";
            text.Setup($"{prefix}{amount:F0}", col, isCrit ? 1.4f : 1.0f);
            AddChild(text);
        }

        public void SpawnGold(Vector2 pos, double amount)
        {
            if (FloatingTextScene == null) return;
            var text = FloatingTextScene.Instantiate<UI.FloatingText>();
            text.GlobalPosition = pos;
            text.Setup($"+{Core.BigNumberFormatter.Format(amount)} 🪙", new Color(1f, 0.85f, 0.2f), 1.15f);
            AddChild(text);
        }

        public void SpawnEnrage(Vector2 pos, float multiplier)
        {
            if (FloatingTextScene == null) return;
            var text = FloatingTextScene.Instantiate<UI.FloatingText>();
            text.GlobalPosition = pos;
            text.Setup($"ENRAGE x{multiplier:F1}!", new Color(1f, 0.15f, 0.15f), 1.5f);
            AddChild(text);
        }
    }
}
