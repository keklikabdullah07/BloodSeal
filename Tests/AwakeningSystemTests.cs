using System;
using System.Linq;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class AwakeningSystemTests
    {
        [Fact]
        public void AwakeningDatabase_Contains_All_Seven_Nodes()
        {
            var nodes = AwakeningDatabase.AllNodes;
            Assert.Equal(7, nodes.Count);

            var warNodes = AwakeningDatabase.GetNodesForBranch(AwakeningBranch.War);
            var momentumNodes = AwakeningDatabase.GetNodesForBranch(AwakeningBranch.Momentum);
            var heritageNodes = AwakeningDatabase.GetNodesForBranch(AwakeningBranch.Heritage);

            Assert.Equal(3, warNodes.Count);
            Assert.Equal(2, momentumNodes.Count);
            Assert.Equal(2, heritageNodes.Count);
        }

        [Theory]
        [InlineData("War_PrimordialMight", 1, 1)]
        [InlineData("War_PrimordialMight", 5, 5)]
        [InlineData("Flow_WaveLeap", 1, 2)]
        [InlineData("Flow_WaveLeap", 6, 12)]
        [InlineData("Heritage_BloodRecall", 1, 1)]
        [InlineData("Heritage_BloodRecall", 5, 8)]
        public void AwakeningNodes_Calculate_Correct_Costs(string id, int level, int expectedCost)
        {
            var node = AwakeningDatabase.GetNode(id);
            Assert.NotNull(node);
            Assert.Equal(expectedCost, node.GetCost(level));
        }

        [Theory]
        [InlineData(19, 0)]
        [InlineData(20, 1)]
        [InlineData(25, 2)]
        [InlineData(30, 3)]
        [InlineData(40, 6)]
        [InlineData(50, 9)]
        [InlineData(75, 17)]
        [InlineData(100, 25)]
        public void CalculatePendingPoints_Matches_Formula_Table(int wave, int expectedPoints)
        {
            var manager = new AwakeningManager();
            Assert.Equal(expectedPoints, manager.CalculatePendingPoints(wave));
        }

        [Fact]
        public void Upgrading_Seal_Deducts_AP_And_Provides_Multipliers()
        {
            var manager = new AwakeningManager();
            manager.AddAwakeningPoints(10);
            Assert.Equal(10, manager.AwakeningPoints);

            Assert.True(manager.CanUpgradeSeal("War_PrimordialMight"));
            Assert.True(manager.TryUpgradeSeal("War_PrimordialMight")); // costs 1 AP
            Assert.Equal(9, manager.AwakeningPoints);
            Assert.Equal(1, manager.GetSealLevel("War_PrimordialMight"));
            Assert.Equal(1.15f, manager.GetDamageMultiplier(), 0.001f);

            Assert.True(manager.TryUpgradeSeal("War_PrimordialMight")); // costs 2 AP
            Assert.Equal(7, manager.AwakeningPoints);
            Assert.Equal(2, manager.GetSealLevel("War_PrimordialMight"));
            Assert.Equal(1.30f, manager.GetDamageMultiplier(), 0.001f);
        }
    }
}
