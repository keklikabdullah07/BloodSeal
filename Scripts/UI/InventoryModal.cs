#nullable enable
using Godot;
using System;
using System.Linq;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class InventoryModal : Control
    {
        private Button? _closeBtn;
        private VBoxContainer? _equippedContainer;
        private GridContainer? _bagGrid;
        private Label? _statsSummaryLabel;
        private PanelContainer? _itemDetailsPanel;
        private EquipmentItem? _selectedItem;

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

            var panel = new PanelContainer { CustomMinimumSize = new Vector2(920, 680) };
            panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.05f, 0.10f, 0.98f),
                BorderColor = new Color(0.75f, 0.2f, 0.25f),
                BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2,
                CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10, CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10
            });
            center.AddChild(panel);

            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 20); margin.AddThemeConstantOverride("margin_right", 20);
            margin.AddThemeConstantOverride("margin_top", 16); margin.AddThemeConstantOverride("margin_bottom", 16);
            panel.AddChild(margin);

            var rootVbox = new VBoxContainer();
            rootVbox.AddThemeConstantOverride("separation", 10);
            margin.AddChild(rootVbox);

            var header = new HBoxContainer();
            var title = new Label { Text = "🛡️ GOTİK ENVANTER & EKİPMANLAR" };
            title.AddThemeFontSizeOverride("font_size", 22);
            title.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));
            header.AddChild(title);
            header.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

            _closeBtn = new Button { Text = "✖ KAPAT", CustomMinimumSize = new Vector2(100, 36) };
            _closeBtn.Connect("pressed", Callable.From(CloseModal));
            header.AddChild(_closeBtn);
            rootVbox.AddChild(header);

            var contentHbox = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
            contentHbox.AddThemeConstantOverride("separation", 16);
            rootVbox.AddChild(contentHbox);

            var leftVbox = new VBoxContainer { CustomMinimumSize = new Vector2(340, 0) };
            leftVbox.AddThemeConstantOverride("separation", 8);
            contentHbox.AddChild(leftVbox);

            var eqTitle = new Label { Text = "Kuşanılmış Eşyalar:" };
            eqTitle.AddThemeFontSizeOverride("font_size", 15);
            eqTitle.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.6f));
            leftVbox.AddChild(eqTitle);

            _equippedContainer = new VBoxContainer();
            _equippedContainer.AddThemeConstantOverride("separation", 6);
            leftVbox.AddChild(_equippedContainer);

            _statsSummaryLabel = new Label();
            _statsSummaryLabel.AddThemeFontSizeOverride("font_size", 12);
            _statsSummaryLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.85f, 0.8f));
            leftVbox.AddChild(_statsSummaryLabel);

            var rightVbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            rightVbox.AddThemeConstantOverride("separation", 8);
            contentHbox.AddChild(rightVbox);

            var bagScroll = new ScrollContainer { CustomMinimumSize = new Vector2(510, 290) };
            _bagGrid = new GridContainer { Columns = 4 };
            _bagGrid.AddThemeConstantOverride("h_separation", 6);
            _bagGrid.AddThemeConstantOverride("v_separation", 6);
            bagScroll.AddChild(_bagGrid);
            rightVbox.AddChild(bagScroll);

            _itemDetailsPanel = new PanelContainer { CustomMinimumSize = new Vector2(510, 230), SizeFlagsVertical = SizeFlags.ExpandFill };
            rightVbox.AddChild(_itemDetailsPanel);
        }

        public void RefreshUI()
        {
            var em = EquipmentManager.Instance;
            if (em == null || _equippedContainer == null || _bagGrid == null || _statsSummaryLabel == null) return;

            foreach (var c in _equippedContainer.GetChildren()) c.QueueFree();
            foreach (var slot in new[] { EquipmentSlot.Weapon, EquipmentSlot.Armor, EquipmentSlot.Amulet, EquipmentSlot.Ring })
            {
                var item = em.EquippedItems[slot];
                var btn = CreateSlotButton(slot, item);
                _equippedContainer.AddChild(btn);
            }

            _statsSummaryLabel.Text = $"Toplam Ekipman Gücü:\n⚔️ +{em.GetTotalPrimaryBonus(EquipmentSlot.Weapon):0} ATK  |  🛡️ +{em.GetTotalPrimaryBonus(EquipmentSlot.Armor):0} HP\n📿 +{em.GetTotalPrimaryBonus(EquipmentSlot.Amulet):0.1}% Can Çalma  |  💍 +{em.GetTotalPrimaryBonus(EquipmentSlot.Ring)*100:0}% Atk Hızı\n🔥 Kritik: +{em.GetTotalSecondaryBonus("Crit")*100:0.1}%  |  💰 Altın: +{em.GetTotalSecondaryBonus("Gold")*100:0.1}%";

            foreach (var c in _bagGrid.GetChildren()) c.QueueFree();
            for (int i = 0; i < EquipmentManager.MaxBagCapacity; i++)
            {
                var item = i < em.BagItems.Count ? em.BagItems[i] : null;
                var slotBtn = CreateBagSlotButton(item);
                _bagGrid.AddChild(slotBtn);
            }

            RenderItemDetails();
        }

        private Button CreateSlotButton(EquipmentSlot slot, EquipmentItem? item)
        {
            string label = item == null ? $"[{slot}]: Boş" : $"[{slot}]: {EquipmentDatabase.Get(item.DefinitionId).Name} +{item.Level}";
            var btn = new Button { Text = label, CustomMinimumSize = new Vector2(330, 40) };
            if (item != null) btn.AddThemeColorOverride("font_color", new Color(item.GetRarityHex()));
            btn.Connect("pressed", Callable.From(() => { _selectedItem = item; RenderItemDetails(); }));
            return btn;
        }

        private Button CreateBagSlotButton(EquipmentItem? item)
        {
            string text = item == null ? "—" : $"{EquipmentDatabase.Get(item.DefinitionId).IconSymbol} +{item.Level}";
            var btn = new Button { Text = text, CustomMinimumSize = new Vector2(118, 42) };
            if (item != null) btn.AddThemeColorOverride("font_color", new Color(item.GetRarityHex()));
            btn.Connect("pressed", Callable.From(() => { _selectedItem = item; RenderItemDetails(); }));
            return btn;
        }

        private void RenderItemDetails()
        {
            if (_itemDetailsPanel == null) return;
            foreach (var c in _itemDetailsPanel.GetChildren()) c.QueueFree();

            if (_selectedItem == null)
            {
                var lbl = new Label { Text = "İncelemek için bir eşyaya tıklayın.", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                _itemDetailsPanel.AddChild(lbl);
                return;
            }

            var em = EquipmentManager.Instance;
            var def = EquipmentDatabase.Get(_selectedItem.DefinitionId);
            bool isEquipped = em != null && em.EquippedItems.Values.Any(i => i?.InstanceId == _selectedItem.InstanceId);

            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 6);
            _itemDetailsPanel.AddChild(vbox);

            var nameLbl = new Label { Text = $"{def.IconSymbol} {def.Name} (+{_selectedItem.Level}) — {_selectedItem.Rarity}" };
            nameLbl.AddThemeColorOverride("font_color", new Color(_selectedItem.GetRarityHex()));
            nameLbl.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(nameLbl);

            var statsLbl = new Label { Text = $"{def.PrimaryStatLabel}: {_selectedItem.FinalPrimaryValue:0.#}\n{string.Join(", ", _selectedItem.SecondaryBonuses.Select(kv => $"+{kv.Value * 100:0.#}% {kv.Key}"))}\n\"{def.Lore}\"" };
            statsLbl.AddThemeFontSizeOverride("font_size", 12);
            statsLbl.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.85f));
            vbox.AddChild(statsLbl);

            var actionHbox = new HBoxContainer();
            actionHbox.AddThemeConstantOverride("separation", 8);
            vbox.AddChild(actionHbox);

            var equipBtn = new Button { Text = isEquipped ? "ÇIKAR" : "KUŞAN", CustomMinimumSize = new Vector2(110, 34) };
            equipBtn.Connect("pressed", Callable.From(() =>
            {
                if (em == null) return;
                if (isEquipped) em.Unequip(_selectedItem.Slot); else em.Equip(_selectedItem.InstanceId);
                AudioManager.Instance?.PlayButtonClick(); RefreshUI();
            }));
            actionHbox.AddChild(equipBtn);

            double upCost = em != null ? em.GetUpgradeCost(_selectedItem) : 0.0;
            var upBtn = new Button { Text = $"BİLE (+1) [{upCost:0} 🪙]", CustomMinimumSize = new Vector2(160, 34) };
            upBtn.Disabled = _selectedItem.Level >= 10 || (GameManager.Instance?.Gold ?? 0.0) < upCost;
            upBtn.Connect("pressed", Callable.From(() =>
            {
                double g = GameManager.Instance?.Gold ?? 0.0;
                if (em != null && em.UpgradeItem(_selectedItem.InstanceId, ref g))
                {
                    GameManager.Instance?.SpendGold(upCost);
                    AudioManager.Instance?.PlayCoin(); RefreshUI();
                }
            }));
            actionHbox.AddChild(upBtn);

            if (!isEquipped)
            {
                double sellGain = em != null ? em.GetDismantleValue(_selectedItem) : 0.0;
                var sellBtn = new Button { Text = $"SAT [{sellGain:0} 🪙]", CustomMinimumSize = new Vector2(140, 34) };
                sellBtn.Connect("pressed", Callable.From(() =>
                {
                    double g = GameManager.Instance?.Gold ?? 0.0;
                    if (em != null && em.DismantleItem(_selectedItem.InstanceId, ref g))
                    {
                        GameManager.Instance?.AddGold(sellGain);
                        _selectedItem = null; AudioManager.Instance?.PlayCoin(); RefreshUI();
                    }
                }));
                actionHbox.AddChild(sellBtn);
            }
        }
    }
}
