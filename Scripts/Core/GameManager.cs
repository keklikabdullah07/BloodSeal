using Godot;
using System;
using BloodSeal.Combat;

namespace BloodSeal.Core
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        public PentagramStats Stats { get; private set; } = new PentagramStats();
        public CharacterProfile Profile { get; private set; } = new CharacterProfile();

        public double Gold { get; private set; } = 100.0; // Başlangıç testi için 100 altın
        public int CurrentWave { get; private set; } = 1;
        public int HighestWave { get; private set; } = 1;
        public bool IsInSafeFarmMode { get; private set; } = false;
        public float RagePercentage { get; private set; } = 0f;
        public bool IsRageActive { get; private set; } = false;

        public event Action<double> OnGoldChanged;
        public event Action<int, bool> OnWaveChanged;
        public event Action<float> OnRageChanged;
        public event Action<bool> OnRageStateChanged;
        public event Action OnHeroDied;
        public event Action OnStatsUpgraded;
        public event Action<CharacterProfile> OnProfileChanged;

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

        public void AddGold(double amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool SpendGold(double amount)
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
            double cost = Stats.GetAtkCost();
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
            double cost = Stats.GetAtkSpeedCost();
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
            double cost = Stats.GetLifestealCost();
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
            double cost = Stats.GetMaxHpCost();
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
            double cost = Stats.GetRangeCost();
            if (SpendGold(cost))
            {
                Stats.RangeLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public void SetProfile(string name, BloodlineType bloodline, StreetOriginType origin)
        {
            Profile.PlayerName = string.IsNullOrWhiteSpace(name) ? "Valerius" : name.Trim();
            Profile.Bloodline = bloodline;
            Profile.Origin = origin;
            Profile.HasCompletedPrologue = true;
            OnProfileChanged?.Invoke(Profile);
            OnStatsUpgraded?.Invoke();
        }
    }
}
