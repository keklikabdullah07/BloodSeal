#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class AwakeningModal : Control
    {
        [Export] public Button? CloseBtn;
        [Export] public Button? WarTabBtn;
        [Export] public Button? MomentumTabBtn;
        [Export] public Button? HeritageTabBtn;

        [Export] public Label? HeaderStatusLabel;
        [Export] public Label? RitualStatusLabel;
        [Export] public Button? ExecuteAwakeningBtn;
        [Export] public VBoxContainer? TreeCardsContainer;

        private AwakeningBranch _currentBranch = AwakeningBranch.War;

        public override void _Ready()
        {
            Visible = false;
            CloseBtn?.Connect("pressed", Callable.From(CloseModal));
            WarTabBtn?.Connect("pressed", Callable.From(() => SwitchBranch(AwakeningBranch.War)));
            MomentumTabBtn?.Connect("pressed", Callable.From(() => SwitchBranch(AwakeningBranch.Momentum)));
            HeritageTabBtn?.Connect("pressed", Callable.From(() => SwitchBranch(AwakeningBranch.Heritage)));
            ExecuteAwakeningBtn?.Connect("pressed", Callable.From(OnExecuteAwakeningPressed));

            if (AwakeningManager.Instance != null)
            {
                AwakeningManager.Instance.OnAwakeningPointsChanged += _ => RefreshUI();
                AwakeningManager.Instance.OnSealUpgraded += (_, _) => RefreshUI();
                AwakeningManager.Instance.OnAwakened += RefreshUI;
            }
        }

        public void ShowModal()
        {
            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.2f);
            AudioManager.Instance?.PlayModalOpen();
            SwitchBranch(_currentBranch);
        }

        public void CloseModal()
        {
            AudioManager.Instance?.PlayModalClose();
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.15f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        public void SwitchBranch(AwakeningBranch branch)
        {
            _currentBranch = branch;
            UpdateTabStyles();
            BuildCards();
            RefreshUI();
        }

        private void UpdateTabStyles()
        {
            HighlightButton(WarTabBtn, _currentBranch == AwakeningBranch.War);
            HighlightButton(MomentumTabBtn, _currentBranch == AwakeningBranch.Momentum);
            HighlightButton(HeritageTabBtn, _currentBranch == AwakeningBranch.Heritage);
        }

        private void HighlightButton(Button? btn, bool active)
        {
            if (btn == null) return;
            btn.Modulate = active ? new Color(1.3f, 0.85f, 0.4f) : new Color(0.7f, 0.7f, 0.7f);
        }

        private void RefreshUI()
        {
            var am = AwakeningManager.Instance;
            var gm = GameManager.Instance;
            if (am == null || gm == null) return;

            int currentWave = gm.CurrentWave;
            int pending = am.CalculatePendingPoints(currentWave);

            if (HeaderStatusLabel != null)
            {
                HeaderStatusLabel.Text = $"⚔️ Dalga: {currentWave}  |  ✨ AP: {am.AwakeningPoints}  |  📜 Uyanışlar: {am.TotalAwakenings}";
            }

            bool canAwaken = am.CanAwaken(currentWave);
            if (RitualStatusLabel != null)
            {
                RitualStatusLabel.Text = canAwaken
                    ? $"⚡ UYANIŞA HAZIR!\nKazanılacak AP: +{pending} Puan\n(Dalga {currentWave})"
                    : $"🔒 MÜHÜR UYKUDA\nGereksinim: Dalga 20+\n(Mevcut: Dalga {currentWave})";
                RitualStatusLabel.Modulate = canAwaken ? new Color(1.0f, 0.35f, 0.35f) : new Color(0.6f, 0.6f, 0.6f);
            }

            if (ExecuteAwakeningBtn != null)
            {
                ExecuteAwakeningBtn.Disabled = !canAwaken;
                ExecuteAwakeningBtn.Text = canAwaken ? $"KIZIL UYANIŞI BAŞLAT (+{pending} AP)" : "DALGA 20'YE ULAŞ";
            }

            BuildCards();
        }

        private void BuildCards()
        {
            if (TreeCardsContainer == null) return;
            foreach (var child in TreeCardsContainer.GetChildren())
            {
                child.QueueFree();
            }

            var am = AwakeningManager.Instance;
            if (am == null) return;

            var nodes = AwakeningDatabase.GetNodesForBranch(_currentBranch);
            foreach (var node in nodes)
            {
                var card = CreateNodeCard(node, am);
                TreeCardsContainer.AddChild(card);
            }
        }

        private Control CreateNodeCard(AwakeningNodeDefinition node, AwakeningManager am)
        {
            int currentLvl = am.GetSealLevel(node.Id);
            bool isMax = currentLvl >= node.MaxLevel;
            int nextLvl = currentLvl + 1;
            int cost = isMax ? 0 : node.GetCost(nextLvl);
            bool canAfford = !isMax && am.AwakeningPoints >= cost;

            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(0, 72);
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.04f, 0.07f, 0.95f),
                BorderColor = isMax ? new Color(0.9f, 0.7f, 0.2f) : new Color(0.5f, 0.2f, 0.25f),
                BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4, CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4
            };
            panel.AddThemeStyleboxOverride("panel", style);

            var hbox = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            panel.AddChild(hbox);

            var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            hbox.AddChild(vbox);

            var nameLabel = new Label
            {
                Text = $"{node.Name} (Lv.{currentLvl}/{node.MaxLevel})",
                Modulate = isMax ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.4f, 0.4f)
            };
            vbox.AddChild(nameLabel);

            float currentBonus = node.GetEffectValue(currentLvl);
            string bonusText = node.Unit == "%" ? $"+{currentBonus * 100f:F1}%" : $"+{BigNumberFormatter.Format(currentBonus)}{node.Unit}";
            var descLabel = new Label
            {
                Text = $"{node.Description} [Mevcut: {bonusText}]",
                Modulate = new Color(0.75f, 0.75f, 0.75f)
            };
            vbox.AddChild(descLabel);

            var upgradeBtn = new Button
            {
                CustomMinimumSize = new Vector2(130, 40),
                Disabled = isMax || !canAfford,
                Text = isMax ? "MAKSİMUM" : $"YÜKSELT\n{cost} AP"
            };
            upgradeBtn.Connect("pressed", Callable.From(() => am.TryUpgradeSeal(node.Id)));
            hbox.AddChild(upgradeBtn);

            return panel;
        }

        private void OnExecuteAwakeningPressed()
        {
            var am = AwakeningManager.Instance;
            var gm = GameManager.Instance;
            if (am == null || gm == null) return;

            bool success = am.ExecuteAwakening(gm.CurrentWave, () => gm.ResetForAwakening());
            if (success)
            {
                AudioManager.Instance?.PlayAwakeningRitual();
                CloseModal();
            }
        }
    }
}
