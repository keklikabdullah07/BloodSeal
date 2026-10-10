#nullable enable
using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class SettingsModal : Control
    {
        [Export] public Button? CloseBtn;
        [Export] public Button? HapticBtn;
        [Export] public HSlider? MasterSlider;
        [Export] public Label? MasterValueLabel;
        [Export] public CheckBox? MasterMuteCheck;
        [Export] public HSlider? BgmSlider;
        [Export] public Label? BgmValueLabel;
        [Export] public CheckBox? BgmMuteCheck;
        [Export] public HSlider? SfxSlider;
        [Export] public Label? SfxValueLabel;
        [Export] public CheckBox? SfxMuteCheck;

        private bool _isUpdatingUI = false;

        public override void _Ready()
        {
            Visible = false;
            BuildUIIfNeeded();
            WireEvents();
        }

        public void ShowModal()
        {
            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.2f);
            AudioManager.Instance?.PlayModalOpen();
            RefreshValuesFromAudio();
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

        private void WireEvents()
        {
            CloseBtn?.Connect("pressed", Callable.From(CloseModal));
            HapticBtn?.Connect("pressed", Callable.From(OnHapticToggled));

            if (MasterSlider != null)
            {
                MasterSlider.Connect("value_changed", Callable.From<double>(OnMasterChanged));
                MasterSlider.Connect("drag_ended", Callable.From<bool>(_ => PlayTestSound()));
            }
            if (BgmSlider != null) BgmSlider.Connect("value_changed", Callable.From<double>(OnBgmChanged));
            if (SfxSlider != null)
            {
                SfxSlider.Connect("value_changed", Callable.From<double>(OnSfxChanged));
                SfxSlider.Connect("drag_ended", Callable.From<bool>(_ => PlayTestSound()));
            }

            MasterMuteCheck?.Connect("toggled", Callable.From<bool>(OnMasterMuteToggled));
            BgmMuteCheck?.Connect("toggled", Callable.From<bool>(OnBgmMuteToggled));
            SfxMuteCheck?.Connect("toggled", Callable.From<bool>(OnSfxMuteToggled));
        }

        private void RefreshValuesFromAudio()
        {
            var audio = AudioManager.Instance;
            if (audio == null) return;

            _isUpdatingUI = true;
            if (MasterSlider != null) MasterSlider.Value = audio.MasterVolume * 100.0;
            if (MasterValueLabel != null) MasterValueLabel.Text = $"%{(int)(audio.MasterVolume * 100)}";
            if (MasterMuteCheck != null) MasterMuteCheck.ButtonPressed = audio.IsMuted;

            if (BgmSlider != null) BgmSlider.Value = audio.BgmVolume * 100.0;
            if (BgmValueLabel != null) BgmValueLabel.Text = $"%{(int)(audio.BgmVolume * 100)}";

            if (SfxSlider != null) SfxSlider.Value = audio.SfxVolume * 100.0;
            if (SfxValueLabel != null) SfxValueLabel.Text = $"%{(int)(audio.SfxVolume * 100)}";
            _isUpdatingUI = false;

            UpdateHapticButtonText();
        }

        private void OnMasterChanged(double val)
        {
            if (_isUpdatingUI) return;
            float v = (float)(val / 100.0);
            if (MasterValueLabel != null) MasterValueLabel.Text = $"%{(int)val}";
            AudioManager.Instance?.SetMasterVolume(v);
        }

        private void OnBgmChanged(double val)
        {
            if (_isUpdatingUI) return;
            float v = (float)(val / 100.0);
            if (BgmValueLabel != null) BgmValueLabel.Text = $"%{(int)val}";
            AudioManager.Instance?.SetBgmVolume(v);
        }

        private void OnSfxChanged(double val)
        {
            if (_isUpdatingUI) return;
            float v = (float)(val / 100.0);
            if (SfxValueLabel != null) SfxValueLabel.Text = $"%{(int)val}";
            AudioManager.Instance?.SetSfxVolume(v);
        }

        private void OnMasterMuteToggled(bool muted) { if (!_isUpdatingUI) AudioManager.Instance?.SetMuted(muted); }
        private void OnBgmMuteToggled(bool muted) { if (!_isUpdatingUI) AudioManager.Instance?.SetBusMute("BGM", muted); }
        private void OnSfxMuteToggled(bool muted) { if (!_isUpdatingUI) AudioManager.Instance?.SetBusMute("SFX", muted); }
        private void PlayTestSound() => AudioManager.Instance?.PlayButtonClick();

        private void OnHapticToggled()
        {
            var h = HapticManager.Instance;
            h.SetHapticsEnabled(!h.IsHapticsEnabled);
            UpdateHapticButtonText();
            PlayTestSound();
            if (h.IsHapticsEnabled) h.VibrateLight();
        }

        private void UpdateHapticButtonText()
        {
            if (HapticBtn == null) return;
            bool on = HapticManager.Instance.IsHapticsEnabled;
            HapticBtn.Text = on ? "📳 Titreşim (Haptik): [AÇIK]" : "📳 Titreşim (Haptik): [KAPALI]";
            HapticBtn.Modulate = on ? new Color(0.6f, 1.2f, 0.7f) : new Color(0.7f, 0.7f, 0.7f);
        }

        private void BuildUIIfNeeded()
        {
            if (GetChildCount() > 0) return;
            SetAnchorsPreset(LayoutPreset.FullRect);

            var dim = new ColorRect { Color = new Color(0.04f, 0.02f, 0.06f, 0.88f), MouseFilter = MouseFilterEnum.Stop };
            dim.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(dim);

            var center = new CenterContainer();
            center.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(center);

            var panel = new PanelContainer { CustomMinimumSize = new Vector2(620, 480) };
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.05f, 0.09f, 0.98f),
                BorderColor = new Color(0.7f, 0.2f, 0.25f, 1.0f),
                BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8
            };
            panel.AddThemeStyleboxOverride("panel", style);
            center.AddChild(panel);

            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 32); margin.AddThemeConstantOverride("margin_right", 32);
            margin.AddThemeConstantOverride("margin_top", 24); margin.AddThemeConstantOverride("margin_bottom", 24);
            panel.AddChild(margin);

            var vbox = new VBoxContainer { CustomMinimumSize = new Vector2(550, 420) };
            vbox.AddThemeConstantOverride("separation", 16);
            margin.AddChild(vbox);

            var title = new Label { Text = "⚙️ SES VE SİSTEM AYARLARI", HorizontalAlignment = HorizontalAlignment.Center, Modulate = new Color(1.3f, 0.85f, 0.35f) };
            vbox.AddChild(title);

            MasterSlider = CreateSliderRow(vbox, "Ana Ses (Master):", out MasterValueLabel, out MasterMuteCheck, "Sessize Al");
            BgmSlider = CreateSliderRow(vbox, "Müzik (BGM):", out BgmValueLabel, out BgmMuteCheck, "Müziği Kapat");
            SfxSlider = CreateSliderRow(vbox, "Ses Efektleri (SFX):", out SfxValueLabel, out SfxMuteCheck, "Efektleri Kapat");

            HapticBtn = new Button
            {
                Text = "📳 Titreşim (Haptik): [AÇIK]",
                CustomMinimumSize = new Vector2(300, 40),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter
            };
            vbox.AddChild(HapticBtn);

            CloseBtn = new Button
            {
                Text = "✕ AYARLARI KAPAT",
                CustomMinimumSize = new Vector2(200, 40),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter
            };
            vbox.AddChild(CloseBtn);
        }

        private HSlider CreateSliderRow(VBoxContainer parent, string titleText, out Label valueLabel, out CheckBox muteCheck, string muteText)
        {
            var row = new VBoxContainer();
            row.AddThemeConstantOverride("separation", 6);
            parent.AddChild(row);

            var topHBox = new HBoxContainer();
            row.AddChild(topHBox);
            topHBox.AddChild(new Label { Text = titleText, SizeFlagsHorizontal = SizeFlags.ExpandFill, Modulate = new Color(0.9f, 0.9f, 0.9f) });

            valueLabel = new Label { Text = "%100", CustomMinimumSize = new Vector2(50, 0), HorizontalAlignment = HorizontalAlignment.Right, Modulate = new Color(1.2f, 0.8f, 0.3f) };
            topHBox.AddChild(valueLabel);

            var sliderHBox = new HBoxContainer();
            sliderHBox.AddThemeConstantOverride("separation", 16);
            row.AddChild(sliderHBox);

            var slider = new HSlider { MinValue = 0, MaxValue = 100, Step = 1, Value = 100, SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(340, 24) };
            sliderHBox.AddChild(slider);

            muteCheck = new CheckBox { Text = muteText, Modulate = new Color(0.8f, 0.8f, 0.8f) };
            sliderHBox.AddChild(muteCheck);
            return slider;
        }
    }
}
