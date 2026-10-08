#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public enum AwakeningBranch
    {
        War = 0,
        Momentum = 1,
        Heritage = 2
    }

    public class AwakeningNodeDefinition
    {
        public string Id { get; set; } = string.Empty;
        public AwakeningBranch Branch { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxLevel { get; set; }
        public string Unit { get; set; } = "%";

        public int GetCost(int targetLevel) => Id switch
        {
            "War_PrimordialMight" => targetLevel,       // 1, 2, 3.. 10
            "War_BloodAegis" => targetLevel,            // 1, 2, 3.. 10
            "War_VampiricThirst" => targetLevel + 1,    // 2, 3, 4, 5, 6
            "Flow_WaveLeap" => targetLevel * 2,         // 2, 4, 6, 8, 10, 12
            "Flow_CrimsonSurge" => targetLevel,         // 1, 2, 3, 4, 5
            "Heritage_BloodRecall" => targetLevel switch
            {
                1 => 1, 2 => 2, 3 => 3, 4 => 5, 5 => 8, _ => targetLevel * 2
            },
            "Heritage_PrimordialHarvest" => targetLevel switch
            {
                1 => 3, 2 => 5, 3 => 7, 4 => 10, 5 => 15, _ => targetLevel * 3
            },
            _ => targetLevel
        };

        public float GetEffectValue(int level) => Id switch
        {
            "War_PrimordialMight" => level * 0.15f,     // +15% damage per lvl
            "War_BloodAegis" => level * 0.15f,          // +15% max hp per lvl
            "War_VampiricThirst" => level * 0.5f,       // +0.5% lifesteal per lvl
            "Flow_WaveLeap" => level * 0.05f,           // +5% leap chance per lvl
            "Flow_CrimsonSurge" => level * 0.20f,       // +20% rage gain per lvl
            "Heritage_BloodRecall" => level switch      // starting gold bonus
            {
                1 => 500f, 2 => 2500f, 3 => 10000f, 4 => 50000f, 5 => 250000f, _ => 0f
            },
            "Heritage_PrimordialHarvest" => level * 0.10f, // +10% AP gain per lvl
            _ => 0f
        };
    }

    public static class AwakeningDatabase
    {
        private static readonly Dictionary<string, AwakeningNodeDefinition> Nodes = new();

        static AwakeningDatabase()
        {
            Register(new AwakeningNodeDefinition
            {
                Id = "War_PrimordialMight",
                Branch = AwakeningBranch.War,
                Name = "Kadim Kudret",
                Description = "Tüm kahraman ve tıklama hasarına kalıcı çarpan sağlar.",
                MaxLevel = 10,
                Unit = "%"
            });
            Register(new AwakeningNodeDefinition
            {
                Id = "War_BloodAegis",
                Branch = AwakeningBranch.War,
                Name = "Kan Zırhı",
                Description = "Maksimum can değerine kalıcı çarpan sağlar.",
                MaxLevel = 10,
                Unit = "%"
            });
            Register(new AwakeningNodeDefinition
            {
                Id = "War_VampiricThirst",
                Branch = AwakeningBranch.War,
                Name = "Vampirik Açlık",
                Description = "Can çalma oranına doğrudan kalıcı taban yüzde ekler.",
                MaxLevel = 5,
                Unit = "%"
            });

            Register(new AwakeningNodeDefinition
            {
                Id = "Flow_WaveLeap",
                Branch = AwakeningBranch.Momentum,
                Name = "Dalga Sıçraması",
                Description = "Minyon dalgası temizlendiğinde sonraki dalgayı da doğrudan atlama şansı.",
                MaxLevel = 6,
                Unit = "%"
            });
            Register(new AwakeningNodeDefinition
            {
                Id = "Flow_CrimsonSurge",
                Branch = AwakeningBranch.Momentum,
                Name = "Kızıl Hiddet",
                Description = "Vuruşlardan kazanılan Öfke (Rage) miktarını hızlandırır.",
                MaxLevel = 5,
                Unit = "%"
            });

            Register(new AwakeningNodeDefinition
            {
                Id = "Heritage_BloodRecall",
                Branch = AwakeningBranch.Heritage,
                Name = "Kan Hafızası",
                Description = "Her Uyanış sonrası oyuna ekstra taban altınla başlarsınız.",
                MaxLevel = 5,
                Unit = " Altın"
            });
            Register(new AwakeningNodeDefinition
            {
                Id = "Heritage_PrimordialHarvest",
                Branch = AwakeningBranch.Heritage,
                Name = "Kadim Hasat",
                Description = "Uyanış yapıldığında kazanılan Uyanış Puanına kalıcı çarpan ekler.",
                MaxLevel = 5,
                Unit = "%"
            });
        }

        private static void Register(AwakeningNodeDefinition node) => Nodes[node.Id] = node;
        public static AwakeningNodeDefinition? GetNode(string id) => Nodes.GetValueOrDefault(id);
        public static IReadOnlyList<AwakeningNodeDefinition> AllNodes => new List<AwakeningNodeDefinition>(Nodes.Values);
        public static List<AwakeningNodeDefinition> GetNodesForBranch(AwakeningBranch branch)
        {
            var list = new List<AwakeningNodeDefinition>();
            foreach (var node in Nodes.Values)
            {
                if (node.Branch == branch) list.Add(node);
            }
            return list;
        }
    }
}
