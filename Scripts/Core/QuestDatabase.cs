using System.Collections.Generic;
using System.Linq;

namespace BloodSeal.Core
{
    public static class QuestDatabase
    {
        private static readonly List<QuestDefinition> _milestones = new()
        {
            new QuestDefinition
            {
                Id = "milestone_kills_1",
                Title = "İlk Kan Banyosu",
                Description = "50 düşman katlet.",
                Category = QuestCategory.Milestone,
                Type = QuestType.KillEnemies,
                TargetAmount = 50,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 500), new(QuestRewardType.LoreScrolls, 1) }
            },
            new QuestDefinition
            {
                Id = "milestone_kills_2",
                Title = "Kıyım Ustası",
                Description = "250 düşman katlet.",
                Category = QuestCategory.Milestone,
                Type = QuestType.KillEnemies,
                TargetAmount = 250,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 2500), new(QuestRewardType.LoreScrolls, 2), new(QuestRewardType.AwakeningPoints, 1) }
            },
            new QuestDefinition
            {
                Id = "milestone_kills_3",
                Title = "Ölümün Sureti",
                Description = "1000 düşman katlet.",
                Category = QuestCategory.Milestone,
                Type = QuestType.KillEnemies,
                TargetAmount = 1000,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 10000), new(QuestRewardType.LoreScrolls, 5), new(QuestRewardType.AwakeningPoints, 3) }
            },
            new QuestDefinition
            {
                Id = "milestone_boss_1",
                Title = "Muhafızın Düşüşü",
                Description = "1 Mahzen Bekçisi veya Bölge Boss'u alt et.",
                Category = QuestCategory.Milestone,
                Type = QuestType.DefeatBosses,
                TargetAmount = 1,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 1000), new(QuestRewardType.LoreScrolls, 2) }
            },
            new QuestDefinition
            {
                Id = "milestone_boss_2",
                Title = "Kadimlerin Avcısı",
                Description = "5 Boss katlet.",
                Category = QuestCategory.Milestone,
                Type = QuestType.DefeatBosses,
                TargetAmount = 5,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 5000), new(QuestRewardType.LoreScrolls, 3), new(QuestRewardType.AwakeningPoints, 2) }
            },
            new QuestDefinition
            {
                Id = "milestone_boss_3",
                Title = "Katedral Fatihi",
                Description = "20 Boss katlet.",
                Category = QuestCategory.Milestone,
                Type = QuestType.DefeatBosses,
                TargetAmount = 20,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 25000), new(QuestRewardType.LoreScrolls, 5), new(QuestRewardType.AwakeningPoints, 5) }
            },
            new QuestDefinition
            {
                Id = "milestone_wave_10",
                Title = "Harabeleri Aşmak",
                Description = "Dalga 10'a ulaş.",
                Category = QuestCategory.Milestone,
                Type = QuestType.ReachWave,
                TargetAmount = 10,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 1000), new(QuestRewardType.LoreScrolls, 1) }
            },
            new QuestDefinition
            {
                Id = "milestone_wave_25",
                Title = "Karanlığın Derinlikleri",
                Description = "Dalga 25'e ulaş.",
                Category = QuestCategory.Milestone,
                Type = QuestType.ReachWave,
                TargetAmount = 25,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 5000), new(QuestRewardType.LoreScrolls, 2), new(QuestRewardType.AwakeningPoints, 2) }
            },
            new QuestDefinition
            {
                Id = "milestone_wave_50",
                Title = "Kan Hükümdarı",
                Description = "Dalga 50'ye ulaş.",
                Category = QuestCategory.Milestone,
                Type = QuestType.ReachWave,
                TargetAmount = 50,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 20000), new(QuestRewardType.LoreScrolls, 5), new(QuestRewardType.AwakeningPoints, 5) }
            },
            new QuestDefinition
            {
                Id = "milestone_berserk_1",
                Title = "Kan Arzusu",
                Description = "Berserk modunu 1 kez etkinleştir.",
                Category = QuestCategory.Milestone,
                Type = QuestType.TriggerBerserk,
                TargetAmount = 1,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 300), new(QuestRewardType.LoreScrolls, 1) }
            },
            new QuestDefinition
            {
                Id = "milestone_berserk_10",
                Title = "Gözü Dönmüş Savaşçı",
                Description = "Berserk modunu 10 kez etkinleştir.",
                Category = QuestCategory.Milestone,
                Type = QuestType.TriggerBerserk,
                TargetAmount = 10,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 3000), new(QuestRewardType.LoreScrolls, 2), new(QuestRewardType.AwakeningPoints, 1) }
            },
            new QuestDefinition
            {
                Id = "milestone_upgrade_10",
                Title = "Karanlık Güçlenme",
                Description = "Herhangi bir niteliği 10 kez geliştir.",
                Category = QuestCategory.Milestone,
                Type = QuestType.UpgradeStats,
                TargetAmount = 10,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 500), new(QuestRewardType.LoreScrolls, 1) }
            },
            new QuestDefinition
            {
                Id = "milestone_upgrade_50",
                Title = "Kusursuz Beden",
                Description = "Toplam 50 nitelik yükseltmesi yap.",
                Category = QuestCategory.Milestone,
                Type = QuestType.UpgradeStats,
                TargetAmount = 50,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 3000), new(QuestRewardType.LoreScrolls, 2), new(QuestRewardType.AwakeningPoints, 1) }
            }
        };

        private static readonly List<QuestDefinition> _dailyPool = new()
        {
            new QuestDefinition
            {
                Id = "daily_kills_30",
                Title = "Günlük Av: 30 Kelle",
                Description = "30 düşman öldür.",
                Category = QuestCategory.Daily,
                Type = QuestType.KillEnemies,
                TargetAmount = 30,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 400), new(QuestRewardType.LoreScrolls, 1) }
            },
            new QuestDefinition
            {
                Id = "daily_boss_1",
                Title = "Günlük Av: Boss İnfazı",
                Description = "1 Boss yok et.",
                Category = QuestCategory.Daily,
                Type = QuestType.DefeatBosses,
                TargetAmount = 1,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 600), new(QuestRewardType.LoreScrolls, 1) }
            },
            new QuestDefinition
            {
                Id = "daily_tap_50",
                Title = "Günlük Egzersiz: 50 Darbe",
                Description = "50 kez manuel saldırı yap.",
                Category = QuestCategory.Daily,
                Type = QuestType.TapAttacks,
                TargetAmount = 50,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 300), new(QuestRewardType.LoreScrolls, 1) }
            },
            new QuestDefinition
            {
                Id = "daily_berserk_2",
                Title = "Günlük Hiddet: 2 Berserk",
                Description = "2 kez Berserk aç.",
                Category = QuestCategory.Daily,
                Type = QuestType.TriggerBerserk,
                TargetAmount = 2,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 450), new(QuestRewardType.LoreScrolls, 1) }
            },
            new QuestDefinition
            {
                Id = "daily_upgrade_5",
                Title = "Günlük Gelişim: 5 Yükseltme",
                Description = "5 nitelik yükseltmesi gerçekleştir.",
                Category = QuestCategory.Daily,
                Type = QuestType.UpgradeStats,
                TargetAmount = 5,
                Rewards = new List<QuestReward> { new(QuestRewardType.Gold, 350), new(QuestRewardType.LoreScrolls, 1) }
            }
        };

        public static IReadOnlyList<QuestDefinition> GetMilestones() => _milestones;
        public static IReadOnlyList<QuestDefinition> GetDailyQuestPool() => _dailyPool;

        public static QuestDefinition? GetQuestById(string id)
        {
            return _milestones.FirstOrDefault(q => q.Id == id) ?? _dailyPool.FirstOrDefault(q => q.Id == id);
        }
    }
}
