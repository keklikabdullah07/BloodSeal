using System;
using Xunit;
using BloodSeal.Core;
using BloodSeal.Combat;

namespace BloodSeal.Tests
{
    public class ManorGateTests
    {
        [Fact]
        public void ManorGate_UnlockWave_IsWave5()
        {
            Assert.Equal(5, ManorGateHelper.GateUnlockWave);
        }

        [Fact]
        public void ManorGate_RuneApproaches_MapCorrectly()
        {
            Assert.Equal(RuneType.BloodArmor, ManorGateHelper.GetRuneForApproach(GateApproachType.FrontGate));
            Assert.Equal(RuneType.ShadowSpeed, ManorGateHelper.GetRuneForApproach(GateApproachType.Sewers));
            Assert.Equal(RuneType.SoulLeech, ManorGateHelper.GetRuneForApproach(GateApproachType.BloodSeal));
            Assert.Equal(RuneType.WealthGreed, ManorGateHelper.GetRuneForApproach(GateApproachType.RoofInfiltration));
        }

        [Fact]
        public void PentagramStats_WithBloodArmor_AppliesAtkAndHpBonus()
        {
            var baseStats = new PentagramStats { AtkLevel = 1, MaxHpLevel = 1 };
            float baseAtk = baseStats.Atk; // 10.0
            float baseHp = baseStats.MaxHp; // 100.0

            var runeStats = new PentagramStats
            {
                AtkLevel = 1,
                MaxHpLevel = 1,
                ActiveRune = RuneType.BloodArmor
            };

            Assert.Equal(baseAtk * 1.10f, runeStats.Atk, precision: 2);
            Assert.Equal(baseHp * 1.10f, runeStats.MaxHp, precision: 2);
        }

        [Fact]
        public void PentagramStats_WithShadowSpeed_AppliesSpeedAndRangeBonus()
        {
            var baseStats = new PentagramStats { AtkSpeedLevel = 1, RangeLevel = 1 };
            float baseSpeed = baseStats.AtkSpeed; // 1.0
            float baseRange = baseStats.Range;    // 180.0

            var runeStats = new PentagramStats
            {
                AtkSpeedLevel = 1,
                RangeLevel = 1,
                ActiveRune = RuneType.ShadowSpeed
            };

            Assert.Equal(baseSpeed * 1.10f, runeStats.AtkSpeed, precision: 2);
            Assert.Equal(baseRange + 25f, runeStats.Range, precision: 2);
        }

        [Fact]
        public void PentagramStats_WithSoulLeech_AppliesLifestealBonus()
        {
            var baseStats = new PentagramStats { LifestealLevel = 1 };
            float baseLifesteal = baseStats.LifestealPercent; // 1.0%

            var runeStats = new PentagramStats
            {
                LifestealLevel = 1,
                ActiveRune = RuneType.SoulLeech
            };

            Assert.Equal(baseLifesteal + 2.5f, runeStats.LifestealPercent, precision: 2);
        }

        [Fact]
        public void TacticalAdvantage_ChangesAccordingToOrigin()
        {
            string pitDesc = ManorGateHelper.GetApproachTacticalAdvantage(GateApproachType.RoofInfiltration, StreetOriginType.PitFighter);
            string thiefDesc = ManorGateHelper.GetApproachTacticalAdvantage(GateApproachType.RoofInfiltration, StreetOriginType.StreetThief);
            string mercDesc = ManorGateHelper.GetApproachTacticalAdvantage(GateApproachType.RoofInfiltration, StreetOriginType.ExMercenary);

            Assert.Contains("Kafes Dövüşçüsü", pitDesc);
            Assert.Contains("Sokak Hırsızı", thiefDesc);
            Assert.Contains("Eski Paralı Asker", mercDesc);
        }

        [Fact]
        public void SaveData_SerializesAndRestoresManorGateFields()
        {
            var original = new SaveData
            {
                CurrentWave = 5,
                ActiveRune = RuneType.BloodArmor,
                SelectedGateApproach = GateApproachType.FrontGate,
                HasEncounteredGate = true,
                HasClaimedGateReward = true,
                HasFirstLoreScroll = true
            };

            string json = System.Text.Json.JsonSerializer.Serialize(original);
            var restored = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json);

            Assert.NotNull(restored);
            Assert.Equal(RuneType.BloodArmor, restored.ActiveRune);
            Assert.Equal(GateApproachType.FrontGate, restored.SelectedGateApproach);
            Assert.True(restored.HasEncounteredGate);
            Assert.True(restored.HasClaimedGateReward);
            Assert.True(restored.HasFirstLoreScroll);
        }
    }
}
