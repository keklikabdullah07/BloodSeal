using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public enum QuestCategory
    {
        Milestone = 0,
        Daily = 1
    }

    public enum QuestType
    {
        KillEnemies = 0,
        DefeatBosses = 1,
        ReachWave = 2,
        TriggerBerserk = 3,
        UpgradeStats = 4,
        TapAttacks = 5
    }

    public enum QuestRewardType
    {
        Gold = 0,
        LoreScrolls = 1,
        AwakeningPoints = 2
    }

    public class QuestReward
    {
        public QuestRewardType Type { get; set; }
        public double Amount { get; set; }

        public QuestReward() { }
        public QuestReward(QuestRewardType type, double amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    public class QuestDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public QuestCategory Category { get; set; }
        public QuestType Type { get; set; }
        public double TargetAmount { get; set; }
        public List<QuestReward> Rewards { get; set; } = new();
    }

    public class QuestProgress
    {
        public string QuestId { get; set; } = string.Empty;
        public double CurrentAmount { get; set; }
        public bool IsClaimed { get; set; }

        public bool IsCompleted(double target) => CurrentAmount >= target;
    }
}
