using System.Collections.Generic;
using System.Linq;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class QuestSystemTests
    {
        [Fact]
        public void QuestDatabase_ContainsMilestonesAndDailyPool()
        {
            var milestones = QuestDatabase.GetMilestones();
            var dailies = QuestDatabase.GetDailyQuestPool();

            Assert.NotEmpty(milestones);
            Assert.NotEmpty(dailies);
            Assert.True(milestones.Count >= 10, "Expected at least 10 milestones defined.");
            Assert.True(dailies.Count >= 5, "Expected at least 5 daily bounties in the pool.");
        }

        [Fact]
        public void QuestDatabase_GetQuestById_ReturnsValidDefinition()
        {
            var quest = QuestDatabase.GetQuestById("milestone_kills_1");
            Assert.NotNull(quest);
            Assert.Equal(QuestCategory.Milestone, quest.Category);
            Assert.Equal(QuestType.KillEnemies, quest.Type);
            Assert.True(quest.TargetAmount > 0);
            Assert.NotEmpty(quest.Rewards);
        }

        [Fact]
        public void QuestProgress_IsCompleted_CalculatesAccurately()
        {
            var progress = new QuestProgress { QuestId = "test_q", CurrentAmount = 49, IsClaimed = false };
            Assert.False(progress.IsCompleted(50));

            progress.CurrentAmount = 50;
            Assert.True(progress.IsCompleted(50));

            progress.CurrentAmount = 100;
            Assert.True(progress.IsCompleted(50));
        }
    }
}
