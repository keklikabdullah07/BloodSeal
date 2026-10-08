using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class MainHUD : CanvasLayer
    {
        [Export] public Label GoldLabel;
        [Export] public Label WaveLabel;
        [Export] public Label ProfileLabel;
        [Export] public Button RageButton;
        [Export] public ProgressBar RageProgressBar;
        [Export] public Button RetryBossButton;
        [Export] public ColorRect RageVignetteRect;

        // Pentagram Buttons
        [Export] public Button UpgradeAtkBtn;
        [Export] public Button UpgradeAtkSpdBtn;
        [Export] public Button UpgradeLifestealBtn;
        [Export] public Button UpgradeMaxHpBtn;
        [Export] public Button UpgradeRangeBtn;

        // Manor Gate
        [Export] public Button GateNotificationBtn;
        [Export] public ManorGateModal GateModal;

        // Manor Library
        [Export] public Button LibraryBtn;
        [Export] public LibraryModal LibraryModal;

        // Awakening / Rebirth
        [Export] public Button AwakeningBtn;
        [Export] public AwakeningModal AwakeningModal;

        // Lore Relics Vault
        [Export] public Button RelicVaultBtn;
        [Export] public RelicVaultModal RelicVaultModal;

        private Action<RelicDefinition> _onRelicUnlockedHandler;

        public override void _Ready()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += UpdateGoldUI;
                GameManager.Instance.OnWaveChanged += UpdateWaveUI;
                GameManager.Instance.OnRageChanged += UpdateRageUI;
                GameManager.Instance.OnRageStateChanged += UpdateRageStateUI;
                GameManager.Instance.OnStatsUpgraded += UpdateAllStatButtons;
                GameManager.Instance.OnProfileChanged += UpdateProfileUI;
                GameManager.Instance.OnGateNotificationAvailable += UpdateGateNotificationUI;
                GameManager.Instance.OnRuneEquipped += _ => UpdateGateNotificationUI();

                UpdateGoldUI(GameManager.Instance.Gold);
                UpdateWaveUI(GameManager.Instance.CurrentWave, GameManager.Instance.CurrentWave % 10 == 0);
                UpdateRageUI(GameManager.Instance.RagePercentage);
                UpdateRageStateUI(GameManager.Instance.IsRageActive);
                UpdateProfileUI(GameManager.Instance.Profile);
                UpdateGateNotificationUI();
                UpdateLibraryNotificationUI();
                UpdateAwakeningNotificationUI();
                UpdateRelicVaultNotificationUI();
                UpdateAllStatButtons();
            }

            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.OnLoreScrollsChanged += _ => UpdateLibraryNotificationUI();
            }

            if (AwakeningManager.Instance != null)
            {
                AwakeningManager.Instance.OnAwakeningPointsChanged += _ => UpdateAwakeningNotificationUI();
                AwakeningManager.Instance.OnAwakened += UpdateAwakeningNotificationUI;
            }

            if (RelicManager.Instance != null)
            {
                _onRelicUnlockedHandler = _ => UpdateRelicVaultNotificationUI();
                RelicManager.Instance.OnRelicUnlocked += _onRelicUnlockedHandler;
            }

            // Connect button signals
            RageButton?.Connect("pressed", Callable.From(OnRagePressed));
            RetryBossButton?.Connect("pressed", Callable.From(OnRetryBossPressed));
            GateNotificationBtn?.Connect("pressed", Callable.From(() => GateModal?.ShowModal()));
            LibraryBtn?.Connect("pressed", Callable.From(() => LibraryModal?.ShowModal()));
            AwakeningBtn?.Connect("pressed", Callable.From(() => AwakeningModal?.ShowModal()));
            RelicVaultBtn?.Connect("pressed", Callable.From(() => RelicVaultModal?.ShowModal()));
            UpgradeAtkBtn?.Connect("pressed", Callable.From(() => GameManager.Instance?.UpgradeAtk()));
            UpgradeAtkSpdBtn?.Connect("pressed", Callable.From(() => GameManager.Instance?.UpgradeAtkSpeed()));
            UpgradeLifestealBtn?.Connect("pressed", Callable.From(() => GameManager.Instance?.UpgradeLifesteal()));
            UpgradeMaxHpBtn?.Connect("pressed", Callable.From(() => GameManager.Instance?.UpgradeMaxHp()));
            UpgradeRangeBtn?.Connect("pressed", Callable.From(() => GameManager.Instance?.UpgradeRange()));
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -= UpdateGoldUI;
                GameManager.Instance.OnWaveChanged -= UpdateWaveUI;
                GameManager.Instance.OnRageChanged -= UpdateRageUI;
                GameManager.Instance.OnRageStateChanged -= UpdateRageStateUI;
                GameManager.Instance.OnStatsUpgraded -= UpdateAllStatButtons;
                GameManager.Instance.OnProfileChanged -= UpdateProfileUI;
                GameManager.Instance.OnGateNotificationAvailable -= UpdateGateNotificationUI;
            }
            if (RelicManager.Instance != null && _onRelicUnlockedHandler != null)
            {
                RelicManager.Instance.OnRelicUnlocked -= _onRelicUnlockedHandler;
            }
        }

        private void UpdateGateNotificationUI()
        {
            if (GateNotificationBtn == null || GameManager.Instance == null) return;

            bool isAvailable = GameManager.Instance.CurrentWave >= ManorGateHelper.GateUnlockWave
                               && !GameManager.Instance.HasClaimedGateReward;

            GateNotificationBtn.Visible = isAvailable;
        }

        private void UpdateLibraryNotificationUI()
        {
            if (LibraryBtn == null || GameManager.Instance == null) return;

            bool isAvailable = GameManager.Instance.CurrentWave >= ManorGateHelper.GateUnlockWave
                               || GameManager.Instance.HasClaimedGateReward
                               || (ResearchManager.Instance != null && ResearchManager.Instance.LoreScrolls > 0);

            LibraryBtn.Visible = isAvailable;
        }

        private void UpdateAwakeningNotificationUI()
        {
            if (AwakeningBtn == null || GameManager.Instance == null) return;
            bool isAvailable = GameManager.Instance.CurrentWave >= AwakeningManager.MinimumAwakeningWave
                               || (AwakeningManager.Instance != null && (AwakeningManager.Instance.AwakeningPoints > 0 || AwakeningManager.Instance.TotalAwakenings > 0));

            AwakeningBtn.Visible = isAvailable;
            if (isAvailable && AwakeningManager.Instance != null)
            {
                int pending = AwakeningManager.Instance.CalculatePendingPoints(GameManager.Instance.CurrentWave);
                if (pending > 0)
                    AwakeningBtn.Text = $"🩸 UYANIŞ (+{pending} AP)";
                else if (AwakeningManager.Instance.AwakeningPoints > 0)
                    AwakeningBtn.Text = $"✨ MÜHÜRLER ({AwakeningManager.Instance.AwakeningPoints} AP)";
                else
                    AwakeningBtn.Text = "🩸 KIZIL UYANIŞ";
            }
        }

        private void UpdateProfileUI(CharacterProfile profile)
        {
            if (ProfileLabel != null && profile != null)
            {
                string bl = CharacterProfile.GetBloodlineName(profile.Bloodline);
                string or = CharacterProfile.GetOriginName(profile.Origin);
                string runeTag = "";
                if (GameManager.Instance != null && GameManager.Instance.ActiveRune != RuneType.None)
                {
                    runeTag = $" | ᚱ {ManorGateHelper.GetRuneName(GameManager.Instance.ActiveRune)}";
                }
                ProfileLabel.Text = $"👤 {profile.PlayerName} [{bl} | {or}{runeTag}]";
            }
        }

        private void UpdateGoldUI(double gold)
        {
            if (GoldLabel != null) GoldLabel.Text = $"🪙 {BigNumberFormatter.Format(gold)} Altın";
            UpdateAllStatButtons();
        }

        private void UpdateWaveUI(int wave, bool isBoss)
        {
            if (WaveLabel != null)
            {
                if (isBoss) WaveLabel.Text = $"⚠️ BOSS SAVAŞI: DALGA {wave} ⚠️";
                else if (GameManager.Instance != null && GameManager.Instance.IsInSafeFarmMode) WaveLabel.Text = $"⚔️ GÜVENLİ FARM: DALGA {wave} ⚔️";
                else WaveLabel.Text = $"DALGA {wave} / {((wave / 10) + 1) * 10}";
            }

            if (RetryBossButton != null) RetryBossButton.Visible = GameManager.Instance != null && GameManager.Instance.IsInSafeFarmMode;

            UpdateLibraryNotificationUI();
            UpdateAwakeningNotificationUI();
            UpdateRelicVaultNotificationUI();
        }

        private void UpdateRelicVaultNotificationUI()
        {
            if (RelicVaultBtn == null || GameManager.Instance == null) return;
            int count = RelicManager.Instance?.GetCollectedCount() ?? 0;
            bool isAvailable = count > 0 || GameManager.Instance.CurrentWave >= 10;
            RelicVaultBtn.Visible = isAvailable;
            if (isAvailable) RelicVaultBtn.Text = $"🏛️ MAHZEN ({count}/10)";
        }

        private void UpdateRageUI(float percentage)
        {
            if (RageProgressBar != null) RageProgressBar.Value = percentage;
            if (RageButton != null)
            {
                bool isRage = GameManager.Instance != null && GameManager.Instance.IsRageActive;
                RageButton.Disabled = percentage < 100f || isRage;
                RageButton.Text = isRage ? "BERSERK AKTİF!" : (percentage >= 100f ? "ÖFKEYİ SERBEST BIRAK! 🔥" : $"Öfke: %{percentage:F0}");
            }
        }

        private void UpdateRageStateUI(bool isActive)
        {
            if (RageVignetteRect != null) RageVignetteRect.Visible = isActive;
            if (GameManager.Instance != null) UpdateRageUI(GameManager.Instance.RagePercentage);
        }

        private void UpdateAllStatButtons()
        {
            if (GameManager.Instance == null) return;
            var stats = GameManager.Instance.Stats;
            double gold = GameManager.Instance.Gold;

            UpdateButton(UpgradeAtkBtn, "ATK (Güç)", stats.AtkLevel, stats.Atk, stats.GetAtkCost(), gold);
            UpdateButton(UpgradeAtkSpdBtn, "HIZ", stats.AtkSpeedLevel, stats.AtkSpeed, stats.GetAtkSpeedCost(), gold, "/s");
            UpdateButton(UpgradeLifestealBtn, "CAN ÇALMA", stats.LifestealLevel, stats.LifestealPercent, stats.GetLifestealCost(), gold, "%");
            UpdateButton(UpgradeMaxHpBtn, "MAX CAN", stats.MaxHpLevel, stats.MaxHp, stats.GetMaxHpCost(), gold);
            UpdateButton(UpgradeRangeBtn, "MENZİL", stats.RangeLevel, stats.Range, stats.GetRangeCost(), gold, "px");
        }

        private void UpdateButton(Button btn, string statName, int lvl, float val, double cost, double currentGold, string unit = "")
        {
            if (btn == null) return;
            btn.Text = $"{statName} Lv.{lvl}\n({val:F1}{unit})\n🪙 {BigNumberFormatter.Format(cost)}";
            btn.Disabled = currentGold < cost;
        }

        private void OnRagePressed() => GameManager.Instance?.TriggerRage();
        private void OnRetryBossPressed() => GameManager.Instance?.RetryBoss();
    }
}
