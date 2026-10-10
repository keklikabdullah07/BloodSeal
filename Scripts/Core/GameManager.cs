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

        public OfflineEarningsResult PendingOfflineEarnings { get; private set; }

        // Manor Gate & Runes (GDD Bölüm 5)
        public RuneType ActiveRune { get; set; } = RuneType.None;
        public GateApproachType SelectedGateApproach { get; set; } = GateApproachType.None;
        public bool HasEncounteredGate { get; set; } = false;
        public bool HasClaimedGateReward { get; set; } = false;
        public bool HasFirstLoreScroll { get; set; } = false;

        public event Action<double> OnGoldChanged;
        public event Action<int, bool> OnWaveChanged;
        public event Action<float> OnRageChanged;
        public event Action<bool> OnRageStateChanged;
        public event Action OnHeroDied;
        public event Action OnStatsUpgraded;
        public event Action<CharacterProfile> OnProfileChanged;
        public event Action<OfflineEarningsResult> OnOfflineEarningsReady;
        public event Action<RuneType> OnRuneEquipped;
        public event Action OnGateNotificationAvailable;

        private double _rageActiveTimer = 0.0;
        private double _autoSaveTimer = 30.0;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
                Stats.Profile = Profile;
            }
            else QueueFree();
        }

        public override void _Ready()
        {
            LoadGame();
            QuestManager.Instance?.CheckAndRefreshDailyQuests(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
        public override void _ExitTree() { if (Instance == this) SaveGame(); }

        public void LoadGame()
        {
            var data = SaveSystem.Load();
            if (data == null) return;
            var offline = SaveSystem.CalculateOfflineEarnings(data);
            SaveSystem.ApplySaveData(data, this);
            if (offline != null && offline.HasClaimableEarnings)
            {
                PendingOfflineEarnings = offline;
                Callable.From(() => OnOfflineEarningsReady?.Invoke(offline)).CallDeferred();
            }
        }

        public void SaveGame() => SaveSystem.SaveAtomic(SaveSystem.CaptureSaveData(this));

        public void ClaimOfflineEarnings()
        {
            if (PendingOfflineEarnings == null || PendingOfflineEarnings.GoldEarned <= 0) return;
            AddGold(PendingOfflineEarnings.GoldEarned);
            PendingOfflineEarnings = null;
            SaveGame();
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
                double bonus = ResearchManager.Instance?.GetBerserkBonusDuration() ?? 0.0;
                _rageActiveTimer = 10.0 + bonus;
                AudioManager.Instance?.PlayRageBurst();
                OnRageStateChanged?.Invoke(true);
                QuestManager.Instance?.RecordBerserkActivated();
                return true;
            }
            return false;
        }

        public void SetWave(int wave, bool isSafeFarm = false)
        {
            CurrentWave = wave;
            IsInSafeFarmMode = isSafeFarm;
            if (wave > HighestWave) HighestWave = wave;
            QuestManager.Instance?.RecordWaveProgress(wave);
            FamiliarManager.Instance?.CheckWaveUnlocks(wave);
            bool isBoss = (wave % 10 == 0);
            OnWaveChanged?.Invoke(CurrentWave, isBoss);

            if (isBoss) AudioManager.Instance?.PlayBGM(BgmTrackType.BossCombat);
            else AudioManager.Instance?.PlayBGM(BgmTrackType.GothicAmbient);

            if (wave >= ManorGateHelper.GateUnlockWave && !HasClaimedGateReward)
                OnGateNotificationAvailable?.Invoke();

            SaveGame();
        }

        public void AdvanceWave() => SetWave(CurrentWave + 1, false);
        public void NotifyHeroDied() { OnHeroDied?.Invoke(); SetWave(Math.Max(1, CurrentWave - 1), true); }
        public void RetryBoss() { if (IsInSafeFarmMode) SetWave(((CurrentWave / 10) + 1) * 10, false); }

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
                QuestManager.Instance?.RecordStatUpgraded();
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
            Stats.Profile = Profile;
            OnProfileChanged?.Invoke(Profile);
            OnStatsUpgraded?.Invoke();
        }

        public double CalculateGoldReward(double baseGold)
        {
            double mult = 1.0;
            if (Profile?.Origin == StreetOriginType.StreetThief) mult += 0.15;
            if (ActiveRune == RuneType.WealthGreed) mult += 0.20;
            if (ResearchManager.Instance != null)
            {
                mult *= ResearchManager.Instance.GetGoldBountyMultiplier();
                if (CurrentWave % 10 == 0) mult *= ResearchManager.Instance.GetBossTributeMultiplier();
            }
            if (RelicManager.Instance != null) mult *= (1.0 + RelicManager.Instance.GetGoldBonus());
            if (FamiliarManager.Instance != null) mult *= (1.0 + FamiliarManager.Instance.GetGoldMultiplierBonus());
            return baseGold * mult;
        }

        public void ClaimGateReward(GateApproachType approach)
        {
            SelectedGateApproach = approach;
            ActiveRune = ManorGateHelper.GetRuneForApproach(approach);
            Stats.ActiveRune = ActiveRune;
            HasEncounteredGate = true;
            HasClaimedGateReward = true;
            if (approach == GateApproachType.BloodSeal)
            {
                HasFirstLoreScroll = true;
                ResearchManager.Instance?.AddLoreScrolls(1);
            }
            OnRuneEquipped?.Invoke(ActiveRune);
            OnStatsUpgraded?.Invoke();
            SaveGame();
        }

        public void ResetForAwakening()
        {
            Stats.ResetToDefaults();
            CurrentWave = 1;
            IsInSafeFarmMode = false;
            double startingBonus = AwakeningManager.Instance?.GetStartingGold() ?? 0.0;
            Gold = 100.0 + startingBonus;
            RagePercentage = 0f;
            IsRageActive = false;
            OnGoldChanged?.Invoke(Gold);
            OnWaveChanged?.Invoke(CurrentWave, false);
            OnRageChanged?.Invoke(0f);
            OnRageStateChanged?.Invoke(false);
            OnStatsUpgraded?.Invoke();
            SaveGame();
        }
    }
}
