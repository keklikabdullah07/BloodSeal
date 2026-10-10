# 🩸 BloodSeal: Başarımlar & Günlük Kan Görevleri (Quest & Achievement System) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a fully decoupled Gothic Quests & Achievements system comprising permanent progressive Milestones and 24-hour rotating Daily Bounties, complete with event-driven progress tracking, secure persistence, rewarding mechanics (Gold, Lore Scrolls, Awakening Points), and a rich dual-tab modal UI.

**Architecture:** A pure C# domain manager (`QuestManager`) with model definitions (`QuestModels`) and data registry (`QuestDatabase`) handles progression without Godot node overhead, enabling 100% unit testability in xUnit. Game events (`EnemyDefeated`, `WaveChanged`, `TapCombat`, `BerserkActive`, `StatsUpgraded`) drive quest progress. Save data is serialized via `SaveSystem`. A decoupled UI controller (`QuestController`) and Gothic modal (`QuestModal`) integrate into the HUD without inflating `MainHUD.cs` beyond the binding 250-line limit.

**Tech Stack:** C# 12 / .NET 10.0, Godot 4.7 Mono, xUnit test suite.

## Global Constraints
- Maximum 250 lines per C# source file (Binding Rule).
- `Engine.TimeScale` must NEVER be modified.
- All numbers use `double` or `float` with overflow protection.
- "Call Down, Signal Up" decoupling: `MainHUD.cs` and `GameManager.cs` must NOT be bloated or directly coupled; both must remain <= 250 lines.
- No UI interactions blocking combat unless explicitly in a modal (`MouseFilter` handled correctly).
- Full unit test coverage in `BloodSeal.Tests` (`dotnet test` must pass with 0 warnings, 0 errors).

---

### Task 1: Quest Data Models and Database Registry

**Files:**
- Create: `Scripts/Core/QuestModels.cs`
- Create: `Scripts/Core/QuestDatabase.cs`
- Test: `Tests/QuestSystemTests.cs`

**Interfaces:**
- Consumes: None
- Produces:
  - `enum QuestCategory { Milestone = 0, Daily = 1 }`
  - `enum QuestType { KillEnemies = 0, DefeatBosses = 1, ReachWave = 2, TriggerBerserk = 3, UpgradeStats = 4, TapAttacks = 5 }`
  - `enum QuestRewardType { Gold = 0, LoreScrolls = 1, AwakeningPoints = 2 }`
  - `class QuestReward { public QuestRewardType Type; public double Amount; }`
  - `class QuestDefinition { public string Id; public string Title; public string Description; public QuestCategory Category; public QuestType Type; public double TargetAmount; public List<QuestReward> Rewards; }`
  - `class QuestProgress { public string QuestId; public double CurrentAmount; public bool IsClaimed; public bool IsCompleted(double target); }`
  - `class QuestDatabase`:
    - `public static IReadOnlyList<QuestDefinition> GetMilestones()`
    - `public static IReadOnlyList<QuestDefinition> GetDailyQuestPool()`
    - `public static QuestDefinition GetQuestById(string id)`

- [ ] **Step 1: Write the failing tests for QuestModels and QuestDatabase**

Create `Tests/QuestSystemTests.cs`:
```csharp
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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~QuestSystemTests"`
Expected: FAIL with compilation error (QuestDatabase and QuestModels do not exist).

- [ ] **Step 3: Write minimal implementation**

Create `Scripts/Core/QuestModels.cs`:
```csharp
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
```

Create `Scripts/Core/QuestDatabase.cs`:
```csharp
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

        public static QuestDefinition GetQuestById(string id)
        {
            return _milestones.FirstOrDefault(q => q.Id == id) ?? _dailyPool.FirstOrDefault(q => q.Id == id);
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~QuestSystemTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/QuestModels.cs Scripts/Core/QuestDatabase.cs Tests/QuestSystemTests.cs
git commit -m "feat(quests): create QuestModels, QuestDatabase, and initial unit tests"
```

---

### Task 2: Core QuestManager Implementation (Pure C# Domain Service)

**Files:**
- Create: `Scripts/Core/QuestManager.cs`
- Modify: `Tests/QuestSystemTests.cs`

