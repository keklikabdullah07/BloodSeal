#nullable enable
using Godot;
using System;
using System.Linq;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class RelicVaultModal : Control
    {
        [Export] public Button? CloseBtn;
        [Export] public Label? HeaderStatusLabel;
        [Export] public VBoxContainer? RelicListContainer;

        // Showcase / Inspection Detail Panel
        [Export] public Label? DetailIconLabel;
        [Export] public Label? DetailNameLabel;
        [Export] public Label? DetailWaveLabel;
        [Export] public Label? DetailBonusLabel;
        [Export] public Label? DetailStatusBadge;
        [Export] public RichTextLabel? DetailLoreLabel;

        private RelicDefinition? _selectedRelic;

        public override void _Ready()
        {
            Visible = false;
            CloseBtn?.Connect("pressed", Callable.From(CloseModal));

            if (RelicManager.Instance != null)
            {
                RelicManager.Instance.OnRelicUnlocked += _ => RefreshUI();
            }
        }

        public void ShowModal()
        {
            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.2f);

            var all = RelicDatabase.AllRelics;
            _selectedRelic = all.FirstOrDefault(r => RelicManager.Instance?.HasRelic(r.Id) == true) ?? (all.Count > 0 ? all[0] : null);

            RefreshUI();
        }

        public void CloseModal()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.15f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        public void RefreshUI()
        {
            var rm = RelicManager.Instance;
            int count = rm?.GetCollectedCount() ?? 0;
            int total = RelicDatabase.AllRelics.Count;

            if (HeaderStatusLabel != null)
            {
                HeaderStatusLabel.Text = $"📜 Kurtarılan Eserler: {count} / {total}  |  Kalıcı Gotik Miras (Uyanışta Kaybolmaz)";
            }

            BuildList();
            UpdateDetailPanel();
        }

        private void BuildList()
        {
            if (RelicListContainer == null) return;

            foreach (Node child in RelicListContainer.GetChildren())
            {
                child.QueueFree();
            }

            var rm = RelicManager.Instance;
            foreach (var relic in RelicDatabase.AllRelics)
            {
                bool isUnlocked = rm?.HasRelic(relic.Id) == true;
                bool isSelected = _selectedRelic?.Id == relic.Id;

                var btn = new Button
                {
                    CustomMinimumSize = new Vector2(0, 44),
                    Alignment = HorizontalAlignment.Left
                };

                string iconStr = isUnlocked ? relic.IconSymbol : "🔒";
                string nameStr = isUnlocked ? relic.Name : "Kilitli Kadim Eser";
                string bonusTag = isUnlocked ? $"[{relic.BonusDisplay}]" : $"[Dalga {relic.MilestoneWave} Boss'u]";

                btn.Text = $" {iconStr}  {nameStr} - {bonusTag}";

                if (isSelected)
                {
                    btn.Modulate = new Color(1.3f, 0.9f, 0.4f);
                }
                else if (isUnlocked)
                {
                    btn.Modulate = new Color(1.0f, 0.95f, 0.9f);
                }
                else
                {
                    btn.Modulate = new Color(0.55f, 0.55f, 0.55f);
                }

                var r = relic;
                btn.Connect("pressed", Callable.From(() =>
                {
                    _selectedRelic = r;
                    RefreshUI();
                }));

                RelicListContainer.AddChild(btn);
            }
        }

        private void UpdateDetailPanel()
        {
            if (_selectedRelic == null) return;
            var rm = RelicManager.Instance;
            bool isUnlocked = rm?.HasRelic(_selectedRelic.Id) == true;

            if (DetailIconLabel != null)
            {
                DetailIconLabel.Text = isUnlocked ? _selectedRelic.IconSymbol : "🔒";
            }

            if (DetailNameLabel != null)
            {
                DetailNameLabel.Text = isUnlocked ? _selectedRelic.Name : "Mühürlü Eser";
                DetailNameLabel.Modulate = isUnlocked ? new Color(1.0f, 0.85f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
            }

            if (DetailWaveLabel != null)
            {
                DetailWaveLabel.Text = $"Dalga {_selectedRelic.MilestoneWave} Muhafızı Kalıntısı";
            }

            if (DetailBonusLabel != null)
            {
                DetailBonusLabel.Text = isUnlocked
                    ? $"✨ Kalıcı Pasif Etki: {_selectedRelic.BonusDisplay}"
                    : $"🔒 Kilitli Etki: {_selectedRelic.BonusDisplay}";
                DetailBonusLabel.Modulate = isUnlocked ? new Color(0.4f, 1.0f, 0.5f) : new Color(0.7f, 0.7f, 0.7f);
            }

            if (DetailStatusBadge != null)
            {
                DetailStatusBadge.Text = isUnlocked ? "✓ KURTARILDI (ETKİN)" : "✗ KİLİTLİ (HENÜZ BULUNMADI)";
                DetailStatusBadge.Modulate = isUnlocked ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.85f, 0.3f, 0.3f);
            }

            if (DetailLoreLabel != null)
            {
                DetailLoreLabel.Text = isUnlocked
                    ? $"[i]\"{_selectedRelic.LoreText}\"[/i]"
                    : $"[i][color=#888888]\"Bu kalıntının geçmişi kan ve karanlığın ardında saklıdır. Dalga {_selectedRelic.MilestoneWave} Boss'unu yenerek gerçeği ortaya çıkar.\"[/color][/i]";
            }
        }
    }
}
