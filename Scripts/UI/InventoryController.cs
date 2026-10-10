#nullable enable
using Godot;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class InventoryController : CanvasLayer
    {
        private Button? _inventoryBtn;
        private InventoryModal? _inventoryModal;

        public override void _Ready()
        {
            Layer = 102;

            _inventoryModal = new InventoryModal();
            AddChild(_inventoryModal);

            CreateHUDButton();

            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged += RefreshButtonUI;
                EquipmentManager.Instance.OnItemAcquired += OnItemAcquired;
            }

            Callable.From(RefreshButtonUI).CallDeferred();
        }

        public override void _ExitTree()
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentChanged -= RefreshButtonUI;
                EquipmentManager.Instance.OnItemAcquired -= OnItemAcquired;
            }
        }

        private void CreateHUDButton()
        {
            _inventoryBtn = new Button
            {
                Text = "🛡️ ÇANTA",
                CustomMinimumSize = new Vector2(120, 44),
                Position = new Vector2(1430, 16)
            };

            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.12f, 0.08f, 0.16f, 0.95f),
                BorderColor = new Color(0.5f, 0.35f, 0.7f, 1f),
                BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8
            };
            _inventoryBtn.AddThemeStyleboxOverride("normal", style);
            _inventoryBtn.AddThemeFontSizeOverride("font_size", 14);
            _inventoryBtn.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));

            _inventoryBtn.Connect("pressed", Callable.From(() =>
            {
                AudioManager.Instance?.PlayButtonClick();
                _inventoryModal?.ShowModal();
            }));

            AddChild(_inventoryBtn);
        }

        private void OnItemAcquired(EquipmentItem item) => RefreshButtonUI();

        private void RefreshButtonUI()
        {
            if (_inventoryBtn == null) return;
            var em = EquipmentManager.Instance;
            if (em == null) return;

            int bagCount = em.BagItems.Count;
            _inventoryBtn.Text = bagCount > 0 ? $"🛡️ ÇANTA ({bagCount})" : "🛡️ ÇANTA";
        }
    }
}
