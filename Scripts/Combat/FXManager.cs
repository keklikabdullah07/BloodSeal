using Godot;

namespace BloodSeal.Combat
{
    public partial class FXManager : Node2D
    {
        public static FXManager Instance { get; private set; }

        [Export] public PackedScene SlashScene;
        [Export] public PackedScene BloodSplatterScene;
        [Export] public PackedScene DeathExplosionScene;
        [Export] public PackedScene TapRippleScene;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public void PlaySlash(Vector2 pos, bool isBerserk)
        {
            if (SlashScene == null) return;
            var slash = SlashScene.Instantiate<Node2D>();
            slash.GlobalPosition = pos;
            if (isBerserk)
            {
                slash.Scale = new Vector2(1.6f, 1.6f);
                slash.Modulate = new Color(1.5f, 0.4f, 0.2f);
            }
            AddChild(slash);
        }

        public void PlayBloodSplatter(Vector2 pos, Vector2 direction)
        {
            if (BloodSplatterScene == null) return;
            var blood = BloodSplatterScene.Instantiate<Node2D>();
            blood.GlobalPosition = pos;
            blood.Rotation = direction.Angle();
            AddChild(blood);
        }

        public void PlayDeathExplosion(Vector2 pos, bool isBoss)
        {
            if (DeathExplosionScene == null) return;
            var expl = DeathExplosionScene.Instantiate<Node2D>();
            expl.GlobalPosition = pos;
            if (isBoss)
            {
                expl.Scale = new Vector2(2.5f, 2.5f);
            }
            AddChild(expl);
        }

        public void PlayTapRipple(Vector2 pos)
        {
            if (TapRippleScene == null) return;
            var rip = TapRippleScene.Instantiate<Node2D>();
            rip.GlobalPosition = pos;
            AddChild(rip);
        }

        public async void TriggerHitFreeze(float duration = 0.045f)
        {
            Engine.TimeScale = 0.05;
            await ToSignal(GetTree().CreateTimer(duration * 0.05, true, false, true), "timeout");
            Engine.TimeScale = 1.0;
        }
    }
}
