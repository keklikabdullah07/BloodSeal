#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using BloodSeal.Core;
using BloodSeal.Combat;

namespace BloodSeal.UI
{
    public partial class QuestModal : Control
    {
        private Button? _closeBtn;
        private Button? _milestonesTabBtn;
        private Button? _dailyTabBtn;
        private VBoxContainer? _questListContainer;
        private QuestCategory _currentTab = QuestCategory.Milestone;

        public override void _Ready()
        {
            Visible = false;
            BuildUI();
        }

        public void ShowModal()
        {
            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.2f);

            AudioManager.Instance?.PlayModalOpen();
            RefreshList();
        }

        public void CloseModal()
        {
            AudioManager.Instance?.PlayModalClose();
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.15f);
            tween.TweenCallback(Callable.From(() =>
            {
                Visible = false;
                GameManager.Instance?.SaveGame();
            }));
        }

        private void SetTab(QuestCategory category)
        {
            _currentTab = category;
            AudioManager.Instance?.PlayButtonClick();
            UpdateTabStyles();
            RefreshList();
        }

        private void UpdateTabStyles()
        {
            if (_milestonesTabBtn != null)
                _milestonesTabBtn.Modulate = _currentTab == QuestCategory.Milestone ? Colors.White : new Color(0.6f, 0.6f, 0.6f);
            if (_dailyTabBtn != null)
                _dailyTabBtn.Modulate = _currentTab == QuestCategory.Daily ? Colors.White : new Color(0.6f, 0.6f, 0.6f);
        }

        public void RefreshList()
        {
            if (_questListContainer == null) return;
            foreach (var child in _questListContainer.GetChildren()) child.QueueFree();

            var qm = QuestManager.Instance;
            if (qm == null) return;

            var quests = _currentTab == QuestCategory.Milestone
                ? QuestDatabase.GetMilestones()
                : qm.ActiveDailyQuestIds.Select(id => QuestDatabase.GetQuestById(id)).OfType<QuestDefinition>().ToList();

            foreach (var q in quests)
            {
                if (q == null) continue;
                var progress = qm.GetProgress(q.Id);
                var card = CreateQuestCard(q, progress);
                _questListContainer.AddChild(card);
            }
        }

        private PanelContainer CreateQuestCard(QuestDefinition def, QuestProgress progress)
        {
            var card = new PanelContainer { CustomMinimumSize = new Vector2(740, 78) };
            var box = new StyleBoxFlat
            {
                BgColor = new Color(0.10f, 0.07f, 0.12f, 0.95f),
                BorderColor = progress.IsClaimed ? new Color(0.35f, 0.35f, 0.35f) : (progress.IsCompleted(def.TargetAmount) ? new Color(0.9f, 0.75f, 0.2f) : new Color(0.6f, 0.15f, 0.2f)),
                BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6
            };
            card.AddThemeStyleboxOverride("panel", box);

            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 14); margin.AddThemeConstantOverride("margin_right", 14);
            margin.AddThemeConstantOverride("margin_top", 10); margin.AddThemeConstantOverride("margin_bottom", 10);
            card.AddChild(margin);

            var hBox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            margin.AddChild(hBox);

            var infoBox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            hBox.AddChild(infoBox);

            var titleLbl = new Label { Text = def.Title };
            titleLbl.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
            titleLbl.AddThemeFontSizeOverride("font_size", 16);
            infoBox.AddChild(titleLbl);

            var descLbl = new Label { Text = $"{def.Description} ({Math.Min(progress.CurrentAmount, def.TargetAmount)} / {def.TargetAmount})" };
            descLbl.AddThemeColorOverride("font_color", new Color(0.8f, 0.78f, 0.82f));
            descLbl.AddThemeFontSizeOverride("font_size", 13);
            infoBox.AddChild(descLbl);

            // Rewards string
            var rewardsStr = string.Join(" | ", def.Rewards.Select(r => r.Type switch
            {
                QuestRewardType.Gold => $"🪙 +{BigNumberFormatter.Format(r.Amount)}",
                QuestRewardType.LoreScrolls => $"📜 +{r.Amount}",
                QuestRewardType.AwakeningPoints => $"🩸 +{r.Amount} AP",
                _ => ""
            }));
            var rewardLbl = new Label { Text = rewardsStr };
            rewardLbl.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
            rewardLbl.AddThemeFontSizeOverride("font_size", 14);
            hBox.AddChild(rewardLbl);

            var claimBtn = new Button { CustomMinimumSize = new Vector2(130, 42) };
            if (progress.IsClaimed)
            {
                claimBtn.Text = "✓ Alındı"; claimBtn.Disabled = true;
            }
            else if (progress.IsCompleted(def.TargetAmount))
            {
                claimBtn.Text = "Talep Et ✨";
                claimBtn.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.4f));
                claimBtn.Connect("pressed", Callable.From(() => ClaimQuest(def.Id, claimBtn)));
            }
            else
            {
                claimBtn.Text = "Sürüyor..."; claimBtn.Disabled = true;
            }
            hBox.AddChild(claimBtn);
            return card;
        }

        private void ClaimQuest(string questId, Button btn)
        {
            var qm = QuestManager.Instance;
            if (qm == null) return;

            bool claimed = qm.ClaimReward(questId, (type, amount) =>
            {
                switch (type)
                {
                    case QuestRewardType.Gold:
                        GameManager.Instance?.AddGold(amount);
                        break;
                    case QuestRewardType.LoreScrolls:
                        ResearchManager.Instance?.AddLoreScrolls((int)amount);
                        break;
                    case QuestRewardType.AwakeningPoints:
                        AwakeningManager.Instance?.AddAwakeningPoints((int)amount);
                        break;
                }
            });

            if (claimed)
            {
                AudioManager.Instance?.PlayCoin();
                AudioManager.Instance?.PlayRelicUnlock();
                FloatingTextManager.Instance?.SpawnMessage(btn.GlobalPosition + new Vector2(0, -30), "ÖDÜL KAZANILDI! ✨", new Color(1f, 0.9f, 0.2f));
                RefreshList();
                GameManager.Instance?.SaveGame();
            }
        }

        private void BuildUI()
        {
            if (GetChildCount() > 0) return;
            SetAnchorsPreset(LayoutPreset.FullRect);

            var dim = new ColorRect { Color = new Color(0.04f, 0.02f, 0.06f, 0.88f), MouseFilter = MouseFilterEnum.Stop };
            dim.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(dim);

            var center = new CenterContainer();
            center.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(center);

            var panel = new PanelContainer { CustomMinimumSize = new Vector2(800, 560) };
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.07f, 0.05f, 0.09f, 0.98f),
                BorderColor = new Color(0.75f, 0.18f, 0.22f, 1f),
                BorderWidthBottom = 3, BorderWidthLeft = 3, BorderWidthRight = 3, BorderWidthTop = 3,
                CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10, CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10
            };
            panel.AddThemeStyleboxOverride("panel", style);
            center.AddChild(panel);

            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 24); margin.AddThemeConstantOverride("margin_right", 24);
            margin.AddThemeConstantOverride("margin_top", 18); margin.AddThemeConstantOverride("margin_bottom", 18);
            panel.AddChild(margin);

            var vBox = new VBoxContainer { CustomMinimumSize = new Vector2(750, 520) };
            margin.AddChild(vBox);

            // Header
            var headerHBox = new HBoxContainer();
            vBox.AddChild(headerHBox);
            var title = new Label { Text = "📜 KAN GÖREVLERİ & BAŞARIMLAR" };
            title.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.35f));
            title.AddThemeFontSizeOverride("font_size", 22);
            title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            headerHBox.AddChild(title);

            _closeBtn = new Button { Text = "✕", CustomMinimumSize = new Vector2(36, 36) };
            _closeBtn.Connect("pressed", Callable.From(CloseModal));
            headerHBox.AddChild(_closeBtn);

            // Tabs
            var tabsHBox = new HBoxContainer { CustomMinimumSize = new Vector2(0, 42) };
            vBox.AddChild(tabsHBox);
            _milestonesTabBtn = new Button { Text = "⚔️ Kalıcı Başarımlar", SizeFlagsHorizontal = SizeFlags.ExpandFill };
            _milestonesTabBtn.Connect("pressed", Callable.From(() => SetTab(QuestCategory.Milestone)));
            tabsHBox.AddChild(_milestonesTabBtn);

            _dailyTabBtn = new Button { Text = "🩸 Günlük Kan Avı (24s)", SizeFlagsHorizontal = SizeFlags.ExpandFill };
            _dailyTabBtn.Connect("pressed", Callable.From(() => SetTab(QuestCategory.Daily)));
            tabsHBox.AddChild(_dailyTabBtn);

            // Scroll List
            var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(750, 410), SizeFlagsVertical = SizeFlags.ExpandFill };
            vBox.AddChild(scroll);
            _questListContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            scroll.AddChild(_questListContainer);

            UpdateTabStyles();
        }
    }
}
