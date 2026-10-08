#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public enum ResearchDiscipline
    {
        Economy = 0,
        BloodMemory = 1,
        CombatEsotericism = 2
    }

    public class ResearchNodeDefinition
    {
        public string Id { get; set; } = string.Empty;
        public ResearchDiscipline Discipline { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxLevel { get; set; }
        public double BaseGoldCost { get; set; }
        public double CostGrowthRate { get; set; } = 1.4;
        public int ScrollCostStep { get; set; } = 0; // 0: no scrolls, 1: 1 scroll, 2: 1 at lv2+, 2 at lv4+
        public string Unit { get; set; } = "%";

        public double GetGoldCost(int targetLevel)
        {
            if (targetLevel <= 1) return BaseGoldCost;
            return BaseGoldCost * Math.Pow(CostGrowthRate, targetLevel - 1);
        }

        public int GetScrollCost(int targetLevel)
        {
            if (ScrollCostStep == 0) return 0;
            if (ScrollCostStep == 1) return 1;
            if (ScrollCostStep == 2)
            {
                if (targetLevel <= 2) return 1;
                return 2;
            }
            return 1;
        }

        public float GetEffectValue(int level) => Id switch
        {
            "Econ_GoldBounty" => level * 0.05f,      // +5% per lvl
            "Econ_SealEfficiency" => level * 0.02f,  // -2% cost per lvl
            "Econ_BossTribute" => level * 0.20f,     // +20% boss gold per lvl
            "Mem_OfflineCap" => level * 3600f,       // +1 hour (in seconds) per lvl
            "Mem_OfflineYield" => level * 0.10f,     // +10% offline gold per lvl
            "Mem_DeepSlumber" => level * 0.05f,      // +5% simulation speed per lvl
            "War_TapMastery" => level * 0.15f,       // +15% tap DMG per lvl
            "War_PetFrequency" => level * 0.08f,     // +8% pet damage & speed per lvl
            "War_BerserkProlong" => level * 1.0f,    // +1s berserk duration per lvl
            _ => 0f
        };
    }

    public static class ResearchDatabase
    {
        private static readonly Dictionary<string, ResearchNodeDefinition> _nodes = new()
        {
            // Disiplin 1: Kadim Ekonomi
            ["Econ_GoldBounty"] = new ResearchNodeDefinition
            {
                Id = "Econ_GoldBounty",
                Discipline = ResearchDiscipline.Economy,
                Name = "Gasp Edilen Servet",
                Description = "Minyon ve bosslardan kazanılan altını artırır.",
                MaxLevel = 10,
                BaseGoldCost = 150.0,
                CostGrowthRate = 1.35,
                ScrollCostStep = 0
            },
            ["Econ_SealEfficiency"] = new ResearchNodeDefinition
            {
                Id = "Econ_SealEfficiency",
                Discipline = ResearchDiscipline.Economy,
                Name = "Mühür Tasarrufu",
                Description = "Pentagram statlarının altın geliştirme maliyetini düşürür.",
                MaxLevel = 5,
                BaseGoldCost = 300.0,
                CostGrowthRate = 1.5,
                ScrollCostStep = 2
            },
            ["Econ_BossTribute"] = new ResearchNodeDefinition
            {
                Id = "Econ_BossTribute",
                Discipline = ResearchDiscipline.Economy,
                Name = "Hükümdar Haraçları",
                Description = "Yenilen her Boss'tan düşen altın miktarını katlar.",
                MaxLevel = 5,
                BaseGoldCost = 450.0,
                CostGrowthRate = 1.6,
                ScrollCostStep = 1
            },

            // Disiplin 2: Kan Hafızası
            ["Mem_OfflineCap"] = new ResearchNodeDefinition
            {
                Id = "Mem_OfflineCap",
                Discipline = ResearchDiscipline.BloodMemory,
                Name = "Uykusuz Mezar",
                Description = "Çevrimdışı ilerleme süre sınırını uzatır (Temel 6 saat).",
                MaxLevel = 6,
                BaseGoldCost = 500.0,
                CostGrowthRate = 1.7,
                ScrollCostStep = 1,
                Unit = " Saat"
            },
            ["Mem_OfflineYield"] = new ResearchNodeDefinition
            {
                Id = "Mem_OfflineYield",
                Discipline = ResearchDiscipline.BloodMemory,
                Name = "Gölge Hasadı",
                Description = "Çevrimdışı kalınan süredeki altın üretim verimini artırır.",
                MaxLevel = 10,
                BaseGoldCost = 250.0,
                CostGrowthRate = 1.4,
                ScrollCostStep = 0
            },
            ["Mem_DeepSlumber"] = new ResearchNodeDefinition
            {
                Id = "Mem_DeepSlumber",
                Discipline = ResearchDiscipline.BloodMemory,
                Name = "Derin Koma Hızı",
                Description = "Çevrimdışında minyon temizleme simülasyon hızını artırır.",
                MaxLevel = 5,
                BaseGoldCost = 400.0,
                CostGrowthRate = 1.55,
                ScrollCostStep = 2
            },

            // Disiplin 3: Savaş Ezoterizmi
            ["War_TapMastery"] = new ResearchNodeDefinition
            {
                Id = "War_TapMastery",
                Discipline = ResearchDiscipline.CombatEsotericism,
                Name = "Kan Pençesi Ustalığı",
                Description = "Ekrana dokunarak verilen Tıklama Hasarını (Tap DMG) artırır.",
                MaxLevel = 10,
                BaseGoldCost = 200.0,
                CostGrowthRate = 1.35,
                ScrollCostStep = 0
            },
            ["War_PetFrequency"] = new ResearchNodeDefinition
            {
                Id = "War_PetFrequency",
                Discipline = ResearchDiscipline.CombatEsotericism,
                Name = "Gölge Ruh Senkronu",
                Description = "Süzülen 2 ruhun büyü hasarını ve atış sıklığını güçlendirir.",
                MaxLevel = 5,
                BaseGoldCost = 500.0,
                CostGrowthRate = 1.6,
                ScrollCostStep = 1
            },
            ["War_BerserkProlong"] = new ResearchNodeDefinition
            {
                Id = "War_BerserkProlong",
                Discipline = ResearchDiscipline.CombatEsotericism,
                Name = "Tükenmez Öfke",
                Description = "Berserk modunun aktif kalma süresini uzatır (Temel 10s).",
                MaxLevel = 5,
                BaseGoldCost = 600.0,
                CostGrowthRate = 1.7,
                ScrollCostStep = 2,
                Unit = " sn"
            }
        };

        public static List<ResearchNodeDefinition> GetAllNodes() => new(_nodes.Values);
        public static ResearchNodeDefinition? GetNode(string id) => _nodes.TryGetValue(id, out var n) ? n : null;
    }
}
