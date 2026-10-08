using System;
using System.Linq;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class RelicSystemTests
    {
        [Fact]
        public void RelicDatabase_Contains_All_Ten_Relics()
        {
            var relics = RelicDatabase.AllRelics;
            Assert.Equal(10, relics.Count);

            for (int wave = 10; wave <= 100; wave += 10)
            {
                var relic = RelicDatabase.GetRelicForWave(wave);
                Assert.NotNull(relic);
                Assert.Equal(wave, relic.MilestoneWave);
                Assert.False(string.IsNullOrWhiteSpace(relic.Name));
                Assert.False(string.IsNullOrWhiteSpace(relic.LoreText));
            }
        }

        [Theory]
        [InlineData(10, "Relic_DariusRing", RelicStatType.Damage, 0.05f)]
        [InlineData(20, "Relic_TornPortrait", RelicStatType.MaxHp, 0.05f)]
        [InlineData(30, "Relic_CovenantMedallion", RelicStatType.Gold, 0.05f)]
        [InlineData(70, "Relic_BoneChalice", RelicStatType.Lifesteal, 0.5f)]
        [InlineData(100, "Relic_FirstScroll", RelicStatType.AllStats, 0.15f)]
        public void RelicDatabase_Maps_Wave_And_Bonuses_Accurately(int wave, string expectedId, RelicStatType expectedType, float expectedVal)
        {
            var relic = RelicDatabase.GetRelicForWave(wave);
            Assert.NotNull(relic);
            Assert.Equal(expectedId, relic.Id);
            Assert.Equal(expectedType, relic.StatType);
            Assert.Equal(expectedVal, relic.BonusValue, 0.001f);
        }

        [Fact]
        public void RelicManager_Unlocks_Relic_And_Calculates_Cumulative_Bonuses()
        {
            var rm = new RelicManager();
            Assert.False(rm.HasRelic("Relic_DariusRing"));
            Assert.Equal(0, rm.GetCollectedCount());
            Assert.Equal(0f, rm.GetDamageBonus());

            bool unlocked = rm.UnlockRelicForWave(10);
            Assert.True(unlocked);
            Assert.True(rm.HasRelic("Relic_DariusRing"));
            Assert.Equal(1, rm.GetCollectedCount());
            Assert.Equal(0.05f, rm.GetDamageBonus(), 0.001f);

            // Duplicate unlock should be no-op
            Assert.False(rm.UnlockRelicForWave(10));
            Assert.Equal(1, rm.GetCollectedCount());

            // Wave 100 provides AllStats (+15%) which stacks with Damage (+5%)
            rm.UnlockRelicForWave(100);
            Assert.Equal(0.20f, rm.GetDamageBonus(), 0.001f);
            Assert.Equal(0.15f, rm.GetHpBonus(), 0.001f);
        }
    }
}
