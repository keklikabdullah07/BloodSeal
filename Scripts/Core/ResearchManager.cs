#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public class ResearchManager
    {
        private static ResearchManager? _instance;
        public static ResearchManager Instance => _instance ??= new ResearchManager();

        public int LoreScrolls { get; private set; } = 0;
        private readonly Dictionary<string, int> _researchLevels = new();
        private readonly HashSet<int> _defeatedMilestoneBosses = new();

        public event Action<string, int>? OnResearchUpgraded;
        public event Action<int>? OnLoreScrollsChanged;

        public static void SetInstance(ResearchManager instance)
        {
            _instance = instance;
        }

        public void Reset()
        {
            LoreScrolls = 0;
            _researchLevels.Clear();
            _defeatedMilestoneBosses.Clear();
        }

        public int GetResearchLevel(string id)
        {
            return _researchLevels.TryGetValue(id, out int lvl) ? lvl : 0;
        }

        public void SetResearchLevel(string id, int level)
        {
            _researchLevels[id] = level;
        }

        public Dictionary<string, int> GetAllLevels() => new(_researchLevels);
        public List<int> GetDefeatedMilestones() => new(_defeatedMilestoneBosses);

        public void AddLoreScrolls(int count)
        {
            if (count <= 0) return;
            LoreScrolls += count;
            OnLoreScrollsChanged?.Invoke(LoreScrolls);
        }

        public bool SpendLoreScrolls(int count)
        {
            if (LoreScrolls >= count)
            {
                LoreScrolls -= count;
                OnLoreScrollsChanged?.Invoke(LoreScrolls);
                return true;
            }
            return false;
        }

        public bool HasDefeatedMilestoneBoss(int wave) => _defeatedMilestoneBosses.Contains(wave);

        public void RecordMilestoneBossDefeated(int wave)
        {
            _defeatedMilestoneBosses.Add(wave);
        }

        public bool CanUpgradeResearch(string id, double currentGold)
        {
            var node = ResearchDatabase.GetNode(id);
            if (node == null) return false;

            int currentLvl = GetResearchLevel(id);
            if (currentLvl >= node.MaxLevel) return false;

            int nextLvl = currentLvl + 1;
            double goldCost = node.GetGoldCost(nextLvl);
            int scrollCost = node.GetScrollCost(nextLvl);

            return currentGold >= goldCost && LoreScrolls >= scrollCost;
        }

        public bool TryUpgradeResearch(string id, ref double currentGold)
        {
            var node = ResearchDatabase.GetNode(id);
            if (node == null) return false;

            int currentLvl = GetResearchLevel(id);
            if (currentLvl >= node.MaxLevel) return false;

            int nextLvl = currentLvl + 1;
            double goldCost = node.GetGoldCost(nextLvl);
            int scrollCost = node.GetScrollCost(nextLvl);

            if (currentGold < goldCost || LoreScrolls < scrollCost)
                return false;

            currentGold -= goldCost;
            if (scrollCost > 0)
            {
                SpendLoreScrolls(scrollCost);
            }

            _researchLevels[id] = nextLvl;
            OnResearchUpgraded?.Invoke(id, nextLvl);
            return true;
        }

        // Gameplay effect getters
        public float GetSealCostDiscountMultiplier()
        {
            var node = ResearchDatabase.GetNode("Econ_SealEfficiency");
            if (node == null) return 1.0f;
            float discount = node.GetEffectValue(GetResearchLevel(node.Id));
            return Math.Max(0.5f, 1.0f - discount);
        }

        public double GetGoldBountyMultiplier()
        {
            var node = ResearchDatabase.GetNode("Econ_GoldBounty");
            if (node == null) return 1.0;
            return 1.0 + node.GetEffectValue(GetResearchLevel(node.Id));
        }

        public double GetBossTributeMultiplier()
        {
            var node = ResearchDatabase.GetNode("Econ_BossTribute");
            if (node == null) return 1.0;
            return 1.0 + node.GetEffectValue(GetResearchLevel(node.Id));
        }

        public long GetOfflineCapBonusSeconds()
        {
            var node = ResearchDatabase.GetNode("Mem_OfflineCap");
            if (node == null) return 0;
            return (long)node.GetEffectValue(GetResearchLevel(node.Id));
        }

        public double GetOfflineYieldMultiplier()
        {
            var node = ResearchDatabase.GetNode("Mem_OfflineYield");
            if (node == null) return 1.0;
            return 1.0 + node.GetEffectValue(GetResearchLevel(node.Id));
        }

        public float GetTapDamageMultiplier()
        {
            var node = ResearchDatabase.GetNode("War_TapMastery");
            if (node == null) return 1.0f;
            return 1.0f + node.GetEffectValue(GetResearchLevel(node.Id));
        }

        public float GetPetMultiplier()
        {
            var node = ResearchDatabase.GetNode("War_PetFrequency");
            if (node == null) return 1.0f;
            return 1.0f + node.GetEffectValue(GetResearchLevel(node.Id));
        }

        public double GetBerserkBonusDuration()
        {
            var node = ResearchDatabase.GetNode("War_BerserkProlong");
            if (node == null) return 0.0;
            return node.GetEffectValue(GetResearchLevel(node.Id));
        }
    }
}