**Interfaces:**
- Consumes: `QuestModels`, `QuestDatabase`
- Produces:
  - `class QuestManager`:
    - `public static QuestManager Instance { get; }`
    - `public IReadOnlyDictionary<string, QuestProgress> AllProgress { get; }`
    - `public IReadOnlyList<string> ActiveDailyQuestIds { get; }`
    - `public long LastDailyResetTimestamp { get; }`
    - `public event Action<QuestProgress> OnQuestProgressUpdated;`
    - `public event Action<QuestDefinition> OnQuestCompleted;`
    - `public event Action<QuestDefinition, QuestReward> OnQuestRewardClaimed;`
    - `public event Action OnDailyQuestsRefreshed;`
    - `public void RecordEnemyKilled(bool isBoss);`
    - `public void RecordWaveProgress(int currentWave);`
    - `public void RecordTapAttack();`
    - `public void RecordBerserkActivated();`
    - `public void RecordStatUpgraded();`
    - `public bool ClaimReward(string questId, Action<QuestRewardType, double> rewardApplier);`
    - `public int GetUnclaimedRewardCount();`
    - `public void CheckAndRefreshDailyQuests(long currentUnixTimestamp, bool force = false);`
    - `public void LoadState(Dictionary<string, QuestProgress> savedProgress, List<string> dailyIds, long lastReset);`
    - `public void ResetAll();`

- [ ] **Step 1: Write failing tests for QuestManager logic**

Append to `Tests/QuestSystemTests.cs`:
```csharp
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
            var qId = firstDailies.FirstOrDefault(id => QuestDatabase.GetQuestById(id).Type == QuestType.KillEnemies);
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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~QuestSystemTests"`
Expected: FAIL with compilation error (`QuestManager` does not exist).

- [ ] **Step 3: Implement QuestManager**

