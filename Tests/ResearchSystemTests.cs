using System;
using System.Linq;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class ResearchSystemTests
    {
        [Fact]
        public void ResearchDatabase_ContainsNineDefinedNodes_AcrossThreeDisciplines()
        {
            var nodes = ResearchDatabase.GetAllNodes();
            Assert.Equal(9, nodes.Count);
            Assert.Equal(3, nodes.Count(n => n.Discipline == ResearchDiscipline.Economy));
            Assert.Equal(3, nodes.Count(n => n.Discipline == ResearchDiscipline.BloodMemory));
            Assert.Equal(3, nodes.Count(n => n.Discipline == ResearchDiscipline.CombatEsotericism));
        }

        [Fact]
        public void ResearchNode_CalculatesCostCorrectly()
        {
            var node = ResearchDatabase.GetNode("Econ_GoldBounty");
            Assert.NotNull(node);
            double costLv1 = node.GetGoldCost(1);
            double costLv2 = node.GetGoldCost(2);
            Assert.Equal(node.BaseGoldCost, costLv1);
            Assert.True(costLv2 > costLv1);
            Assert.Equal(0, node.GetScrollCost(1));
        }

        [Fact]
        public void ResearchManager_TryUpgrade_ConsumesGoldAndScrollsCorrectly()
        {
            var manager = new ResearchManager();
            manager.AddLoreScrolls(2);
            double gold = 1000.0;

            Assert.Equal(0, manager.GetResearchLevel("Econ_SealEfficiency"));
            bool success = manager.TryUpgradeResearch("Econ_SealEfficiency", ref gold);
            
            Assert.True(success);
            Assert.Equal(1, manager.GetResearchLevel("Econ_SealEfficiency"));
            Assert.Equal(1, manager.LoreScrolls); // Consumed 1 scroll
            Assert.True(gold < 1000.0);
            Assert.Equal(0.98f, manager.GetSealCostDiscountMultiplier(), 0.001f); // -2% discount -> 0.98x
        }
    }
}
