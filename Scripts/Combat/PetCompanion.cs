using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class PetCompanion : Node2D
    {
        [Export] public Vector2 BaseOffset = new Vector2(-70, -80);
        [Export] public float HoverPhase = 0f;
        [Export] public Color AuraColor = new Color(0.9f, 0.1f, 0.2f, 0.9f);
        [Export] public PackedScene ProjectileScene;

        private Node2D _hero;
        private double _shootTimer = 0.0;
        private double _timePassed = 0.0;

        public void Setup(Node2D hero)
        {
            _hero = hero;
        }

        public override void _Ready()
        {
            if (_hero == null && GetParent() != null)
            {
                _hero = GetParent().GetNodeOrNull<Node2D>("Hero");
            }
        }

        public override void _Process(double delta)
        {
            _timePassed += delta;
            if (IsInstanceValid(_hero))
            {
                float hoverY = Mathf.Sin((float)_timePassed * 3.2f + HoverPhase) * 14f;
                GlobalPosition = _hero.GlobalPosition + BaseOffset + new Vector2(0, hoverY);
            }

            float shootInterval = (GameManager.Instance?.Profile?.Origin == StreetOriginType.GangLeader) ? 1.05f : 1.4f;
            _shootTimer += delta;
            if (_shootTimer >= shootInterval)
            {
                _shootTimer = 0.0;
                TryShoot();
            }
        }

        private void TryShoot()
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            Enemy nearest = null;
            float minDist = float.MaxValue;

            foreach (var node in enemies)
            {
                if (node is Enemy e && !e.IsDead)
                {
                    float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = e;
                    }
                }
            }

            if (nearest != null && ProjectileScene != null)
            {
                var proj = ProjectileScene.Instantiate<BloodProjectile>();
                proj.GlobalPosition = GlobalPosition;
                proj.Target = nearest;
                proj.Damage = GameManager.Instance.Stats.Atk * 0.4f;
                GetTree().CurrentScene.AddChild(proj);
            }
        }
    }
}
