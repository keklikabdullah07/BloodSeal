using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class BossEnemy : Enemy
    {
        public float EnrageMultiplier { get; private set; } = 1.0f;
        private double _enrageTimer = 0.0;

        public override void Setup(int wave, Hero hero)
        {
            _heroTarget = hero;
            MoveSpeed = 80f;
            MaxHp = wave * 220f + 450f;
            CurrentHp = MaxHp;
            AttackDamage = wave * 14f + 25f;
            AddToGroup("Enemies");
            Scale = new Vector2(1.9f, 1.9f);
            UpdateHealthBar();
            TriggerHealthChanged(CurrentHp, MaxHp);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (IsDead) return;

            _enrageTimer += delta;
            if (_enrageTimer >= 5.0)
            {
                _enrageTimer = 0.0;
                EnrageMultiplier += 0.25f;
                AttackDamage *= 1.25f;

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

            long bossGold = GameManager.Instance.CurrentWave * 60 + 250;
            GameManager.Instance.AddGold(bossGold);
            FloatingTextManager.Instance?.SpawnGold(GlobalPosition + new Vector2(0, -40), bossGold);

            QueueFree();
        }
    }
}
