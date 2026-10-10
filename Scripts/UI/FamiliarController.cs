#nullable enable
using Godot;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class FamiliarController : CanvasLayer
    {
        private Button? _familiarBtn;
        private FamiliarModal? _familiarModal;

        public override void _Ready()
        {
            Layer = 103;

            _familiarModal = new FamiliarModal();
            AddChild(_familiarModal);

            CreateHUDButton();

            if (FamiliarManager.Instance != null)
            {
                FamiliarManager.Instance.OnActiveFamiliarChanged += OnFamiliarChanged;
                FamiliarManager.Instance.OnFamiliarUnlocked += OnFamiliarUnlocked;
            }

            Callable.From(RefreshButtonUI).CallDeferred();
        }

        public override void _ExitTree()
        {
            if (FamiliarManager.Instance != null)
            {
                FamiliarManager.Instance.OnActiveFamiliarChanged -= OnFamiliarChanged;
                FamiliarManager.Instance.OnFamiliarUnlocked -= OnFamiliarUnlocked;
            }
        }

        private void CreateHUDButton()
        {
            _familiarBtn = new Button
            {
                Text = "🦇 YOLDAŞ",
                CustomMinimumSize = new Vector2(120, 44),
                Position = new Vector2(1570, 16)
            };

            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.16f, 0.08f, 0.20f, 0.95f),
                BorderColor = new Color(0.6f, 0.25f, 0.75f, 1f),
                BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8
            };
            _familiarBtn.AddThemeStyleboxOverride("normal", style);
            _familiarBtn.AddThemeFontSizeOverride("font_size", 14);
            _familiarBtn.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));

            _familiarBtn.Connect("pressed", Callable.From(() =>
            {
                AudioManager.Instance?.PlayButtonClick();
                _familiarModal?.ShowModal();
            }));

            AddChild(_familiarBtn);
        }

        private void OnFamiliarChanged(string id) => RefreshButtonUI();
        private void OnFamiliarUnlocked(string id) => RefreshButtonUI();

        private void RefreshButtonUI()
        {
            if (_familiarBtn == null) return;
            var fm = FamiliarManager.Instance;
            if (fm == null) return;

            var activeDef = FamiliarDatabase.Get(fm.ActiveFamiliarId);
            _familiarBtn.Text = "🦇 YOLDAŞ";
            _familiarBtn.TooltipText = $"Aktif: {activeDef.Name}";
        }
    }
}
