#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using BloodSeal.Core;
using BloodSeal.Combat;

namespace BloodSeal.UI
{
    public partial class LibraryModal : Control
    {
        [Export] public Button? EconTabBtn;
        [Export] public Button? MemoryTabBtn;
        [Export] public Button? WarTabBtn;
        [Export] public Button? CloseBtn;

        [Export] public Label? GoldLabel;
        [Export] public Label? ScrollsLabel;
        [Export] public Label? CategoryTitleLabel;
        [Export] public VBoxContainer? CardsContainer;

        private ResearchDiscipline _currentDiscipline = ResearchDiscipline.Economy;
        private readonly List<Button> _upgradeButtons = new();

        public override void _Ready()
        {
            Visible = false;

            EconTabBtn?.Connect("pressed", Callable.From(() => SwitchTab(ResearchDiscipline.Economy)));
            MemoryTabBtn?.Connect("pressed", Callable.From(() => SwitchTab(ResearchDiscipline.BloodMemory)));
            WarTabBtn?.Connect("pressed", Callable.From(() => SwitchTab(ResearchDiscipline.CombatEsotericism)));
            CloseBtn?.Connect("pressed", Callable.From(CloseModal));

            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.OnResearchUpgraded += (_, _) => RefreshUI();
                ResearchManager.Instance.OnLoreScrollsChanged += _ => RefreshUI();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += _ => RefreshUI();
            }
        }

        public void ShowModal()
        {
            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.22f);

