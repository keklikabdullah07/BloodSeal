using Godot;
using System;
using BloodSeal.Combat;

namespace BloodSeal.Core
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        public PentagramStats Stats { get; private set; } = new PentagramStats();

        public long Gold { get; private set; } = 100; // Başlangıç testi için 100 altın
        public int CurrentWave { get; private set; } = 1;
        public int HighestWave { get; private set; } = 1;
        public bool IsInSafeFarmMode { get; private set; } = false;
        public float RagePercentage { get; private set; } = 0f;
        public bool IsRageActive { get; private set; } = false;

        public event Action<long> OnGoldChanged;
        public event Action<int, bool> OnWaveChanged;
        public event Action<float> OnRageChanged;
        public event Action<bool> OnRageStateChanged;
        public event Action OnHeroDied;
        public event Action OnStatsUpgraded;

        private double _rageActiveTimer = 0.0;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public override void _Process(double delta)
        {
            if (IsRageActive)
            {
                _rageActiveTimer -= delta;
                if (_rageActiveTimer <= 0)
                {
                    IsRageActive = false;
                    RagePercentage = 0f;
                    OnRageStateChanged?.Invoke(false);
                    OnRageChanged?.Invoke(0f);
                }
            }
        }

        public void AddGold(long amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool SpendGold(long amount)
        {
            if (Gold >= amount)
            {
                Gold -= amount;
                OnGoldChanged?.Invoke(Gold);
                return true;
            }
            return false;
        }

        public void AddRage(float amount)
        {
            if (IsRageActive) return;
            RagePercentage = Mathf.Clamp(RagePercentage + amount, 0f, 100f);
            OnRageChanged?.Invoke(RagePercentage);
        }

        public bool TriggerRage()
        {
            if (RagePercentage >= 100f && !IsRageActive)
            {
                IsRageActive = true;
                _rageActiveTimer = 10.0;
                OnRageStateChanged?.Invoke(true);
                return true;
            }
            return false;
        }

        public void SetWave(int wave, bool isSafeFarm = false)
        {
            CurrentWave = wave;
            IsInSafeFarmMode = isSafeFarm;
            if (wave > HighestWave) HighestWave = wave;
            bool isBoss = (wave % 10 == 0);
            OnWaveChanged?.Invoke(CurrentWave, isBoss);
        }

        public void AdvanceWave()
        {
            SetWave(CurrentWave + 1, false);
        }

        public void NotifyHeroDied()
        {
            OnHeroDied?.Invoke();
            // Boss veya zorlu dalgada ölürse güvenli farm dalgasına çekil
            int retreatWave = Math.Max(1, CurrentWave - 1);
            SetWave(retreatWave, true);
        }

        public void RetryBoss()
        {
            if (IsInSafeFarmMode)
            {
                int bossWave = ((CurrentWave / 10) + 1) * 10;
                SetWave(bossWave, false);
            }
        }

        public bool UpgradeAtk()
        {
            long cost = Stats.GetAtkCost();
            if (SpendGold(cost))
            {
                Stats.AtkLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeAtkSpeed()
        {
            long cost = Stats.GetAtkSpeedCost();
            if (SpendGold(cost))
            {
                Stats.AtkSpeedLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeLifesteal()
        {
            long cost = Stats.GetLifestealCost();
            if (SpendGold(cost))
            {
                Stats.LifestealLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeMaxHp()
        {
            long cost = Stats.GetMaxHpCost();
            if (SpendGold(cost))
            {
                Stats.MaxHpLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeRange()
        {
            long cost = Stats.GetRangeCost();
            if (SpendGold(cost))
            {
                Stats.RangeLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }
    }
}
