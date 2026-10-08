#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public class RelicManager
    {
        private static RelicManager? _instance;
        public static RelicManager Instance => _instance ??= new RelicManager();

        private readonly HashSet<string> _collectedRelics = new();

        public event Action<RelicDefinition>? OnRelicUnlocked;

        public static void SetInstance(RelicManager instance) => _instance = instance;

        public void Reset()
        {
            _collectedRelics.Clear();
        }

        public bool HasRelic(string id) => _collectedRelics.Contains(id);
        public int GetCollectedCount() => _collectedRelics.Count;
        public List<string> GetAllCollectedIds() => new(_collectedRelics);

        public bool UnlockRelic(string id)
        {
            var relic = RelicDatabase.GetRelic(id);
            if (relic == null || _collectedRelics.Contains(id)) return false;

            _collectedRelics.Add(id);
            OnRelicUnlocked?.Invoke(relic);
            return true;
        }

        public bool UnlockRelicForWave(int wave)
        {
            var relic = RelicDatabase.GetRelicForWave(wave);
            if (relic == null) return false;
            return UnlockRelic(relic.Id);
        }

        // Cumulative Bonus Calculations
        public float GetDamageBonus()
        {
            float bonus = 0f;
            if (HasRelic("Relic_DariusRing")) bonus += 0.05f;
            if (HasRelic("Relic_FirstScroll")) bonus += 0.15f; // AllStats
            return bonus;
        }

        public float GetHpBonus()
        {
            float bonus = 0f;
            if (HasRelic("Relic_TornPortrait")) bonus += 0.05f;
            if (HasRelic("Relic_FirstScroll")) bonus += 0.15f; // AllStats
            return bonus;
        }

        public float GetGoldBonus()
        {
            float bonus = 0f;
            if (HasRelic("Relic_CovenantMedallion")) bonus += 0.05f;
            if (HasRelic("Relic_FirstScroll")) bonus += 0.15f; // AllStats
            return bonus;
        }

        public float GetAttackSpeedBonus()
        {
            float bonus = 0f;
            if (HasRelic("Relic_KnightSpur")) bonus += 0.05f;
            if (HasRelic("Relic_FirstScroll")) bonus += 0.15f; // AllStats
            return bonus;
        }

        public float GetLifestealBonus() => HasRelic("Relic_BoneChalice") ? 0.5f : 0f;
        public float GetRageGainBonus() => HasRelic("Relic_BlackenedBell") ? 0.05f : 0f;
        public float GetTapDamageBonus() => HasRelic("Relic_InquisitorMask") ? 0.10f : 0f;
        public float GetOfflineIncomeBonus() => HasRelic("Relic_CryptKey") ? 0.10f : 0f;
        public float GetAwakeningBonus() => HasRelic("Relic_ExtinguishedLantern") ? 0.10f : 0f;
    }
}
