using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class Hero : CharacterBody2D
    {
        public float CurrentHp { get; private set; }
        public float MaxHp => (GameManager.Instance != null ? GameManager.Instance.Stats.MaxHp : 100f) + (EquipmentManager.Instance?.GetTotalPrimaryBonus(EquipmentSlot.Armor) ?? 0f);

        public event Action<float, float> OnHealthChanged; // current, max

        private double _attackCooldown = 0.0;
        private double _idleTime = 0.0;
        private Node2D _visualRoot;
        private ProgressBar _healthBar;
        private bool _wasRageActive = false;

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
            if (EquipmentManager.Instance != null) EquipmentManager.Instance.OnEquipmentChanged += UpdateHealthUI;
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsUpgraded -= OnStatsChanged;
                GameManager.Instance.OnHeroDied -= OnHeroRespawned;
            }
            if (EquipmentManager.Instance != null) EquipmentManager.Instance.OnEquipmentChanged -= UpdateHealthUI;
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
            bool isRage = GameManager.Instance != null && GameManager.Instance.IsRageActive;
            if (isRage != _wasRageActive)
            {
                _wasRageActive = isRage;
                CameraShake.Instance?.SetBaseTrauma(isRage ? 0.15f : 0.0f);
            }

            _idleTime += delta;
            if (_attackCooldown > 0.0)
            {
                _attackCooldown -= delta;
            }

            if (_visualRoot != null)
            {
                _visualRoot.Scale = new Vector2(1.0f, 1.0f + Mathf.Sin((float)_idleTime * 3.5f) * 0.02f);
            }

            float eqSpeed = EquipmentManager.Instance?.GetTotalPrimaryBonus(EquipmentSlot.Ring) ?? 0f;
            float currentAtkSpeed = (GameManager.Instance != null ? GameManager.Instance.Stats.AtkSpeed : 1.0f) * (1f + eqSpeed);
            if (isRage) currentAtkSpeed *= 2f;

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
            float eqAtk = EquipmentManager.Instance?.GetTotalPrimaryBonus(EquipmentSlot.Weapon) ?? 0f;
            float damage = (GameManager.Instance != null ? GameManager.Instance.Stats.Atk : 10f) + eqAtk;
            float bonusCrit = (FamiliarManager.Instance?.GetCritChanceBonus() ?? 0f) + (EquipmentManager.Instance?.GetTotalSecondaryBonus("Crit") ?? 0f);
            bool isCrit = isRage || (bonusCrit > 0f && GD.Randf() < bonusCrit);
            if (isCrit)
            {
                float critMult = 2f + (FamiliarManager.Instance?.GetCritDamageBonus() ?? 0f);
                damage *= critMult;
            }

            // Slash tween animation with forward tilt and spring recoil
            if (_visualRoot != null)
            {
                var tween = CreateTween().SetParallel(true);
                tween.TweenProperty(_visualRoot, "position:x", 35f, 0.07f)
                     .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                tween.TweenProperty(_visualRoot, "rotation", 0.08f, 0.07f);

                var backTween = CreateTween().SetParallel(true);
                backTween.TweenProperty(_visualRoot, "position:x", 0f, 0.12f).SetDelay(0.07f);
                backTween.TweenProperty(_visualRoot, "rotation", 0f, 0.12f).SetDelay(0.07f);
            }

            // Spawn Slash VFX & SFX
            FXManager.Instance?.PlaySlash(target.GlobalPosition + new Vector2(-20, -35), isRage);
            AudioManager.Instance?.PlaySlash();

            // Screen shake & hit freeze per binding game feel rules
            bool isBoss = target is BossEnemy;
            if (isRage)
            {
                // In Berserk mode: NO per-hit trauma (fixed 0.15 base rumble handled by CameraShake)
                // and NO hit-freeze during Berserk!
            }
            else
            {
                if (isCrit)
                {
                    CameraShake.Instance?.AddTrauma(0.20f);
                    if (isBoss)
                    {
                        FXManager.Instance?.TriggerLocalHitFreeze(this, target, 0.040f);
                    }
                }
                else
                {
                    CameraShake.Instance?.AddTrauma(0.10f);
                }
            }

            target.TakeDamage(damage, isCrit);

            // Lifesteal
            if (GameManager.Instance != null)
            {
                float eqLifesteal = (EquipmentManager.Instance?.GetTotalPrimaryBonus(EquipmentSlot.Amulet) ?? 0f) / 100f;
                float lifestealRate = (GameManager.Instance.Stats.LifestealPercent / 100f) + eqLifesteal;
                float heal = damage * lifestealRate;
                Heal(heal);

                // Rage accumulation
                float rageMult = AwakeningManager.Instance?.GetRageGainMultiplier() ?? 1.0f;
                float relicRage = RelicManager.Instance?.GetRageGainBonus() ?? 0f;
                GameManager.Instance.AddRage(2.0f * rageMult * (1.0f + relicRage));
            }
        }

        public void Heal(float amount)
        {
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
            UpdateHealthUI();
        }

        public void TakeDamage(float amount)
        {
            if (GameManager.Instance?.Profile?.Bloodline == BloodlineType.SteelFleshed)
            {
                amount = Mathf.Max(1f, amount - 3f);
            }

            float reduction = FamiliarManager.Instance?.GetDamageReductionBonus() ?? 0f;
            if (reduction > 0f) amount = Mathf.Max(1f, amount * (1f - reduction));

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
                AudioManager.Instance?.PlayHeroDeath();
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
