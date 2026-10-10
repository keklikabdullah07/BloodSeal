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
        private Sprite2D _petSprite;
        private CpuParticles2D _auraParticles;
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

        public override void _ExitTree()
        {
            if (FamiliarManager.Instance != null)
            {
                FamiliarManager.Instance.OnActiveFamiliarChanged -= OnActiveFamiliarChanged;
            }
        }

        public override void _Ready()
        {
            Instance = this;
            if (_hero == null && TargetHero != null)
            {
                _hero = TargetHero;
            }

            _petSprite = GetNodeOrNull<Sprite2D>("PetSprite");
            _auraParticles = GetNodeOrNull<CpuParticles2D>("AuraParticles");

            if (ProjectileScene != null)
            {
                _projectilePool = new NodePool<BloodProjectile>(ProjectileScene, this, 15);
            }

            if (FamiliarManager.Instance != null)
            {
                FamiliarManager.Instance.OnActiveFamiliarChanged += OnActiveFamiliarChanged;
                ApplyFamiliarVisuals(FamiliarManager.Instance.ActiveFamiliarId);
            }
        }

        private void OnActiveFamiliarChanged(string id)
        {
            ApplyFamiliarVisuals(id);
        }

        private void ApplyFamiliarVisuals(string id)
        {
            var def = FamiliarDatabase.Get(id);
            if (def == null) return;

            var auraCol = new Color(def.AuraColorHex);
            if (_auraParticles != null)
            {
                _auraParticles.Color = auraCol;
            }

            if (_petSprite != null)
            {
                // Modulate pet sprite slightly to match familiar thematic aura
                _petSprite.Modulate = def.Type switch
                {
                    FamiliarType.ShadowBat => new Color(0.85f, 0.65f, 1.0f),
                    FamiliarType.CrimsonHound => new Color(1.0f, 0.7f, 0.7f),
                    FamiliarType.StoneGargoyle => new Color(0.85f, 0.85f, 0.85f),
                    _ => Colors.White
                };
            }
        }

        public void ReleaseProjectile(BloodProjectile proj)
        {
            if (proj == null) return;
            if (_projectilePool != null) _projectilePool.Release(proj);
            else proj.QueueFree();
        }

        public override void _Process(double delta)
        {
            _timePassed += delta;
            if (IsInstanceValid(_hero))
            {
                float hoverY = Mathf.Sin((float)_timePassed * 3.2f + HoverPhase) * 14f;
                GlobalPosition = _hero.GlobalPosition + BaseOffset + new Vector2(0, hoverY);
            }

            float baseInterval = FamiliarManager.Instance?.GetActiveAttackInterval() ?? 1.4f;
            if (GameManager.Instance?.Profile?.Origin == StreetOriginType.GangLeader)
            {
                baseInterval *= 0.75f;
            }

            float petMult = ResearchManager.Instance?.GetPetMultiplier() ?? 1.0f;
            float shootInterval = Mathf.Max(0.4f, baseInterval / petMult);

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

                float baseAtk = GameManager.Instance?.Stats?.Atk ?? 10f;
                float researchMult = ResearchManager.Instance?.GetPetMultiplier() ?? 1.0f;
                proj.Damage = FamiliarManager.Instance != null
                    ? FamiliarManager.Instance.GetActivePetDamage(baseAtk, researchMult)
                    : baseAtk * 0.4f * researchMult;

                if (_projectilePool == null)
                {
                    GetTree().CurrentScene.AddChild(proj);
                }
            }
        }
    }
}
