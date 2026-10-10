#nullable enable
using Godot;
using System;

namespace BloodSeal.UI
{
    /// <summary>
    /// Mobil cihazlardaki kamera çentiği (notch), delikli kamera ve yuvarlatılmış köşeler
    /// için HUD kenar boşluklarını (safe area) dinamik olarak günceller.
    /// </summary>
    public partial class SafeAreaHandler : Node
    {
        [Export] public MarginContainer? TopBarMargin;
        [Export] public MarginContainer? BottomBarMargin;

        private int _baseTopLeft = 20;
        private int _baseTopRight = 20;
        private int _baseTopTop = 10;
        private int _baseBottomLeft = 20;
        private int _baseBottomRight = 20;
        private int _baseBottomBottom = 10;

        public override void _Ready()
        {
            FindContainersIfNeeded();
            CacheBaseMargins();
            ApplySafeArea();

            var root = GetTree().Root;
            if (root != null)
            {
                root.SizeChanged += OnScreenResized;
            }
        }

        public override void _ExitTree()
        {
            var root = GetTree()?.Root;
            if (root != null)
            {
                root.SizeChanged -= OnScreenResized;
            }
        }

        private void OnScreenResized()
        {
            ApplySafeArea();
        }

        private void FindContainersIfNeeded()
        {
            if (TopBarMargin == null)
            {
                TopBarMargin = GetNodeOrNull<MarginContainer>("../MainHUD/TopBar/Margin")
                            ?? GetTree().Root.FindChild("TopBarMargin", true, false) as MarginContainer;
            }

            if (BottomBarMargin == null)
            {
                BottomBarMargin = GetNodeOrNull<MarginContainer>("../MainHUD/BottomPanel/Margin")
                               ?? GetTree().Root.FindChild("BottomBarMargin", true, false) as MarginContainer;
            }
        }

        private void CacheBaseMargins()
        {
            if (TopBarMargin != null)
            {
                _baseTopLeft = TopBarMargin.GetThemeConstant("margin_left");
                _baseTopRight = TopBarMargin.GetThemeConstant("margin_right");
                _baseTopTop = TopBarMargin.GetThemeConstant("margin_top");
            }

            if (BottomBarMargin != null)
            {
                _baseBottomLeft = BottomBarMargin.GetThemeConstant("margin_left");
                _baseBottomRight = BottomBarMargin.GetThemeConstant("margin_right");
                _baseBottomBottom = BottomBarMargin.GetThemeConstant("margin_bottom");
            }
        }

        public void ApplySafeArea()
        {
            try
            {
                var safeRect = DisplayServer.GetDisplaySafeArea();
                var winSize = DisplayServer.WindowGetSize();

                if (winSize.X <= 0 || winSize.Y <= 0) return;

                int rawLeft = safeRect.Position.X;
                int rawRight = winSize.X - (safeRect.Position.X + safeRect.Size.X);
                int rawTop = safeRect.Position.Y;
                int rawBottom = winSize.Y - (safeRect.Position.Y + safeRect.Size.Y);

                float scaleX = 1920.0f / winSize.X;
                float scaleY = 1080.0f / winSize.Y;

                int insetLeft = Mathf.Max(0, (int)(rawLeft * scaleX));
                int insetRight = Mathf.Max(0, (int)(rawRight * scaleX));
                int insetTop = Mathf.Max(0, (int)(rawTop * scaleY));
                int insetBottom = Mathf.Max(0, (int)(rawBottom * scaleY));

                if (TopBarMargin != null)
                {
                    TopBarMargin.AddThemeConstantOverride("margin_left", _baseTopLeft + insetLeft);
                    TopBarMargin.AddThemeConstantOverride("margin_right", _baseTopRight + insetRight);
                    TopBarMargin.AddThemeConstantOverride("margin_top", _baseTopTop + insetTop);
                }

                if (BottomBarMargin != null)
                {
                    BottomBarMargin.AddThemeConstantOverride("margin_left", _baseBottomLeft + insetLeft);
                    BottomBarMargin.AddThemeConstantOverride("margin_right", _baseBottomRight + insetRight);
                    BottomBarMargin.AddThemeConstantOverride("margin_bottom", _baseBottomBottom + insetBottom);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SafeAreaHandler hata: {ex.Message}");
            }
        }
    }
}
