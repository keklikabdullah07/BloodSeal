#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodSeal.Core
{
    public class QuestManager
    {
        private static QuestManager? _instance;
        public static QuestManager Instance => _instance ??= new QuestManager();

        private readonly Dictionary<string, QuestProgress> _progressMap = new();
        private readonly List<string> _activeDailyQuestIds = new();

        public IReadOnlyDictionary<string, QuestProgress> AllProgress => _progressMap;
        public IReadOnlyList<string> ActiveDailyQuestIds => _activeDailyQuestIds;
        public long LastDailyResetTimestamp { get; private set; }

        public event Action<QuestProgress>? OnQuestProgressUpdated;
        public event Action<QuestDefinition>? OnQuestCompleted;
        public event Action<QuestDefinition, QuestReward>? OnQuestRewardClaimed;
        public event Action? OnDailyQuestsRefreshed;

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
