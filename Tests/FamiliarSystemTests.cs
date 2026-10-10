using System;
using BloodSeal.Core;
using Xunit;

namespace BloodSeal.Tests
{
    public class FamiliarSystemTests
    {
        [Fact]
        public void Database_ContainsAllFourGothicFamiliars()
        {
            var raven = FamiliarDatabase.Get("blood_raven");
            var bat = FamiliarDatabase.Get("shadow_bat");
            var hound = FamiliarDatabase.Get("crimson_hound");
            var gargoyle = FamiliarDatabase.Get("stone_gargoyle");

            Assert.NotNull(raven);
            Assert.NotNull(bat);
            Assert.NotNull(hound);
            Assert.NotNull(gargoyle);

            Assert.Equal(1, raven.UnlockWaveRequirement);
            Assert.Equal(15, bat.UnlockWaveRequirement);
            Assert.Equal(25, hound.UnlockWaveRequirement);
            Assert.Equal(40, gargoyle.UnlockWaveRequirement);
        }

        [Fact]
        public void UpgradeCost_FollowsExponentialFormula()
        {
            var manager = new FamiliarManager();
            FamiliarManager.SetInstanceForTesting(manager);

            double costLv1 = manager.GetGoldCost("blood_raven");
            Assert.Equal(100.0, costLv1);

            double gold = 100.0;
            int parch = 0;
            bool upgraded = manager.Upgrade("blood_raven", ref gold, ref parch);

            Assert.True(upgraded);
            Assert.Equal(0.0, gold);
            Assert.Equal(2, manager.GetProgress("blood_raven").Level);

            double costLv2 = manager.GetGoldCost("blood_raven");
            Assert.Equal(Math.Floor(100.0 * 1.15), costLv2);
        }

        [Fact]
        public void TierMilestone_RequiresParchment()
        {
            var manager = new FamiliarManager();
            FamiliarManager.SetInstanceForTesting(manager);

            var prog = manager.GetProgress("blood_raven");
            prog.Level = 9; // 9 -> 10 requires Tier 1 milestone parchment (5 parchments)

            Assert.Equal(5, manager.GetParchmentCost("blood_raven"));

            double gold = 10000.0;
            int parch = 2; // insufficient
            Assert.False(manager.CanUpgrade("blood_raven", gold, parch));

            parch = 5; // sufficient
            Assert.True(manager.CanUpgrade("blood_raven", gold, parch));
            Assert.True(manager.Upgrade("blood_raven", ref gold, ref parch));
            Assert.Equal(10, prog.Level);
            Assert.Equal(0, parch);
        }

        [Fact]
        public void WaveProgression_UnlocksFamiliarsCorrectly()
        {
            var manager = new FamiliarManager();
            FamiliarManager.SetInstanceForTesting(manager);

            Assert.True(manager.GetProgress("blood_raven").IsUnlocked);
            Assert.False(manager.GetProgress("shadow_bat").IsUnlocked);

            manager.CheckWaveUnlocks(14);
            Assert.False(manager.GetProgress("shadow_bat").IsUnlocked);

            manager.CheckWaveUnlocks(15);
            Assert.True(manager.GetProgress("shadow_bat").IsUnlocked);

            manager.CheckWaveUnlocks(40);
            Assert.True(manager.GetProgress("stone_gargoyle").IsUnlocked);
        }

        [Fact]
        public void ActiveFamiliarBuffs_ApplyExpectedMultipliers()
        {
            var manager = new FamiliarManager();
            FamiliarManager.SetInstanceForTesting(manager);

            // Default raven
            Assert.Equal(0f, manager.GetCritChanceBonus());
            Assert.Equal(0f, manager.GetDamageReductionBonus());

            // Unlock and switch to bat
            manager.CheckWaveUnlocks(15);
            Assert.True(manager.SetActiveFamiliar("shadow_bat"));
            Assert.True(manager.GetCritChanceBonus() >= 0.05f);

            // Unlock and switch to gargoyle
            manager.CheckWaveUnlocks(40);
            Assert.True(manager.SetActiveFamiliar("stone_gargoyle"));
            Assert.True(manager.GetDamageReductionBonus() >= 0.10f);
        }
    }
}