Create `Scripts/Core/QuestManager.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodSeal.Core
{
    public class QuestManager
    {
        private static QuestManager _instance;
        public static QuestManager Instance => _instance ??= new QuestManager();

        private readonly Dictionary<string, QuestProgress> _progressMap = new();
        private readonly List<string> _activeDailyQuestIds = new();

        public IReadOnlyDictionary<string, QuestProgress> AllProgress => _progressMap;
        public IReadOnlyList<string> ActiveDailyQuestIds => _activeDailyQuestIds;
        public long LastDailyResetTimestamp { get; private set; }

        public event Action<QuestProgress> OnQuestProgressUpdated;
        public event Action<QuestDefinition> OnQuestCompleted;
        public event Action<QuestDefinition, QuestReward> OnQuestRewardClaimed;
        public event Action OnDailyQuestsRefreshed;

        public QuestManager()
        {
            InitializeMilestones();
        }

        public void ResetAll()
        {
            _progressMap.Clear();
            _activeDailyQuestIds.Clear();
            LastDailyResetTimestamp = 0;
            InitializeMilestones();
        }

        private void InitializeMilestones()
        {
            foreach (var m in QuestDatabase.GetMilestones())
            {
                if (!_progressMap.ContainsKey(m.Id))
                {
                    _progressMap[m.Id] = new QuestProgress { QuestId = m.Id, CurrentAmount = 0, IsClaimed = false };
                }
            }
        }

        public QuestProgress GetProgress(string questId)
        {
            if (_progressMap.TryGetValue(questId, out var p)) return p;
            var created = new QuestProgress { QuestId = questId, CurrentAmount = 0, IsClaimed = false };
            _progressMap[questId] = created;
            return created;
        }

        public void RecordEnemyKilled(bool isBoss)
        {
            IncrementQuests(QuestType.KillEnemies, 1);
            if (isBoss) IncrementQuests(QuestType.DefeatBosses, 1);
        }

        public void RecordWaveProgress(int currentWave)
        {
            SetQuestsMax(QuestType.ReachWave, currentWave);
        }

        public void RecordTapAttack()
        {
            IncrementQuests(QuestType.TapAttacks, 1);
        }

        public void RecordBerserkActivated()
        {
            IncrementQuests(QuestType.TriggerBerserk, 1);
        }

        public void RecordStatUpgraded()
        {
            IncrementQuests(QuestType.UpgradeStats, 1);
        }

        private void IncrementQuests(QuestType type, double amount)
        {
            var activeQuests = GetRelevantActiveQuests(type);
            foreach (var def in activeQuests)
            {
                var progress = GetProgress(def.Id);
                if (progress.IsClaimed) continue;

                bool wasCompleted = progress.IsCompleted(def.TargetAmount);
                progress.CurrentAmount += amount;
                OnQuestProgressUpdated?.Invoke(progress);

                if (!wasCompleted && progress.IsCompleted(def.TargetAmount))
                {
                    OnQuestCompleted?.Invoke(def);
                }
            }
        }

        private void SetQuestsMax(QuestType type, double amount)
        {
            var activeQuests = GetRelevantActiveQuests(type);
            foreach (var def in activeQuests)
            {
                var progress = GetProgress(def.Id);
                if (progress.IsClaimed) continue;

                bool wasCompleted = progress.IsCompleted(def.TargetAmount);
                if (amount > progress.CurrentAmount)
                {
                    progress.CurrentAmount = amount;
                    OnQuestProgressUpdated?.Invoke(progress);

                    if (!wasCompleted && progress.IsCompleted(def.TargetAmount))
                    {
                        OnQuestCompleted?.Invoke(def);
                    }
                }
            }
        }

        private List<QuestDefinition> GetRelevantActiveQuests(QuestType type)
        {
            var list = new List<QuestDefinition>();
            foreach (var m in QuestDatabase.GetMilestones())
            {
                if (m.Type == type) list.Add(m);
            }
            foreach (var id in _activeDailyQuestIds)
            {
                var d = QuestDatabase.GetQuestById(id);
                if (d != null && d.Type == type) list.Add(d);
            }
            return list;
        }

        public bool ClaimReward(string questId, Action<QuestRewardType, double> rewardApplier)
        {
            var def = QuestDatabase.GetQuestById(questId);
            if (def == null) return false;

            var progress = GetProgress(questId);
            if (progress.IsClaimed || !progress.IsCompleted(def.TargetAmount)) return false;

            progress.IsClaimed = true;
            foreach (var reward in def.Rewards)
            {
                rewardApplier?.Invoke(reward.Type, reward.Amount);
                OnQuestRewardClaimed?.Invoke(def, reward);
            }
            OnQuestProgressUpdated?.Invoke(progress);
            return true;
        }

        public int GetUnclaimedRewardCount()
        {
            int count = 0;
            foreach (var m in QuestDatabase.GetMilestones())
            {
                var p = GetProgress(m.Id);
                if (!p.IsClaimed && p.IsCompleted(m.TargetAmount)) count++;
            }
            foreach (var id in _activeDailyQuestIds)
            {
                var d = QuestDatabase.GetQuestById(id);
                if (d == null) continue;
                var p = GetProgress(id);
                if (!p.IsClaimed && p.IsCompleted(d.TargetAmount)) count++;
            }
            return count;
        }

        public void CheckAndRefreshDailyQuests(long currentUnixTimestamp, bool force = false)
        {
            const long DAY_SECONDS = 86400;
            if (!force && _activeDailyQuestIds.Count == 3 && (currentUnixTimestamp - LastDailyResetTimestamp) < DAY_SECONDS)
            {
                return;
            }

            var pool = QuestDatabase.GetDailyQuestPool().ToList();
            var random = new Random((int)(currentUnixTimestamp / DAY_SECONDS));
            var selected = pool.OrderBy(_ => random.Next()).Take(3).ToList();

            _activeDailyQuestIds.Clear();
            foreach (var q in selected)
            {
                _activeDailyQuestIds.Add(q.Id);
                _progressMap[q.Id] = new QuestProgress { QuestId = q.Id, CurrentAmount = 0, IsClaimed = false };
            }

            LastDailyResetTimestamp = currentUnixTimestamp;
            OnDailyQuestsRefreshed?.Invoke();
        }

        public void LoadState(Dictionary<string, QuestProgress> savedProgress, List<string> dailyIds, long lastReset)
        {
            if (savedProgress != null)
            {
                foreach (var kvp in savedProgress)
                {
                    _progressMap[kvp.Key] = kvp.Value;
                }
            }
            if (dailyIds != null && dailyIds.Count > 0)
            {
                _activeDailyQuestIds.Clear();
                _activeDailyQuestIds.AddRange(dailyIds);
            }
            LastDailyResetTimestamp = lastReset;
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~QuestSystemTests"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/QuestManager.cs Tests/QuestSystemTests.cs
git commit -m "feat(quests): implement QuestManager with event updates, claims, and daily rotation"
```

