using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class BossEnemy : Enemy
    {
        public float EnrageMultiplier { get; private set; } = 1.0f;
        private double _enrageTimer = 0.0;
        private int _enrageSteps = 0;
        private float _baseAttackDamage = 0f;

        public override void Setup(int wave, Hero hero)
        {
            _heroTarget = hero;
            MoveSpeed = 80f;
            MaxHp = wave * 220f + 450f;
            CurrentHp = MaxHp;
            _baseAttackDamage = wave * 14f + 25f;
            AttackDamage = _baseAttackDamage;
            AddToGroup("Enemies");
            Scale = new Vector2(1.9f, 1.9f);
            UpdateHealthBar();
            TriggerHealthChanged(CurrentHp, MaxHp);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (IsDead) return;

            var enrageConfig = BalanceConfig.BossEnrage;
            double interval = enrageConfig.IntervalSeconds > 0 ? enrageConfig.IntervalSeconds : 5.0;

            _enrageTimer += delta;
            if (_enrageTimer >= interval)
            {
                _enrageTimer = 0.0;
                _enrageSteps++;

                float stepMultiplier = (float)enrageConfig.DamageStepMultiplier;
                if (enrageConfig.IsMultiplicative)
                {
                    EnrageMultiplier = Mathf.Pow(1.0f + stepMultiplier, _enrageSteps);
                }
                else
                {
                    EnrageMultiplier = 1.0f + (_enrageSteps * stepMultiplier);
                }

                AttackDamage = _baseAttackDamage * EnrageMultiplier;

                FloatingTextManager.Instance?.SpawnEnrage(GlobalPosition + new Vector2(0, -90), EnrageMultiplier);

                // Boss enrage aura turns darker blood crimson
                if (_visual != null)
                {
                    _visual.Modulate = new Color(1.2f, 0.4f / EnrageMultiplier, 0.4f / EnrageMultiplier);
                }
            }
        }

        protected override void Die()
        {
            if (IsDead) return;
            IsDead = true;

            // Boss death game feel: 0.50 trauma, local freeze, death explosion
            CameraShake.Instance?.AddTrauma(0.50f);
            FXManager.Instance?.TriggerLocalHitFreeze(null, this, 0.040f);
            FXManager.Instance?.PlayDeathExplosion(GlobalPosition + new Vector2(0, -35), true);
            AudioManager.Instance?.PlayBossVictory();

            double baseGold = GameManager.Instance.CurrentWave * 60 + 250;
            double bossGold = GameManager.Instance.CalculateGoldReward(baseGold);
            GameManager.Instance.AddGold(bossGold);
            FloatingTextManager.Instance?.SpawnGold(GlobalPosition + new Vector2(0, -40), bossGold);

            // Kadim Parşömen Düşüşü: İlk kesimde (Milestone) garanti, tekrarlarda %10 şans
            int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 10;
            var rm = ResearchManager.Instance;
            if (rm != null)
            {
                bool isMilestone = !rm.HasDefeatedMilestoneBoss(wave);
                bool dropsScroll = isMilestone || (GD.Randf() <= 0.10f);

                if (dropsScroll)
                {
                    rm.AddLoreScrolls(1);
                    if (isMilestone)
                    {
                        rm.RecordMilestoneBossDefeated(wave);
                        RelicManager.Instance?.UnlockRelicForWave(wave);
                        AudioManager.Instance?.PlayRelicUnlock();
                    }
                    FloatingTextManager.Instance?.SpawnMessage(
                        GlobalPosition + new Vector2(0, -75),
                        isMilestone ? "📜 Kadim Eser & Parşömen Bulundu!" : "📜 Kadim Parşömen Ele Geçirildi!",
                        new Color(0.95f, 0.85f, 0.3f)
                    );
                }
            }

            QueueFree();
        }
    }
}
