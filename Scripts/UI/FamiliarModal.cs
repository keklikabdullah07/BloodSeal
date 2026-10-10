#nullable enable
using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class FamiliarModal : Control
    {
        private Button? _closeBtn;
        private Label? _activeSummaryLabel;
        private VBoxContainer? _listContainer;

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
            RefreshUI();
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

        private void BuildUI()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);
            var overlay = new ColorRect { Color = new Color(0.02f, 0.01f, 0.03f, 0.85f) };
            overlay.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(overlay);

            var center = new CenterContainer();
            center.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(center);

            var panel = new PanelContainer { CustomMinimumSize = new Vector2(860, 680) };
            var panelBox = new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.05f, 0.10f, 0.98f),
                BorderColor = new Color(0.75f, 0.2f, 0.25f),
                BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2,
                CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10, CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10
            };
            panel.AddThemeStyleboxOverride("panel", panelBox);
            center.AddChild(panel);

            var rootVbox = new VBoxContainer();
            rootVbox.AddThemeConstantOverride("separation", 12);
            panel.AddChild(rootVbox);

            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 24); margin.AddThemeConstantOverride("margin_right", 24);
            margin.AddThemeConstantOverride("margin_top", 20); margin.AddThemeConstantOverride("margin_bottom", 20);
            panel.RemoveChild(rootVbox);
            panel.AddChild(margin);
            margin.AddChild(rootVbox);

            var header = new HBoxContainer();
            var title = new Label { Text = "🦇 KAN BAĞI YOLDAŞLARI" };
            title.AddThemeFontSizeOverride("font_size", 22);
            title.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));
            header.AddChild(title);

            var spacer = new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            header.AddChild(spacer);

            _closeBtn = new Button { Text = "✖ KAPAT", CustomMinimumSize = new Vector2(100, 36) };
            _closeBtn.Connect("pressed", Callable.From(CloseModal));
            header.AddChild(_closeBtn);
            rootVbox.AddChild(header);

            _activeSummaryLabel = new Label { Text = "Aktif Yoldaş: ..." };
            _activeSummaryLabel.AddThemeFontSizeOverride("font_size", 14);
            _activeSummaryLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.9f));
            rootVbox.AddChild(_activeSummaryLabel);

            var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(810, 490), SizeFlagsVertical = SizeFlags.ExpandFill };
            _listContainer = new VBoxContainer();
            _listContainer.AddThemeConstantOverride("separation", 10);
            scroll.AddChild(_listContainer);
            rootVbox.AddChild(scroll);
        }

        public void RefreshUI()
        {
            var fm = FamiliarManager.Instance;
            if (fm == null || _listContainer == null) return;

            var activeDef = FamiliarDatabase.Get(fm.ActiveFamiliarId);
            var activeProg = fm.GetProgress(fm.ActiveFamiliarId);
            if (_activeSummaryLabel != null)
            {
                _activeSummaryLabel.Text = $"Aktif: {activeDef.Name} ({activeDef.Title}) — Seviye {activeProg.Level} (Tier {activeProg.Tier})\n{activeDef.PassiveDescription}";
            }

            foreach (var child in _listContainer.GetChildren()) child.QueueFree();

            foreach (var def in FamiliarDatabase.GetAll())
            {
                var prog = fm.GetProgress(def.Id);
                var card = CreateFamiliarCard(def, prog, fm);
                _listContainer.AddChild(card);
            }
        }

        private PanelContainer CreateFamiliarCard(FamiliarDefinition def, FamiliarProgress prog, FamiliarManager fm)
        {
            var card = new PanelContainer { CustomMinimumSize = new Vector2(790, 105) };
            bool isActive = fm.ActiveFamiliarId == def.Id;
            var auraCol = new Color(def.AuraColorHex);

            var box = new StyleBoxFlat
            {
                BgColor = new Color(0.12f, 0.08f, 0.14f, 0.95f),
                BorderColor = isActive ? new Color(1f, 0.85f, 0.2f) : (prog.IsUnlocked ? auraCol : new Color(0.35f, 0.35f, 0.35f)),
                BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8
            };
            card.AddThemeStyleboxOverride("panel", box);

            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 14); margin.AddThemeConstantOverride("margin_right", 14);
            margin.AddThemeConstantOverride("margin_top", 10); margin.AddThemeConstantOverride("margin_bottom", 10);
            card.AddChild(margin);

            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 16);
            margin.AddChild(hbox);

            var infoVbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var nameLbl = new Label { Text = $"{def.Name} - {def.Title}  [Lv. {prog.Level} / Tier {prog.Tier}]" };
            nameLbl.AddThemeFontSizeOverride("font_size", 16);
            nameLbl.AddThemeColorOverride("font_color", isActive ? new Color(1f, 0.85f, 0.3f) : Colors.White);
            infoVbox.AddChild(nameLbl);

            var descLbl = new Label { Text = $"{def.Description}\n{def.PassiveDescription}" };
            descLbl.AddThemeFontSizeOverride("font_size", 12);
            descLbl.AddThemeColorOverride("font_color", new Color(0.8f, 0.75f, 0.85f));
            infoVbox.AddChild(descLbl);
            hbox.AddChild(infoVbox);

            var actionVbox = new VBoxContainer { CustomMinimumSize = new Vector2(170, 0), Alignment = BoxContainer.AlignmentMode.Center };
            actionVbox.AddThemeConstantOverride("separation", 6);

            if (!prog.IsUnlocked)
            {
                var lockLbl = new Label { Text = $"🔒 Dalga {def.UnlockWaveRequirement}'te Açılır", HorizontalAlignment = HorizontalAlignment.Center };
                lockLbl.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                lockLbl.AddThemeFontSizeOverride("font_size", 13);
                actionVbox.AddChild(lockLbl);
            }
            else
            {
                var equipBtn = new Button { Text = isActive ? "✨ KUŞANILDI" : "KUŞAN", CustomMinimumSize = new Vector2(160, 32) };
                equipBtn.Disabled = isActive;
                equipBtn.Connect("pressed", Callable.From(() =>
                {
                    fm.SetActiveFamiliar(def.Id);
                    AudioManager.Instance?.PlayButtonClick();
                    RefreshUI();
                }));
                actionVbox.AddChild(equipBtn);

                double goldCost = fm.GetGoldCost(def.Id);
                int parchCost = fm.GetParchmentCost(def.Id);
                string costText = parchCost > 0 ? $"{FormatNumber(goldCost)} 🪙 + {parchCost} 📜" : $"{FormatNumber(goldCost)} 🪙";

                var upBtn = new Button { Text = $"YÜKSELT ({costText})", CustomMinimumSize = new Vector2(160, 32) };
                double currentGold = GameManager.Instance?.Gold ?? 0.0;
                int currentParch = ResearchManager.Instance?.LoreScrolls ?? 0;
                upBtn.Disabled = !fm.CanUpgrade(def.Id, currentGold, currentParch);

                upBtn.Connect("pressed", Callable.From(() =>
                {
                    double g = GameManager.Instance?.Gold ?? 0.0;
                    int p = ResearchManager.Instance?.LoreScrolls ?? 0;
                    double goldCost = fm.GetGoldCost(def.Id);
                    int parchCost = fm.GetParchmentCost(def.Id);

                    if (fm.CanUpgrade(def.Id, g, p))
                    {
                        if (GameManager.Instance != null && GameManager.Instance.SpendGold(goldCost))
                        {
                            if (parchCost > 0) ResearchManager.Instance?.SpendLoreScrolls(parchCost);
                            double dummyG = double.MaxValue;
                            int dummyP = int.MaxValue;
                            fm.Upgrade(def.Id, ref dummyG, ref dummyP);
                            AudioManager.Instance?.PlayCoin();
                            RefreshUI();
                        }
                    }
                }));
                actionVbox.AddChild(upBtn);
            }

            hbox.AddChild(actionVbox);
            return card;
        }

        private static string FormatNumber(double n)
        {
            if (n >= 1_000_000_000) return $"{n / 1_000_000_000:0.#}B";
            if (n >= 1_000_000) return $"{n / 1_000_000:0.#}M";
            if (n >= 1_000) return $"{n / 1_000:0.#}K";
            return $"{n:0}";
        }
    }
}
