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
        public event Action<OfflineEarningsResult> OnOfflineEarningsReady;

        public OfflineEarningsResult PendingOfflineEarnings { get; private set; }

        private double _rageActiveTimer = 0.0;
        private double _autoSaveTimer = 30.0;

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

        public override void _Ready()
        {
            LoadGame();
        }

        public override void _ExitTree()
        {
            if (Instance == this)
            {
                SaveGame();
            }
        }

        public void LoadGame()
        {
            var data = SaveSystem.Load();
            if (data != null)
            {
                var offline = SaveSystem.CalculateOfflineEarnings(data);
                SaveSystem.ApplySaveData(data, this);
                if (offline != null && offline.HasClaimableEarnings)
                {
                    PendingOfflineEarnings = offline;
                    Callable.From(() => OnOfflineEarningsReady?.Invoke(offline)).CallDeferred();
                }
            }
        }

        public void SaveGame()
        {
            var data = SaveSystem.CaptureSaveData(this);
            SaveSystem.SaveAtomic(data);
        }

        public void ClaimOfflineEarnings()
        {
            if (PendingOfflineEarnings != null && PendingOfflineEarnings.GoldEarned > 0)
            {
                AddGold(PendingOfflineEarnings.GoldEarned);
                PendingOfflineEarnings = null;
                SaveGame();
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

            _autoSaveTimer -= delta;
            if (_autoSaveTimer <= 0.0)
            {
                _autoSaveTimer = 30.0;
                SaveGame();
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
            SaveGame();
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

        public bool UpgradeAtk() => TryUpgradeStat(Stats.GetAtkCost(), () => Stats.AtkLevel++);
        public bool UpgradeAtkSpeed() => TryUpgradeStat(Stats.GetAtkSpeedCost(), () => Stats.AtkSpeedLevel++);
        public bool UpgradeLifesteal() => TryUpgradeStat(Stats.GetLifestealCost(), () => Stats.LifestealLevel++);
        public bool UpgradeMaxHp() => TryUpgradeStat(Stats.GetMaxHpCost(), () => Stats.MaxHpLevel++);
        public bool UpgradeRange() => TryUpgradeStat(Stats.GetRangeCost(), () => Stats.RangeLevel++);

        private bool TryUpgradeStat(double cost, Action upgradeAction)
        {
            if (SpendGold(cost))
            {
                upgradeAction();
                OnStatsUpgraded?.Invoke();
                SaveGame();
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