---

### Task 3: SaveData and SaveSystem Integration

**Files:**
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Core/SaveSystem.cs`
- Test: `Tests/SaveSystemTests.cs`

**Interfaces:**
- Consumes: `QuestManager`, `QuestProgress`
- Produces:
  - `SaveData.QuestProgresses`
  - `SaveData.ActiveDailyQuestIds`
  - `SaveData.LastDailyResetTimestamp`

- [ ] **Step 1: Write failing test in SaveSystemTests**

Add to `Tests/SaveSystemTests.cs`:
```csharp
        [Fact]
        public void SaveSystem_SerializesAndRestoresQuestProgress()
        {
            var data = new SaveData();
            data.QuestProgresses["milestone_kills_1"] = new QuestProgress { QuestId = "milestone_kills_1", CurrentAmount = 45, IsClaimed = false };
            data.ActiveDailyQuestIds.Add("daily_kills_30");
            data.LastDailyResetTimestamp = 123456789;

            string json = System.Text.Json.JsonSerializer.Serialize(data);
            var restored = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json);

            Assert.NotNull(restored);
            Assert.True(restored.QuestProgresses.ContainsKey("milestone_kills_1"));
            Assert.Equal(45, restored.QuestProgresses["milestone_kills_1"].CurrentAmount);
            Assert.Single(restored.ActiveDailyQuestIds);
            Assert.Equal(123456789, restored.LastDailyResetTimestamp);
        }
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~SaveSystem_SerializesAndRestoresQuestProgress"`
Expected: FAIL (fields missing in SaveData).

- [ ] **Step 3: Update SaveData and SaveSystem**

In `Scripts/Core/SaveData.cs`, add:
```csharp
public Dictionary<string, QuestProgress> QuestProgresses { get; set; } = new();
public List<string> ActiveDailyQuestIds { get; set; } = new();
public long LastDailyResetTimestamp { get; set; } = 0;
```

In `Scripts/Core/SaveSystem.cs`, in `SaveGame()` gather data from `QuestManager.Instance` and in `LoadGame()` restore to `QuestManager.Instance`, while keeping `SaveSystem.cs` under 250 lines.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Expected: PASS (All tests pass).

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/SaveData.cs Scripts/Core/SaveSystem.cs Tests/SaveSystemTests.cs
git commit -m "feat(quests): integrate QuestManager persistence into SaveData and SaveSystem"
```

---

### Task 4: Game Event Wiring to QuestManager

**Files:**
- Modify: `Scripts/Core/GameManager.cs` or event subscriber hook
- Test: `Tests/QuestSystemTests.cs`

**Interfaces:**
- Consumes: `GameManager`, `WaveSpawner`, `Hero`
- Produces: Quest tracking hooked cleanly into existing event emitters.

- [ ] **Step 1: Write integration tests for event forwarding**

Append to `Tests/QuestSystemTests.cs`:
```csharp
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
        }
