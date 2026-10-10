#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Amulet,
        Ring
    }

    public enum EquipmentRarity
    {
        Common,
        Rare,
        Epic,
        Legendary,
        AncientBlood
    }

    public class EquipmentDefinition
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public EquipmentSlot Slot { get; set; }
        public string IconSymbol { get; set; } = "⚔️";
        public string Lore { get; set; } = "";
        public float BasePrimaryValue { get; set; }
        public string PrimaryStatLabel { get; set; } = "ATK";
        public double BaseUpgradeCost { get; set; } = 150.0;
    }

    public class EquipmentItem
    {
        public string InstanceId { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string DefinitionId { get; set; } = "";
        public EquipmentSlot Slot { get; set; }
        public EquipmentRarity Rarity { get; set; } = EquipmentRarity.Common;
        public int Level { get; set; } = 0; // +0 .. +10
        public float BaseValue { get; set; }
        public Dictionary<string, float> SecondaryBonuses { get; set; } = new();

        public float FinalPrimaryValue => BaseValue * (1.0f + 0.10f * Level);

        public string GetRarityHex() => Rarity switch
        {
            EquipmentRarity.Common => "#B0B0B0",
            EquipmentRarity.Rare => "#3A86FF",
            EquipmentRarity.Epic => "#9D4EDD",
            EquipmentRarity.Legendary => "#FFB703",
            EquipmentRarity.AncientBlood => "#D00000",
            _ => "#FFFFFF"
        };
    }
}
