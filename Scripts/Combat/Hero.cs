using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class Hero : CharacterBody2D
    {
        public float CurrentHp { get; private set; }
        public float MaxHp => GameManager.Instance != null ? GameManager.Instance.Stats.MaxHp : 100f;

        public event Action<float, float> OnHealthChanged; // current, max

        private double _attackCooldown = 0.0;
        private Node2D _visualRoot;
        private ProgressBar _healthBar;

        public override void _Ready()
        {
            _visualRoot = GetNodeOrNull<Node2D>("VisualRoot");
            _healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
            CurrentHp = MaxHp;
            UpdateHealthUI();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsUpgraded += OnStatsChanged;
                GameManager.Instance.OnHeroDied += OnHeroRespawned;
            }
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsUpgraded -= OnStatsChanged;
                GameManager.Instance.OnHeroDied -= OnHeroRespawned;
            }
        }

        private void OnStatsChanged()
        {
            if (CurrentHp > MaxHp) CurrentHp = MaxHp;
            UpdateHealthUI();
        }

        private void OnHeroRespawned()
        {
            CurrentHp = MaxHp;
            UpdateHealthUI();
        }

        public override void _Process(double delta)
        {
            _attackCooldown -= delta;
            float currentAtkSpeed = GameManager.Instance != null ? GameManager.Instance.Stats.AtkSpeed : 1.0f;
            if (GameManager.Instance != null && GameManager.Instance.IsRageActive)
            {
                currentAtkSpeed *= 2f;
            }

            float cooldownTime = 1f / currentAtkSpeed;

            if (_attackCooldown <= 0.0)
            {
                Enemy target = FindTargetInAttackRange();
                if (target != null)
                {
                    PerformAttack(target);
                    _attackCooldown = cooldownTime;
                }
            }
        }

        private Enemy FindTargetInAttackRange()
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            Enemy closest = null;
            float attackRange = GameManager.Instance != null ? GameManager.Instance.Stats.Range : 180f;

            foreach (var node in enemies)
            {
                if (node is Enemy e && !e.IsDead)
                {
                    float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
                    if (dist <= attackRange)
                    {
                        attackRange = dist;
                        closest = e;
                    }
                }
            }
            return closest;
        }

        private void PerformAttack(Enemy target)
        {
            bool isRage = GameManager.Instance != null && GameManager.Instance.IsRageActive;
            float damage = GameManager.Instance != null ? GameManager.Instance.Stats.Atk : 10f;
            bool isCrit = isRage;
            if (isCrit) damage *= 2f;

            // Slash tween animation
            if (_visualRoot != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(_visualRoot, "position:x", 35f, 0.07f)
                     .SetTrans(Tween.TransitionType.Back)
                     .SetEase(Tween.EaseType.Out);
                tween.TweenProperty(_visualRoot, "position:x", 0f, 0.12f);
            }

            // Spawn Slash VFX
            FXManager.Instance?.PlaySlash(target.GlobalPosition + new Vector2(-20, -35), isRage);

            // Screen shake & hit freeze
            if (isCrit)
            {
                CameraShake.Instance?.AddTrauma(0.35f);
                FXManager.Instance?.TriggerHitFreeze(0.045f);
            }
            else
            {
                CameraShake.Instance?.AddTrauma(0.12f);
            }

            target.TakeDamage(damage, isCrit);

            // Lifesteal
            if (GameManager.Instance != null)
            {
                float lifestealRate = GameManager.Instance.Stats.LifestealPercent / 100f;
                float heal = damage * lifestealRate;
                Heal(heal);

                // Rage accumulation
                GameManager.Instance.AddRage(2.0f);
            }
        }

        public void Heal(float amount)
        {
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
            UpdateHealthUI();
        }

        public void TakeDamage(float amount)
        {
            CurrentHp -= amount;
            UpdateHealthUI();

            // Heavy screen shake on hero taking damage
            CameraShake.Instance?.AddTrauma(0.25f);

            // Flash visual on hit
            if (_visualRoot != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(_visualRoot, "modulate", new Color(2f, 0.4f, 0.4f), 0.05f);
                tween.TweenProperty(_visualRoot, "modulate", Colors.White, 0.1f);
            }

            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                GameManager.Instance?.NotifyHeroDied();
            }
        }

        private void UpdateHealthUI()
        {
            if (_healthBar != null && MaxHp > 0)
            {
                _healthBar.MaxValue = MaxHp;
                _healthBar.Value = Mathf.Max(0, CurrentHp);
            }
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }
    }
}
