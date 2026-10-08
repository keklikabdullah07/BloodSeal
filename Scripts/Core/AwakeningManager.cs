#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public class AwakeningManager
    {
        public const int MinimumAwakeningWave = 20;

        private static AwakeningManager? _instance;
        public static AwakeningManager Instance => _instance ??= new AwakeningManager();

        public int AwakeningPoints { get; private set; } = 0;
        public int TotalAwakenings { get; private set; } = 0;
        private readonly Dictionary<string, int> _sealLevels = new();

        public event Action<int>? OnAwakeningPointsChanged;
        public event Action<string, int>? OnSealUpgraded;
        public event Action? OnAwakened;

        public static void SetInstance(AwakeningManager instance) => _instance = instance;

        public void Reset()
        {
            AwakeningPoints = 0;
            TotalAwakenings = 0;
            _sealLevels.Clear();
        }

        public int GetSealLevel(string id) => _sealLevels.TryGetValue(id, out int lvl) ? lvl : 0;
        public void SetSealLevel(string id, int level) => _sealLevels[id] = level;
        public void SetTotalAwakenings(int count) => TotalAwakenings = Math.Max(0, count);
        public Dictionary<string, int> GetAllLevels() => new(_sealLevels);

        public void AddAwakeningPoints(int points)
        {
            if (points <= 0) return;
            AwakeningPoints += points;
            OnAwakeningPointsChanged?.Invoke(AwakeningPoints);
        }

        public int CalculatePendingPoints(int wave)
        {
            if (wave < MinimumAwakeningWave) return 0;
            int waveDiff = wave - MinimumAwakeningWave;
            double basePoints = 1.0 + (waveDiff / 5.0) + Math.Pow(waveDiff / 15.0, 1.3);
            int points = (int)Math.Floor(basePoints);
            float harvestBonus = GetPointsMultiplier();
            return (int)Math.Floor(points * (1.0f + harvestBonus));
        }

        public bool CanAwaken(int wave) => wave >= MinimumAwakeningWave;

        public bool ExecuteAwakening(int currentWave, Action resetWorldCallback)
        {
            if (!CanAwaken(currentWave)) return false;

            int earnedPoints = CalculatePendingPoints(currentWave);
            AddAwakeningPoints(earnedPoints);
            TotalAwakenings++;

            resetWorldCallback();
            OnAwakened?.Invoke();
            return true;
        }

        public bool CanUpgradeSeal(string id)
        {
            var node = AwakeningDatabase.GetNode(id);
            if (node == null) return false;
            int currentLvl = GetSealLevel(id);
            if (currentLvl >= node.MaxLevel) return false;
            int cost = node.GetCost(currentLvl + 1);
            return AwakeningPoints >= cost;
        }

        public bool TryUpgradeSeal(string id)
        {
            var node = AwakeningDatabase.GetNode(id);
            if (node == null) return false;
            int currentLvl = GetSealLevel(id);
            if (currentLvl >= node.MaxLevel) return false;
            int cost = node.GetCost(currentLvl + 1);

            if (AwakeningPoints < cost) return false;

            AwakeningPoints -= cost;
            int newLvl = currentLvl + 1;
            _sealLevels[id] = newLvl;

            OnAwakeningPointsChanged?.Invoke(AwakeningPoints);
            OnSealUpgraded?.Invoke(id, newLvl);
            return true;
        }

        // Multipliers
        public float GetDamageMultiplier() => 1.0f + (AwakeningDatabase.GetNode("War_PrimordialMight")?.GetEffectValue(GetSealLevel("War_PrimordialMight")) ?? 0f);
        public float GetMaxHpMultiplier() => 1.0f + (AwakeningDatabase.GetNode("War_BloodAegis")?.GetEffectValue(GetSealLevel("War_BloodAegis")) ?? 0f);
        public float GetBonusLifesteal() => AwakeningDatabase.GetNode("War_VampiricThirst")?.GetEffectValue(GetSealLevel("War_VampiricThirst")) ?? 0f;
        public float GetWaveLeapChance() => AwakeningDatabase.GetNode("Flow_WaveLeap")?.GetEffectValue(GetSealLevel("Flow_WaveLeap")) ?? 0f;
        public float GetRageGainMultiplier() => 1.0f + (AwakeningDatabase.GetNode("Flow_CrimsonSurge")?.GetEffectValue(GetSealLevel("Flow_CrimsonSurge")) ?? 0f);
        public double GetStartingGold() => AwakeningDatabase.GetNode("Heritage_BloodRecall")?.GetEffectValue(GetSealLevel("Heritage_BloodRecall")) ?? 0.0;
        public float GetPointsMultiplier() => AwakeningDatabase.GetNode("Heritage_PrimordialHarvest")?.GetEffectValue(GetSealLevel("Heritage_PrimordialHarvest")) ?? 0f;
    }
}