```

- [ ] **Step 2: Connect events**

In `GameManager.cs` (or via `QuestManagerInit` helper to preserve 248 lines):
Hook `EnemyDefeated`, `StatsUpgraded`, `WaveChanged`, `BerserkStateChanged`, `TapCombat` to `QuestManager.Instance`.
Ensure `GameManager.cs` stays <= 248 lines!

- [ ] **Step 3: Run tests to verify**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Expected: PASS (0 errors, 0 warnings).

- [ ] **Step 4: Commit**

```bash
git add Scripts/Core/GameManager.cs Tests/QuestSystemTests.cs
git commit -m "feat(quests): wire game combat and progression events to QuestManager"
```

---

### Task 5: Gothic Quest Modal UI (`QuestModal.cs` & Scene)

**Files:**
- Create: `Scripts/UI/QuestModal.cs`
- Create: `Scenes/UI/QuestModal.tscn`

**Interfaces:**
- Consumes: `QuestManager`, `QuestDatabase`, `AudioManager`, `FloatingTextManager`
- Produces: Dual-tab Gothic Modal (`Milestones` and `Daily Bounties`), animated smoothly with sound and claim feedback.

- [ ] **Step 1: Implement QuestModal.cs**

Create `Scripts/UI/QuestModal.cs`:
- Responsive Control node.
- Tab bar: `[ ⚔️ KALICI BAŞARIMLAR ]` / `[ 🩸 GÜNLÜK KAN AVI ]`.
- ScrollContainer with VBoxContainer for quest items.
- Item Card:
  - Title and description.
  - Progress bar (`ProgressBar`) and text `X / Y`.
  - Reward tags (`🪙 +500`, `📜 +1`, `🩸 +1 AP`).
  - Button `Talep Et` (active & glowing when claimable, disabled when in progress, `✓ Alındı` when claimed).
- Claiming rewards gives sound + floating text and refreshes UI.
- File length < 230 lines (strict compliance with 250-line rule).

- [ ] **Step 2: Create Scenes/UI/QuestModal.tscn**

Gothic styling with dark background, crimson borders, header with close button `✕`.

- [ ] **Step 3: Build and test**

Run: `dotnet build`
Expected: 0 errors, 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add Scripts/UI/QuestModal.cs Scenes/UI/QuestModal.tscn
git commit -m "feat(quests): implement QuestModal dual-tab UI with Gothic styling and rewards claiming"
```

---

### Task 6: HUD Button & QuestController Integration

**Files:**
- Create: `Scripts/UI/QuestController.cs`
- Modify: `Scenes/MainCombat.tscn`

**Interfaces:**
- Consumes: `QuestManager`, `QuestModal`
- Produces:
  - Top HUD Glowing Quest Button (`📜 GÖREVLER (N)` with notification badge).
  - Toggling modal cleanly.
  - Keeps `MainHUD.cs` completely untouched (retaining its 247-line count).

- [ ] **Step 1: Implement QuestController.cs**

Create `Scripts/UI/QuestController.cs`:
- CanvasLayer (Layer 104).
- Adds/positions a Gothic styled quest icon button near top-right (e.g. `X: 1680, Y: 16` or right side panel).
- Connects to `QuestManager.Instance.OnQuestProgressUpdated`, `OnQuestCompleted`, `OnDailyQuestsRefreshed`.
- Updates badge text: `📜 GÖREVLER` or `📜 GÖREVLER (2) 🔥`.
- Instantiates `QuestModal` and controls visibility with tweens.

- [ ] **Step 2: Add QuestController to Scenes/MainCombat.tscn**

Add `QuestController` node to `MainCombat.tscn` alongside `TutorialController`.

- [ ] **Step 3: Build & Smoke Test**

Run: `dotnet build`
Run: `C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 60`
Expected: 0 warnings, 0 errors, clean exit.

- [ ] **Step 4: Commit**

```bash
git add Scripts/UI/QuestController.cs Scenes/MainCombat.tscn
git commit -m "feat(quests): integrate QuestController with glowing HUD badge into MainCombat scene"
```

---

### Task 7: Full Verification and Smoke Testing Gate

**Files:**
- Test: All tests in `Tests/`

- [ ] **Step 1: Run all unit tests**
Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Expected: 80+ tests pass with 0 failures.

- [ ] **Step 2: Line count verification**
Verify all files in `Scripts/Core` and `Scripts/UI` are strictly under 250 lines:
Run: PowerShell line count check.

- [ ] **Step 3: Godot headless engine smoke test**
Run: `C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 60`
Expected: Exit code 0, 0 resource leak errors.

- [ ] **Step 4: Final commit and documentation**
Update `docs/superpowers/HANDOVER.md` and commit.
