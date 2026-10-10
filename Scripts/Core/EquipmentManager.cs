#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodSeal.Core
{
    public class EquipmentManager
    {
        private static EquipmentManager? _instance;
        public static EquipmentManager Instance => _instance ??= new EquipmentManager();

        public const int MaxBagCapacity = 24;

        public Dictionary<EquipmentSlot, EquipmentItem?> EquippedItems { get; } = new()
        {
            [EquipmentSlot.Weapon] = null,
            [EquipmentSlot.Armor] = null,
            [EquipmentSlot.Amulet] = null,
            [EquipmentSlot.Ring] = null
        };

        public List<EquipmentItem> BagItems { get; } = new();

        public event Action? OnEquipmentChanged;
        public event Action<EquipmentItem>? OnItemAcquired;

        public EquipmentManager()
        {
            ResetToDefaults();
        }

        public static void SetInstanceForTesting(EquipmentManager instance) => _instance = instance;

        public void ResetToDefaults()
        {
            EquippedItems[EquipmentSlot.Weapon] = null;
            EquippedItems[EquipmentSlot.Armor] = null;
            EquippedItems[EquipmentSlot.Amulet] = null;
            EquippedItems[EquipmentSlot.Ring] = null;
            BagItems.Clear();
        }

        public bool Equip(string instanceId)
        {
            var item = BagItems.FirstOrDefault(i => i.InstanceId == instanceId);
            if (item == null) return false;

            BagItems.Remove(item);
            var currentEquipped = EquippedItems[item.Slot];
            if (currentEquipped != null)
            {
                BagItems.Add(currentEquipped);
            }
            EquippedItems[item.Slot] = item;
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public bool Unequip(EquipmentSlot slot)
        {
            var item = EquippedItems[slot];
            if (item == null) return false;
            if (BagItems.Count >= MaxBagCapacity) return false;

            EquippedItems[slot] = null;
            BagItems.Add(item);
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public double GetUpgradeCost(EquipmentItem item)
        {
            var def = EquipmentDatabase.Get(item.DefinitionId);
            double rarityMult = GetRarityMultiplier(item.Rarity);
            return Math.Floor(def.BaseUpgradeCost * Math.Pow(1.15, item.Level) * rarityMult);
        }

        public double GetDismantleValue(EquipmentItem item)
        {
            double cost = GetUpgradeCost(item);
            return Math.Max(50.0, Math.Floor(cost * (1.0 + 0.5 * item.Level) / 2.0));
        }

        public bool UpgradeItem(string instanceId, ref double currentGold)
        {
            var item = BagItems.FirstOrDefault(i => i.InstanceId == instanceId)
                       ?? EquippedItems.Values.FirstOrDefault(i => i != null && i.InstanceId == instanceId);
            if (item == null || item.Level >= 10) return false;

            double cost = GetUpgradeCost(item);
            if (currentGold < cost) return false;

            currentGold -= cost;
            item.Level++;
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public bool DismantleItem(string instanceId, ref double currentGold)
        {
            var item = BagItems.FirstOrDefault(i => i.InstanceId == instanceId);
            if (item == null) return false;

            double goldGain = GetDismantleValue(item);
            currentGold += goldGain;
            BagItems.Remove(item);
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public EquipmentItem? RollDrop(int wave, bool isBoss, Random? rng = null)
        {
            rng ??= new Random();
            if (!isBoss && rng.NextDouble() > 0.35) return null;

            var rarity = RollRarity(wave, rng);
            var allDefs = EquipmentDatabase.GetAll().ToList();
            var def = allDefs[rng.Next(allDefs.Count)];

            float multiplier = (float)GetRarityMultiplier(rarity);
            var newItem = new EquipmentItem
            {
                DefinitionId = def.Id,
                Slot = def.Slot,
                Rarity = rarity,
                Level = 0,
                BaseValue = def.BasePrimaryValue * multiplier
            };

            int secondaryCount = rarity switch
            {
                EquipmentRarity.Rare => 1,
                EquipmentRarity.Epic => 2,
                EquipmentRarity.Legendary => 2,
                EquipmentRarity.AncientBlood => 3,
                _ => 0
            };

            for (int i = 0; i < secondaryCount; i++)
            {
                string key = (i % 3) switch { 0 => "Crit", 1 => "Speed", _ => "Gold" };
                newItem.SecondaryBonuses[key] = (float)Math.Round(0.03f * (i + 1) * multiplier, 3);
            }

            if (BagItems.Count < MaxBagCapacity)
            {
                BagItems.Add(newItem);
                OnItemAcquired?.Invoke(newItem);
            }
            return newItem;
        }

        public static EquipmentRarity RollRarity(int wave, Random rng)
        {
            double roll = rng.NextDouble() * 100.0;
            if (wave < 10) return roll < 85.0 ? EquipmentRarity.Common : EquipmentRarity.Rare;
            if (wave < 25) return roll < 50.0 ? EquipmentRarity.Common : roll < 90.0 ? EquipmentRarity.Rare : EquipmentRarity.Epic;
            if (wave < 50) return roll < 20.0 ? EquipmentRarity.Common : roll < 65.0 ? EquipmentRarity.Rare : roll < 90.0 ? EquipmentRarity.Epic : EquipmentRarity.Legendary;
            return roll < 10.0 ? EquipmentRarity.Common : roll < 40.0 ? EquipmentRarity.Rare : roll < 75.0 ? EquipmentRarity.Epic : roll < 95.0 ? EquipmentRarity.Legendary : EquipmentRarity.AncientBlood;
        }

        public static double GetRarityMultiplier(EquipmentRarity rarity) => rarity switch
        {
            EquipmentRarity.Rare => 1.35,
            EquipmentRarity.Epic => 1.80,
            EquipmentRarity.Legendary => 2.50,
            EquipmentRarity.AncientBlood => 3.50,
            _ => 1.0
        };

        public float GetTotalPrimaryBonus(EquipmentSlot slot)
        {
            var item = EquippedItems[slot];
            return item?.FinalPrimaryValue ?? 0f;
        }

        public float GetTotalSecondaryBonus(string key)
        {
            float total = 0f;
            foreach (var item in EquippedItems.Values)
            {
                if (item != null && item.SecondaryBonuses.TryGetValue(key, out float val))
                    total += val;
            }
            return total;
        }

        public void LoadState(Dictionary<EquipmentSlot, EquipmentItem?>? equipped, List<EquipmentItem>? bag)
        {
            ResetToDefaults();
            if (equipped != null)
            {
                foreach (var pair in equipped) EquippedItems[pair.Key] = pair.Value;
            }
            if (bag != null)
            {
                BagItems.AddRange(bag.Take(MaxBagCapacity));
            }
            OnEquipmentChanged?.Invoke();
        }
    }
}
