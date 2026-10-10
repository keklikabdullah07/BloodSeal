#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public class FamiliarManager
    {
        private static FamiliarManager? _instance;
        public static FamiliarManager Instance => _instance ??= new FamiliarManager();

        public string ActiveFamiliarId { get; private set; } = "blood_raven";
        private readonly Dictionary<string, FamiliarProgress> _progresses = new();

        public event Action<string>? OnActiveFamiliarChanged;
        public event Action<string, int>? OnFamiliarUpgraded;
        public event Action<string>? OnFamiliarUnlocked;

        public FamiliarManager()
        {
            ResetToDefaults();
        }

        public static void SetInstanceForTesting(FamiliarManager instance)
        {
            _instance = instance;
        }

        public void ResetToDefaults()
        {
            _progresses.Clear();
            foreach (var def in FamiliarDatabase.GetAll())
            {
                _progresses[def.Id] = new FamiliarProgress
                {
                    Id = def.Id,
                    Level = 1,
                    IsUnlocked = (def.UnlockWaveRequirement <= 1)
                };
            }
            ActiveFamiliarId = "blood_raven";
        }

        public FamiliarProgress GetProgress(string id)
        {
            if (_progresses.TryGetValue(id, out var p)) return p;
            var def = FamiliarDatabase.Get(id);
            var newProgress = new FamiliarProgress { Id = def.Id, Level = 1, IsUnlocked = def.UnlockWaveRequirement <= 1 };
            _progresses[id] = newProgress;
            return newProgress;
        }

        public double GetGoldCost(string id)
        {
            var def = FamiliarDatabase.Get(id);
            var prog = GetProgress(id);
            return Math.Floor(def.BaseUpgradeCost * Math.Pow(1.15, prog.Level - 1));
        }

        public int GetParchmentCost(string id)
        {
            var prog = GetProgress(id);
            if (prog.Level % 10 == 9)
            {
                int nextTier = (prog.Level / 10) + 1;
                return nextTier * 5;
            }
            return 0;
        }

        public bool CanUpgrade(string id, double currentGold, int currentParchments)
        {
            var prog = GetProgress(id);
            if (!prog.IsUnlocked) return false;
            double goldCost = GetGoldCost(id);
            int parchCost = GetParchmentCost(id);
            return currentGold >= goldCost && currentParchments >= parchCost;
        }

        public bool Upgrade(string id, ref double currentGold, ref int currentParchments)
        {
            if (!CanUpgrade(id, currentGold, currentParchments)) return false;

            double goldCost = GetGoldCost(id);
            int parchCost = GetParchmentCost(id);

            currentGold -= goldCost;
            currentParchments -= parchCost;

            var prog = GetProgress(id);
            prog.Level++;
            OnFamiliarUpgraded?.Invoke(id, prog.Level);
            return true;
        }

        public bool SetActiveFamiliar(string id)
        {
            var prog = GetProgress(id);
            if (!prog.IsUnlocked) return false;
            if (ActiveFamiliarId == id) return true;

            ActiveFamiliarId = id;
            OnActiveFamiliarChanged?.Invoke(id);
            return true;
        }

        public void CheckWaveUnlocks(int wave)
        {
            foreach (var def in FamiliarDatabase.GetAll())
            {
                var prog = GetProgress(def.Id);
                if (!prog.IsUnlocked && wave >= def.UnlockWaveRequirement)
                {
                    prog.IsUnlocked = true;
                    OnFamiliarUnlocked?.Invoke(def.Id);
                }
            }
        }

        public float GetActivePetDamage(float heroAtk, float researchMultiplier = 1.0f)
        {
            var def = FamiliarDatabase.Get(ActiveFamiliarId);
            var prog = GetProgress(ActiveFamiliarId);
            float levelBonus = 1.0f + 0.08f * (prog.Level - 1);
            return heroAtk * def.BaseAttackMultiplier * levelBonus * Math.Max(0.1f, researchMultiplier);
        }

        public float GetActiveAttackInterval()
        {
            var def = FamiliarDatabase.Get(ActiveFamiliarId);
            return def.BaseAttackInterval;
        }

        public float GetCritChanceBonus()
        {
            if (ActiveFamiliarId != "shadow_bat") return 0.0f;
            var prog = GetProgress(ActiveFamiliarId);
            return Math.Min(0.20f, 0.05f + prog.Level * 0.005f);
        }

        public float GetCritDamageBonus()
        {
            if (ActiveFamiliarId != "shadow_bat") return 0.0f;
            var prog = GetProgress(ActiveFamiliarId);
            return Math.Min(0.60f, 0.15f + prog.Level * 0.01f);
        }

        public float GetGoldMultiplierBonus()
        {
            if (ActiveFamiliarId != "crimson_hound") return 0.0f;
            var prog = GetProgress(ActiveFamiliarId);
            return Math.Min(0.60f, 0.15f + prog.Level * 0.01f);
        }

        public float GetDamageReductionBonus()
        {
            if (ActiveFamiliarId != "stone_gargoyle") return 0.0f;
            var prog = GetProgress(ActiveFamiliarId);
            return Math.Min(0.35f, 0.10f + prog.Level * 0.005f);
        }

        public Dictionary<string, FamiliarProgress> GetAllProgresses() => _progresses;

        public void LoadProgresses(string activeId, Dictionary<string, FamiliarProgress> saved)
        {
            ResetToDefaults();
            if (saved != null)
            {
                foreach (var pair in saved)
                {
                    _progresses[pair.Key] = pair.Value;
                }
            }
            if (!string.IsNullOrEmpty(activeId) && _progresses.ContainsKey(activeId))
            {
                ActiveFamiliarId = activeId;
            }
        }
    }
}
