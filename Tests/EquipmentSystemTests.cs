using System;
using System.Linq;
using BloodSeal.Core;
using Xunit;

namespace BloodSeal.Tests
{
    public class EquipmentSystemTests
    {
        [Fact]
        public void Database_ContainsAllSixteenGothicItems()
        {
            var weapons = EquipmentDatabase.GetBySlot(EquipmentSlot.Weapon).ToList();
            var armors = EquipmentDatabase.GetBySlot(EquipmentSlot.Armor).ToList();
            var amulets = EquipmentDatabase.GetBySlot(EquipmentSlot.Amulet).ToList();
            var rings = EquipmentDatabase.GetBySlot(EquipmentSlot.Ring).ToList();

            Assert.Equal(4, weapons.Count);
            Assert.Equal(4, armors.Count);
            Assert.Equal(4, amulets.Count);
            Assert.Equal(4, rings.Count);
            Assert.Equal(16, EquipmentDatabase.GetAll().Count());
        }

        [Fact]
        public void RollDrop_GeneratesItemWithRarityAndStats()
        {
            var manager = new EquipmentManager();
            var rng = new Random(42);

            var item = manager.RollDrop(wave: 30, isBoss: true, rng: rng);

            Assert.NotNull(item);
            Assert.Contains(item, manager.BagItems);
            Assert.True(item.FinalPrimaryValue > 0f);
        }

        [Fact]
        public void EquipAndUnequip_SwapsItemsCorrectly()
        {
            var manager = new EquipmentManager();
            var item = new EquipmentItem
            {
                DefinitionId = "weap_blood_rapier",
                Slot = EquipmentSlot.Weapon,
                Rarity = EquipmentRarity.Rare,
                BaseValue = 20f
            };
            manager.BagItems.Add(item);

            bool equipped = manager.Equip(item.InstanceId);
            Assert.True(equipped);
            Assert.Equal(item, manager.EquippedItems[EquipmentSlot.Weapon]);
            Assert.DoesNotContain(item, manager.BagItems);

            bool unequipped = manager.Unequip(EquipmentSlot.Weapon);
            Assert.True(unequipped);
            Assert.Null(manager.EquippedItems[EquipmentSlot.Weapon]);
            Assert.Contains(item, manager.BagItems);
        }

        [Fact]
        public void UpgradeItem_IncreasesLevelAndCalculatesCost()
        {
            var manager = new EquipmentManager();
            var item = new EquipmentItem
            {
                DefinitionId = "weap_blood_rapier",
                Slot = EquipmentSlot.Weapon,
                Rarity = EquipmentRarity.Common,
                Level = 0,
                BaseValue = 15f
            };
            manager.BagItems.Add(item);

            double cost = manager.GetUpgradeCost(item);
            Assert.Equal(150.0, cost);

            double gold = 50.0; // insufficient
            Assert.False(manager.UpgradeItem(item.InstanceId, ref gold));

            gold = 200.0;
            Assert.True(manager.UpgradeItem(item.InstanceId, ref gold));
            Assert.Equal(1, item.Level);
            Assert.Equal(50.0, gold);
            Assert.Equal(15f * 1.10f, item.FinalPrimaryValue);
        }

        [Fact]
        public void DismantleItem_GrantsGoldAndRemovesFromBag()
        {
            var manager = new EquipmentManager();
            var item = new EquipmentItem
            {
                DefinitionId = "weap_blood_rapier",
                Slot = EquipmentSlot.Weapon,
                Rarity = EquipmentRarity.Common,
                Level = 0,
                BaseValue = 15f
            };
            manager.BagItems.Add(item);

            double gold = 0.0;
            double dismantleValue = manager.GetDismantleValue(item);
            Assert.True(dismantleValue >= 50.0);

            bool dismantled = manager.DismantleItem(item.InstanceId, ref gold);
            Assert.True(dismantled);
            Assert.Equal(dismantleValue, gold);
            Assert.DoesNotContain(item, manager.BagItems);
        }

        [Fact]
        public void StatAggregation_ComputesTotalPrimaryAndSecondaryBonuses()
        {
            var manager = new EquipmentManager();
            var weapon = new EquipmentItem
            {
                DefinitionId = "weap_blood_rapier",
                Slot = EquipmentSlot.Weapon,
                BaseValue = 25f,
                Level = 2 // 25 * 1.2 = 30
            };
            weapon.SecondaryBonuses["Crit"] = 0.05f;

            manager.EquippedItems[EquipmentSlot.Weapon] = weapon;

            Assert.Equal(30f, manager.GetTotalPrimaryBonus(EquipmentSlot.Weapon), 2);
            Assert.Equal(0.05f, manager.GetTotalSecondaryBonus("Crit"), 2);
        }
    }
}
