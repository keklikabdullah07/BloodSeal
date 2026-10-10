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

        [Fact]
        public void QuestManager_ProgressesMilestonesOnEvents()
        {
            var manager = new QuestManager();
            manager.ResetAll();

            for (int i = 0; i < 50; i++)
            {
                manager.RecordEnemyKilled(isBoss: false);
            }

            var progress = manager.GetProgress("milestone_kills_1");
            Assert.NotNull(progress);
            Assert.Equal(50, progress.CurrentAmount);
            Assert.True(progress.IsCompleted(50));
            Assert.False(progress.IsClaimed);
            Assert.True(manager.GetUnclaimedRewardCount() >= 1);
        }

        [Fact]
        public void QuestManager_ClaimReward_AppliesRewardsAndMarksClaimed()
        {
            var manager = new QuestManager();
            manager.ResetAll();

            for (int i = 0; i < 50; i++)
            {
                manager.RecordEnemyKilled(isBoss: false);
            }

            var rewardsClaimed = new List<(QuestRewardType Type, double Amount)>();
            bool success = manager.ClaimReward("milestone_kills_1", (type, amt) => rewardsClaimed.Add((type, amt)));

            Assert.True(success);
            Assert.True(manager.GetProgress("milestone_kills_1").IsClaimed);
            Assert.NotEmpty(rewardsClaimed);
            Assert.Contains(rewardsClaimed, r => r.Type == QuestRewardType.Gold && r.Amount == 500);

            // Cannot claim twice
            bool secondClaim = manager.ClaimReward("milestone_kills_1", (t, a) => { });
            Assert.False(secondClaim);
        }

        [Fact]
        public void QuestManager_DailyReset_RefreshesAfter24Hours()
        {
            var manager = new QuestManager();
            manager.ResetAll();

            long t0 = 100000;
            manager.CheckAndRefreshDailyQuests(t0, force: true);
            var firstDailies = manager.ActiveDailyQuestIds.ToList();
            Assert.Equal(3, firstDailies.Count);

            // Progress one daily
            manager.RecordEnemyKilled(isBoss: false);
            var qId = firstDailies.FirstOrDefault(id => QuestDatabase.GetQuestById(id)?.Type == QuestType.KillEnemies);
            if (qId != null)
            {
                Assert.Equal(1, manager.GetProgress(qId).CurrentAmount);
            }

            // Advancing 10 hours should NOT reset
            manager.CheckAndRefreshDailyQuests(t0 + 36000);
            Assert.Equal(firstDailies, manager.ActiveDailyQuestIds);

            // Advancing 25 hours (90000s) SHOULD reset
            manager.CheckAndRefreshDailyQuests(t0 + 90000);
            Assert.Equal(3, manager.ActiveDailyQuestIds.Count);
            Assert.Equal(t0 + 90000, manager.LastDailyResetTimestamp);
        }

        [Fact]
        public void QuestManager_HandlesAllGameEventTypes()
        {
            var qm = new QuestManager();
            qm.ResetAll();

            qm.RecordTapAttack();
            qm.RecordBerserkActivated();
            qm.RecordStatUpgraded();
            qm.RecordWaveProgress(15);
            qm.RecordEnemyKilled(isBoss: true);

            Assert.Equal(1, qm.GetProgress("milestone_berserk_1").CurrentAmount);
            Assert.Equal(1, qm.GetProgress("milestone_upgrade_10").CurrentAmount);
            Assert.Equal(15, qm.GetProgress("milestone_wave_10").CurrentAmount);
            Assert.Equal(1, qm.GetProgress("milestone_boss_1").CurrentAmount);
            Assert.Equal(1, qm.GetProgress("milestone_kills_1").CurrentAmount);
        }
    }
}
