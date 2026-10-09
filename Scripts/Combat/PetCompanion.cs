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

        [Export] public Node2D TargetHero;

        public static PetCompanion Instance { get; private set; }

        private NodePool<BloodProjectile> _projectilePool;
        private Node2D _hero;
        private double _shootTimer = 0.0;
        private double _timePassed = 0.0;

        public void Setup(Node2D hero)
        {
            _hero = hero;
            TargetHero = hero;
        }

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            Instance = this;
            if (_hero == null && TargetHero != null)
            {
                _hero = TargetHero;
            }

            if (ProjectileScene != null)
            {
                _projectilePool = new NodePool<BloodProjectile>(ProjectileScene, this, 15);
            }
        }

        public void ReleaseProjectile(BloodProjectile proj)
        {
            if (proj == null) return;
            if (_projectilePool != null)
            {
                _projectilePool.Release(proj);
            }
            else
            {
                proj.QueueFree();
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

            float petMult = ResearchManager.Instance?.GetPetMultiplier() ?? 1.0f;
            float shootInterval = (GameManager.Instance?.Profile?.Origin == StreetOriginType.GangLeader) ? 1.05f : 1.4f;
            shootInterval = Mathf.Max(0.5f, shootInterval / petMult);

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
                BloodProjectile proj = _projectilePool != null ? _projectilePool.Acquire() : ProjectileScene.Instantiate<BloodProjectile>();
                proj.GlobalPosition = GlobalPosition;
                proj.Target = nearest;
                float petMult = ResearchManager.Instance?.GetPetMultiplier() ?? 1.0f;
                float baseAtk = GameManager.Instance?.Stats?.Atk ?? 10f;
                proj.Damage = baseAtk * 0.4f * petMult;
                if (_projectilePool == null)
                {
                    GetTree().CurrentScene.AddChild(proj);
                }
            }
        }
    }
}
