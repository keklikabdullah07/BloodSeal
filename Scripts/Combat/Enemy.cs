using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class Enemy : CharacterBody2D, IPoolable
    {
        [Export] public float MoveSpeed = 120f;
        public float MaxHp { get; protected set; }
        public float CurrentHp { get; protected set; }
        public float AttackDamage { get; protected set; }
        public bool IsDead { get; protected set; } = false;

        public event Action<float, float> OnHealthChanged;

        protected void TriggerHealthChanged(float current, float max)
        {
            OnHealthChanged?.Invoke(current, max);
        }

        protected Hero _heroTarget;
        protected double _attackTimer = 0.0;
        protected ProgressBar _healthBar;
        protected Node2D _visual;
        protected Sprite2D _sprite;

        public override void _Ready()
        {
            _healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
            _visual = GetNodeOrNull<Node2D>("Visual");
            _sprite = _visual?.GetNodeOrNull<Sprite2D>("EnemySprite");
        }

        public virtual void Setup(int wave, Hero hero)
        {
            _heroTarget = hero;
            IsDead = false;
            MaxHp = wave * 25f + 50f;
            CurrentHp = MaxHp;
            AttackDamage = wave * 3f + 5f;

            if (_sprite != null)
            {
                string texPath = ZoneHelper.GetMinionTexturePath(wave);
                var tex = GD.Load<Texture2D>(texPath);
                if (tex != null) _sprite.Texture = tex;
            }

            if (!IsInGroup("Enemies"))
            {
                AddToGroup("Enemies");
            }
            UpdateHealthBar();
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public override void _Process(double delta)
        {
            if (IsDead) return;

            if (IsInstanceValid(_heroTarget))
            {
                float dist = GlobalPosition.DistanceTo(_heroTarget.GlobalPosition);
                if (dist > 95f)
                {
                    Velocity = new Vector2(-MoveSpeed, 0);
                    MoveAndSlide();
                }
                else
                {
                    _attackTimer += delta;
                    if (_attackTimer >= 1.0)
                    {
                        _attackTimer = 0.0;
                        _heroTarget.TakeDamage(AttackDamage);
                        // Attack hit tween
                        if (_visual != null)
                        {
                            var tween = CreateTween();
                            tween.TweenProperty(_visual, "position:x", -15f, 0.08f);
                            tween.TweenProperty(_visual, "position:x", 0f, 0.12f);
                        }
                    }
                }
            }
        }

        public virtual void TakeDamage(float amount, bool isCrit)
        {
            if (IsDead) return;

            CurrentHp -= amount;
            UpdateHealthBar();
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

            // Pop-up floating damage text
            FloatingTextManager.Instance?.SpawnDamage(GlobalPosition + new Vector2(0, -50), amount, isCrit);

            // Blood splatter VFX & SFX
            FXManager.Instance?.PlayBloodSplatter(GlobalPosition + new Vector2(0, -35), Vector2.Right);
            Core.AudioManager.Instance?.PlayHit(isCrit);

            // Flash visual on hit
            if (_visual != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(_visual, "modulate", new Color(2f, 0.5f, 0.5f), 0.06f);
                tween.TweenProperty(_visual, "modulate", Colors.White, 0.1f);
            }

            if (CurrentHp <= 0)
            {
                Die();
            }
        }

        protected void UpdateHealthBar()
        {
            if (_healthBar != null && MaxHp > 0)
            {
                _healthBar.MaxValue = MaxHp;
                _healthBar.Value = Mathf.Max(0, CurrentHp);
                _healthBar.Visible = CurrentHp < MaxHp && CurrentHp > 0;
            }
        }

        protected virtual void Die()
        {
            if (IsDead) return;
            IsDead = true;

            // Death explosion particle
            FXManager.Instance?.PlayDeathExplosion(GlobalPosition + new Vector2(0, -35), this is BossEnemy);

            if (GameManager.Instance != null)
            {
                double baseGold = GameManager.Instance.CurrentWave * 5 + 10;
                double goldReward = GameManager.Instance.CalculateGoldReward(baseGold);
                GameManager.Instance.AddGold(goldReward);
                FloatingTextManager.Instance?.SpawnGold(GlobalPosition + new Vector2(0, -30), goldReward);
            }

            if (this is not BossEnemy && WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.ReleaseEnemy(this);
            }
            else
            {
                QueueFree();
            }
        }

        public virtual void OnSpawnFromPool()
        {
            IsDead = false;
            Visible = true;
            SetProcess(true);
            SetPhysicsProcess(true);
        }

        public virtual void OnReturnToPool()
        {
            IsDead = true;
            _heroTarget = null;
            Velocity = Vector2.Zero;
            if (IsInGroup("Enemies"))
            {
                RemoveFromGroup("Enemies");
            }
            if (_healthBar != null)
            {
                _healthBar.Visible = false;
            }
            if (_visual != null)
            {
                _visual.Position = Vector2.Zero;
                _visual.Modulate = Colors.White;
            }
            SetProcess(false);
            SetPhysicsProcess(false);
        }
    }
}