            SwitchTab(_currentDiscipline);
        }

        public void CloseModal()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.18f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        public void SwitchTab(ResearchDiscipline discipline)
        {
            _currentDiscipline = discipline;
            UpdateTabStyles();
            BuildCards();
            RefreshUI();
        }

        private void UpdateTabStyles()
        {
            HighlightButton(EconTabBtn, _currentDiscipline == ResearchDiscipline.Economy);
            HighlightButton(MemoryTabBtn, _currentDiscipline == ResearchDiscipline.BloodMemory);
            HighlightButton(WarTabBtn, _currentDiscipline == ResearchDiscipline.CombatEsotericism);

            if (CategoryTitleLabel != null)
            {
                CategoryTitleLabel.Text = _currentDiscipline switch
                {
                    ResearchDiscipline.Economy => "🪙 KADİM EKONOMİ DİSİPLİNİ",
                    ResearchDiscipline.BloodMemory => "⏳ KAN HAFIZASI & ÇEVRİMDIŞI DİSİPLİNİ",
                    ResearchDiscipline.CombatEsotericism => "⚔️ SAVAŞ EZOTERİZMİ DİSİPLİNİ",
                    _ => "ARAŞTIRMA KÜRSÜSÜ"
                };
            }
        }

        private void HighlightButton(Button? btn, bool isSelected)
        {
            if (btn == null) return;
            btn.Modulate = isSelected ? new Color(1.2f, 1.1f, 0.8f, 1f) : new Color(0.7f, 0.7f, 0.75f, 0.9f);
        }

        public void RefreshUI()
        {
            if (!Visible) return;

            if (GoldLabel != null && GameManager.Instance != null)
            {
                GoldLabel.Text = $"🪙 {BigNumberFormatter.Format(GameManager.Instance.Gold)} Altın";
            }

            if (ScrollsLabel != null)
            {
                int scrolls = ResearchManager.Instance?.LoreScrolls ?? 0;
                ScrollsLabel.Text = $"📜 {scrolls} Kadim Parşömen";
            }

            BuildCards();
        }

        private void BuildCards()
        {
            if (CardsContainer == null) return;

            foreach (var child in CardsContainer.GetChildren())
            {
                child.QueueFree();
            }
            _upgradeButtons.Clear();

            var allNodes = ResearchDatabase.GetAllNodes();
            double currentGold = GameManager.Instance?.Gold ?? 0.0;
            int currentScrolls = ResearchManager.Instance?.LoreScrolls ?? 0;

            foreach (var node in allNodes)
            {
                if (node.Discipline != _currentDiscipline) continue;

                int currentLvl = ResearchManager.Instance?.GetResearchLevel(node.Id) ?? 0;
                bool isMax = currentLvl >= node.MaxLevel;
                int nextLvl = currentLvl + 1;
                double goldCost = isMax ? 0 : node.GetGoldCost(nextLvl);
                int scrollCost = isMax ? 0 : node.GetScrollCost(nextLvl);
                bool canAfford = !isMax && currentGold >= goldCost && currentScrolls >= scrollCost;

                var cardPanel = new PanelContainer();
                cardPanel.CustomMinimumSize = new Vector2(0, 95);
                var style = new StyleBoxFlat
                {
                    BgColor = new Color(0.08f, 0.06f, 0.11f, 0.92f),
                    BorderColor = isMax ? new Color(0.85f, 0.7f, 0.2f, 0.8f) : new Color(0.25f, 0.18f, 0.3f, 0.85f),
                    BorderWidthLeft = 2,
                    BorderWidthRight = 2,
                    BorderWidthTop = 2,
                    BorderWidthBottom = 2,
                    CornerRadiusBottomLeft = 6,
                    CornerRadiusBottomRight = 6,
                    CornerRadiusTopLeft = 6,
                    CornerRadiusTopRight = 6
                };
                cardPanel.AddThemeStyleboxOverride("panel", style);

                var margin = new MarginContainer();
                margin.AddThemeConstantOverride("margin_left", 16);
                margin.AddThemeConstantOverride("margin_right", 16);
                margin.AddThemeConstantOverride("margin_top", 10);
                margin.AddThemeConstantOverride("margin_bottom", 10);
                cardPanel.AddChild(margin);

                var hBox = new HBoxContainer();
                hBox.AddThemeConstantOverride("separation", 16);
                margin.AddChild(hBox);

                var infoBox = new VBoxContainer();
                infoBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                infoBox.AddThemeConstantOverride("separation", 4);
                hBox.AddChild(infoBox);

                var titleLabel = new Label();
                string lvlTag = isMax ? "[MAKSİMUM SEVİYE]" : $"Lv.{currentLvl}/{node.MaxLevel}";
                titleLabel.Text = $"{node.Name}  •  {lvlTag}";
                titleLabel.AddThemeColorOverride("font_color", isMax ? new Color(1f, 0.85f, 0.3f) : new Color(0.95f, 0.9f, 0.95f));
                titleLabel.AddThemeFontSizeOverride("font_size", 16);
                infoBox.AddChild(titleLabel);

                var descLabel = new Label();
                float currentVal = node.GetEffectValue(currentLvl);
                float nextVal = isMax ? currentVal : node.GetEffectValue(nextLvl);
                string bonusText = isMax ? $"Mevcut: +{currentVal:F1}{node.Unit}" : $"Mevcut: +{currentVal:F1}{node.Unit} ➜ Sonraki: +{nextVal:F1}{node.Unit}";
                descLabel.Text = $"{node.Description}\n{bonusText}";
                descLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.72f, 0.8f));
                descLabel.AddThemeFontSizeOverride("font_size", 13);
                infoBox.AddChild(descLabel);

                var actionBox = new VBoxContainer();
                actionBox.CustomMinimumSize = new Vector2(170, 0);
                actionBox.Alignment = BoxContainer.AlignmentMode.Center;
                hBox.AddChild(actionBox);

                var costLabel = new Label();
                costLabel.HorizontalAlignment = HorizontalAlignment.Center;
                costLabel.AddThemeFontSizeOverride("font_size", 13);
                if (isMax)
                {
                    costLabel.Text = "Tamamlandı";
                    costLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.85f, 0.5f));
                }
                else
                {
                    string scrollPart = scrollCost > 0 ? $" + 📜 {scrollCost}" : "";
                    costLabel.Text = $"🪙 {BigNumberFormatter.Format(goldCost)}{scrollPart}";
                    costLabel.AddThemeColorOverride("font_color", canAfford ? new Color(1f, 0.85f, 0.3f) : new Color(0.85f, 0.4f, 0.4f));
                }
                actionBox.AddChild(costLabel);

                var upgradeBtn = new Button();
                upgradeBtn.CustomMinimumSize = new Vector2(160, 38);
                upgradeBtn.Text = isMax ? "USTALAŞILDI" : "ARAŞTIR";
                upgradeBtn.Disabled = !canAfford;
                string nodeId = node.Id;
                upgradeBtn.Connect("pressed", Callable.From(() => OnUpgradeClicked(nodeId)));
                actionBox.AddChild(upgradeBtn);
                _upgradeButtons.Add(upgradeBtn);

                CardsContainer.AddChild(cardPanel);
            }
        }

        private void OnUpgradeClicked(string nodeId)
        {
            var node = ResearchDatabase.GetNode(nodeId);
            if (node == null || GameManager.Instance == null) return;

            int nextLvl = (ResearchManager.Instance?.GetResearchLevel(nodeId) ?? 0) + 1;
            double goldCost = node.GetGoldCost(nextLvl);
            int scrollCost = node.GetScrollCost(nextLvl);

            int currentScrolls = ResearchManager.Instance?.LoreScrolls ?? 0;
            if (GameManager.Instance.Gold >= goldCost && currentScrolls >= scrollCost)
            {
                if (GameManager.Instance.SpendGold(goldCost))
                {
                    double dummy = goldCost;
                    ResearchManager.Instance?.TryUpgradeResearch(nodeId, ref dummy);

                    Core.AudioManager.Instance?.PlayHit();
                    FloatingTextManager.Instance?.SpawnMessage(
                        new Vector2(960, 400),
                        $"✨ {node.Name} ARAŞTIRILDI! (Lv.{nextLvl})",
                        new Color(1f, 0.85f, 0.3f)
                    );

                    GameManager.Instance.SaveGame();
                    RefreshUI();
                }
            }
        }
    }
}
