# Malikane Kütüphanesi & Araştırma Sistemi (Research Tree) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Malikane Kapısı ve Boss zaferlerinden kazanılan Kadim Parşömenler ve Altın ile 3 disiplinli (Kadim Ekonomi, Kan Hafızası, Savaş Ezoterizmi) Malikane Kütüphanesi Araştırma Ağacı'nı, arayüzünü ve oyun sistemleri entegrasyonunu hayata geçirmek.

**Architecture:** 250 satır kuralını ve "Call Down, Signal Up" prensibini korumak için araştırma mantığı bağımsız bir `ResearchManager` sınıfında toplanacak. Veri modelleri `ResearchModels.cs` içinde tutulacak, `SaveData` geriye dönük uyumlu şekilde güncellenecek, UI ise modüler `LibraryModal.cs` ve `MainHUD` butonu ile sunulacaktır.

**Tech Stack:** Godot 4.7.x Mono (C#), .NET 10.0 (`net10.0`), NUnit / xUnit test runner (`dotnet test`).

## Global Constraints
- Target Framework: `net10.0` (Android export koşulu için `BloodSeal.csproj`'daki Condition korunmalı).
- Dosya satır sınırı: Hiçbir C# dosyası 250 satırı aşamaz (`GameManager.cs` 248 satır sınırında olduğu için yeni araştırma alanlarıyla şişirilmemeli, yetki `ResearchManager`'a devredilmeli).
- "Call Down, Signal Up": UI çocuk düğümleri event yayar (`event Action`), ebeveynlerini aramak için `GetParent()` kullanmaz.
- `Engine.TimeScale`'e asla dokunulmaz.
- Sayı hijyeni: Altın ve hasar hesaplarında `double`, sürelerde `DateTimeOffset` kullanılır.

---

### Task 1: Research Models & Data Structures (`ResearchModels.cs`)

**Files:**
- Create: `Scripts/Core/ResearchModels.cs`
- Test: `Tests/ResearchSystemTests.cs`

**Interfaces:**
- Produces:
  - `enum ResearchDiscipline { Economy, BloodMemory, CombatEsotericism }`
  - `class ResearchNodeDefinition { string Id, ResearchDiscipline Discipline, string Name, string Description, int MaxLevel, double BaseGoldCost, double CostGrowthRate, int ScrollCostStep, Func<int, float> EffectFormula }`
  - `class ResearchDatabase` containing definitions for all 9 research nodes (`Econ_GoldBounty`, `Econ_SealEfficiency`, `Econ_BossTribute`, `Mem_OfflineCap`, `Mem_OfflineYield`, `Mem_DeepSlumber`, `War_TapMastery`, `War_PetFrequency`, `War_BerserkProlong`).

- [ ] **Step 1: Write failing unit test for Research Models definitions**

```csharp
// In Tests/ResearchSystemTests.cs
using System;
using System.Linq;
using NUnit.Framework;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    [TestFixture]
    public class ResearchSystemTests
    {
        [Test]
        public void ResearchDatabase_ContainsNineDefinedNodes_AcrossThreeDisciplines()
        {
            var nodes = ResearchDatabase.GetAllNodes();
            Assert.That(nodes.Count, Is.EqualTo(9));
            Assert.That(nodes.Count(n => n.Discipline == ResearchDiscipline.Economy), Is.EqualTo(3));
            Assert.That(nodes.Count(n => n.Discipline == ResearchDiscipline.BloodMemory), Is.EqualTo(3));
            Assert.That(nodes.Count(n => n.Discipline == ResearchDiscipline.CombatEsotericism), Is.EqualTo(3));
        }

        [Test]
        public void ResearchNode_CalculatesCostCorrectly()
        {
            var node = ResearchDatabase.GetNode("Econ_GoldBounty");
            Assert.That(node, Is.Not.Null);
            double costLv1 = node.GetGoldCost(1);
            double costLv2 = node.GetGoldCost(2);
            Assert.That(costLv1, Is.EqualTo(node.BaseGoldCost));
            Assert.That(costLv2, Is.GreaterThan(costLv1));
            Assert.That(node.GetScrollCost(1), Is.EqualTo(0));
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "ResearchSystemTests"`
Expected: FAIL (types do not exist yet).

- [ ] **Step 3: Implement `Scripts/Core/ResearchModels.cs`**

```csharp
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
        public string Id { get; set; }
        public ResearchDiscipline Discipline { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
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
        public static ResearchNodeDefinition GetNode(string id) => _nodes.TryGetValue(id, out var n) ? n : null;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "ResearchSystemTests"`
Expected: PASS.

- [ ] **Step 5: Commit Task 1**

```bash
git add Scripts/Core/ResearchModels.cs Tests/ResearchSystemTests.cs
git commit -m "feat(research): implement research node definitions and database"
```

---

### Task 2: Research Manager Logic (`ResearchManager.cs`)

**Files:**
- Create: `Scripts/Core/ResearchManager.cs`
- Modify: `Tests/ResearchSystemTests.cs`

**Interfaces:**
- Produces:
  - `class ResearchManager` (Singleton / Service)
  - `int LoreScrolls { get; set; }`
  - `int GetResearchLevel(string id)`
  - `bool CanUpgradeResearch(string id, double currentGold)`
  - `bool TryUpgradeResearch(string id, ref double currentGold)`
  - `void AddLoreScrolls(int count)`
  - `void RecordMilestoneBossDefeated(int wave)`
  - `bool HasDefeatedMilestoneBoss(int wave)`
  - Multiplier getter methods: `GetSealCostDiscountMultiplier()`, `GetGoldBountyMultiplier()`, `GetBossTributeMultiplier()`, `GetOfflineCapBonusSeconds()`, `GetOfflineYieldMultiplier()`, `GetTapDamageMultiplier()`, `GetPetMultiplier()`, `GetBerserkBonusDuration()`
  - Events: `event Action<string, int> OnResearchUpgraded`, `event Action<int> OnLoreScrollsChanged`

- [ ] **Step 1: Write failing unit test for `ResearchManager` upgrade and calculations**

```csharp
// In Tests/ResearchSystemTests.cs
[Test]
public void ResearchManager_TryUpgrade_ConsumesGoldAndScrollsCorrectly()
{
    var manager = new ResearchManager();
    manager.AddLoreScrolls(2);
    double gold = 1000.0;

    Assert.That(manager.GetResearchLevel("Econ_SealEfficiency"), Is.EqualTo(0));
    bool success = manager.TryUpgradeResearch("Econ_SealEfficiency", ref gold);
    
    Assert.That(success, Is.True);
    Assert.That(manager.GetResearchLevel("Econ_SealEfficiency"), Is.EqualTo(1));
    Assert.That(manager.LoreScrolls, Is.EqualTo(1)); // Consumed 1 scroll
    Assert.That(gold, Is.LessThan(1000.0));
    Assert.That(manager.GetSealCostDiscountMultiplier(), Is.EqualTo(0.98f)); // -2% discount -> 0.98x
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "ResearchManager_TryUpgrade"`
Expected: FAIL (`ResearchManager` not found).

- [ ] **Step 3: Implement `Scripts/Core/ResearchManager.cs`**

```csharp
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public class ResearchManager
    {
        private static ResearchManager _instance;
        public static ResearchManager Instance => _instance ??= new ResearchManager();

        public int LoreScrolls { get; private set; } = 0;
        private readonly Dictionary<string, int> _researchLevels = new();
        private readonly HashSet<int> _defeatedMilestoneBosses = new();

        public event Action<string, int> OnResearchUpgraded;
        public event Action<int> OnLoreScrollsChanged;

        public static void SetInstance(ResearchManager instance)
        {
            _instance = instance;
        }

        public void Reset()
        {
            LoreScrolls = 0;
            _researchLevels.Clear;
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
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "ResearchManager_TryUpgrade"`
Expected: PASS.

- [ ] **Step 5: Commit Task 2**

```bash
git add Scripts/Core/ResearchManager.cs Tests/ResearchSystemTests.cs
git commit -m "feat(research): implement ResearchManager logic and stat multipliers"
```

---

### Task 3: Persistence Integration (`SaveData.cs` & `SaveSystem.cs`)

**Files:**
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Core/SaveSystem.cs`
- Modify: `Tests/SaveSystemTests.cs`

**Interfaces:**
- `SaveData.LoreScrolls`: `int`
- `SaveData.ResearchLevels`: `Dictionary<string, int>`
- `SaveData.DefeatedMilestoneBosses`: `List<int>`
- `SaveSystem.CaptureSaveData(GameManager gm)` serializes research fields from `ResearchManager.Instance`.
- `SaveSystem.ApplySaveData(SaveData data, GameManager gm)` restores research levels and scrolls into `ResearchManager.Instance`.

- [ ] **Step 1: Write failing unit test for research persistence in `SaveSystemTests.cs`**

```csharp
[Test]
public void SaveSystem_PersistsAndRestores_ResearchLevelsAndLoreScrolls()
{
    var rm = ResearchManager.Instance;
    rm.Reset();
    rm.AddLoreScrolls(5);
    rm.SetResearchLevel("Econ_GoldBounty", 3);
    rm.RecordMilestoneBossDefeated(10);

    var data = SaveSystem.CaptureSaveData(null);
    Assert.That(data.LoreScrolls, Is.EqualTo(5));
    Assert.That(data.ResearchLevels["Econ_GoldBounty"], Is.EqualTo(3));
    Assert.That(data.DefeatedMilestoneBosses.Contains(10), Is.True);

    rm.Reset();
    Assert.That(rm.LoreScrolls, Is.EqualTo(0));

    SaveSystem.ApplySaveData(data, null);
    Assert.That(rm.LoreScrolls, Is.EqualTo(5));
    Assert.That(rm.GetResearchLevel("Econ_GoldBounty"), Is.EqualTo(3));
    Assert.That(rm.HasDefeatedMilestoneBoss(10), Is.True);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "SaveSystem_PersistsAndRestores_ResearchLevelsAndLoreScrolls"`
Expected: FAIL (missing fields in SaveData).

- [ ] **Step 3: Update `SaveData.cs` and `SaveSystem.cs`**

In `Scripts/Core/SaveData.cs`:
```csharp
// Manor Library Research Progression (GDD Section 2 & 4.4)
public int LoreScrolls { get; set; } = 0;
public Dictionary<string, int> ResearchLevels { get; set; } = new();
public List<int> DefeatedMilestoneBosses { get; set; } = new();
```

In `Scripts/Core/SaveSystem.cs`:
In `CaptureSaveData`:
```csharp
var rm = ResearchManager.Instance;
if (rm != null)
{
    data.LoreScrolls = rm.LoreScrolls;
    data.ResearchLevels = rm.GetAllLevels();
    data.DefeatedMilestoneBosses = rm.GetDefeatedMilestones();
}
```

In `ApplySaveData`:
```csharp
var rm = ResearchManager.Instance;
if (rm != null)
{
    rm.Reset();
    if (data.LoreScrolls > 0) rm.AddLoreScrolls(data.LoreScrolls);
    if (data.ResearchLevels != null)
    {
        foreach (var kvp in data.ResearchLevels)
            rm.SetResearchLevel(kvp.Key, kvp.Value);
    }
    if (data.DefeatedMilestoneBosses != null)
    {
        foreach (int w in data.DefeatedMilestoneBosses)
            rm.RecordMilestoneBossDefeated(w);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "SaveSystem_PersistsAndRestores_ResearchLevelsAndLoreScrolls"`
Expected: PASS.

- [ ] **Step 5: Commit Task 3**

```bash
git add Scripts/Core/SaveData.cs Scripts/Core/SaveSystem.cs Tests/SaveSystemTests.cs
git commit -m "feat(save): add research levels, lore scrolls and boss milestones persistence"
```

---

### Task 4: Gameplay Subsystems Integration

**Files:**
- Modify: `Scripts/Combat/PentagramStats.cs`
- Modify: `Scripts/Core/OfflineProgressCalculator.cs`
- Modify: `Scripts/Combat/TapCombatArea.cs`
- Modify: `Scripts/Combat/PetCompanion.cs`
- Modify: `Scripts/Combat/BossEnemy.cs`
- Modify: `Scripts/Core/GameManager.cs` (Ensure line limit <= 250 lines!)

**Interfaces:**
- `PentagramStats`: applies `ResearchManager.Instance.GetSealCostDiscountMultiplier()` on all cost calculations.
- `GameManager.CalculateGoldReward`: multiplies with `ResearchManager.Instance.GetGoldBountyMultiplier()`.
- `GameManager.TriggerRage`: adds `ResearchManager.Instance.GetBerserkBonusDuration()`.
- `OfflineProgressCalculator`: adds `GetOfflineCapBonusSeconds()` to max cap and multiplies with `GetOfflineYieldMultiplier()`.
- `TapCombatArea`: scales tap damage with `GetTapDamageMultiplier()`.
- `PetCompanion`: scales pet damage and projectile speed with `GetPetMultiplier()`.
- `BossEnemy`: when dead, checks if wave is milestone (wave % 10 == 0) -> awards guaranteed scroll if first kill, or 10% chance if repeated, spawns floating text and calls `ResearchManager.Instance.AddLoreScrolls(1)`.

- [ ] **Step 1: Write unit tests verifying gameplay multiplier integrations**

```csharp
// In Tests/ResearchSystemTests.cs
[Test]
public void PentagramStats_AppliesResearchDiscount_ToAllStatCosts()
{
    var rm = ResearchManager.Instance;
    rm.Reset();
    var stats = new PentagramStats();
    double baseAtkCost = stats.GetAtkCost();

    rm.SetResearchLevel("Econ_SealEfficiency", 2); // 4% discount -> 0.96x
    double discountedCost = stats.GetAtkCost();

    Assert.That(discountedCost, Is.EqualTo(baseAtkCost * 0.96).Within(0.01));
}

[Test]
public void OfflineProgressCalculator_IncludesResearchCapAndYieldBonus()
{
    var rm = ResearchManager.Instance;
    rm.Reset();
    rm.SetResearchLevel("Mem_OfflineCap", 2); // +2 hours (7200s) -> max 28800s (8 hours)
    rm.SetResearchLevel("Mem_OfflineYield", 3); // +30% yield -> 1.30x

    var save = new SaveData { LastSaveTimestamp = 0, HighestWave = 10 };
    long now = 36000; // 10 hours later
    var result = OfflineProgressCalculator.Calculate(save, now);

    Assert.That(result.ElapsedSeconds, Is.EqualTo(28800)); // Clamped to 8 hours
    Assert.That(result.GoldEarned, Is.GreaterThan(0));
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "PentagramStats_AppliesResearchDiscount"`
Expected: FAIL (discount not yet integrated).

- [ ] **Step 3: Update `Scripts/Combat/PentagramStats.cs`**

Modify cost formulas to multiply with `ResearchManager.Instance.GetSealCostDiscountMultiplier()`.

- [ ] **Step 4: Update `Scripts/Core/OfflineProgressCalculator.cs`**

Update `MaxOfflineSeconds` to `BaseMaxOfflineSeconds (21600) + ResearchManager.Instance.GetOfflineCapBonusSeconds()`.
Multiply final gold with `ResearchManager.Instance.GetOfflineYieldMultiplier()`.

- [ ] **Step 5: Update `Scripts/Combat/TapCombatArea.cs` and `Scripts/Combat/PetCompanion.cs`**

In `TapCombatArea.cs`: Multiply tap damage with `ResearchManager.Instance.GetTapDamageMultiplier()`.
In `PetCompanion.cs`: Multiply pet shoot rate and damage with `ResearchManager.Instance.GetPetMultiplier()`.

- [ ] **Step 6: Update `Scripts/Combat/BossEnemy.cs`**

On boss death:
Check if milestone kill or 10% RNG:
```csharp
int wave = GameManager.Instance?.CurrentWave ?? 10;
var rm = ResearchManager.Instance;
if (rm != null)
{
    bool isMilestone = !rm.HasDefeatedMilestoneBoss(wave);
    bool dropsScroll = isMilestone || (GD.Randf() <= 0.10f);

    if (dropsScroll)
    {
        rm.AddLoreScrolls(1);
        if (isMilestone) rm.RecordMilestoneBossDefeated(wave);
        FloatingTextManager.Instance?.SpawnMessage(
            GlobalPosition + new Vector2(0, -60),
            "📜 Kadim Parşömen Ele Geçirildi!",
            new Color(0.95f, 0.85f, 0.3f)
        );
    }
}
```

- [ ] **Step 7: Update `Scripts/Core/GameManager.cs`**

Add gold bounty and boss tribute multiplier to `CalculateGoldReward`:
```csharp
double bounty = ResearchManager.Instance?.GetGoldBountyMultiplier() ?? 1.0;
return baseGold * mult * bounty;
```
In `TriggerRage`:
```csharp
_rageActiveTimer = 10.0 + (ResearchManager.Instance?.GetBerserkBonusDuration() ?? 0.0);
```
Ensure `GameManager.cs` total lines is <= 245 lines!

- [ ] **Step 8: Run tests to verify they pass**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Expected: PASS for all tests.

- [ ] **Step 9: Commit Task 4**

```bash
git add Scripts/Combat/PentagramStats.cs Scripts/Core/OfflineProgressCalculator.cs Scripts/Combat/TapCombatArea.cs Scripts/Combat/PetCompanion.cs Scripts/Combat/BossEnemy.cs Scripts/Core/GameManager.cs Tests/ResearchSystemTests.cs
git commit -m "feat(combat): integrate research multipliers into pentagram, offline progress, combat and boss loot"
```

---

### Task 5: UI Modal & HUD Integration (`LibraryModal.cs` & `MainHUD.cs`)

**Files:**
- Create: `Scripts/UI/LibraryModal.cs`
- Create: `Scenes/UI/LibraryModal.tscn`
- Modify: `Scripts/UI/MainHUD.cs`
- Modify: `Scenes/UI/MainHUD.tscn`

**Interfaces:**
- `LibraryModal`:
  - Category buttons: `EconTabBtn`, `MemoryTabBtn`, `WarTabBtn`, `CloseBtn`
  - Labels: `GoldLabel`, `ScrollLabel`, `CategoryTitleLabel`
  - Container: `VBoxContainer CardsContainer`
  - Generates cards dynamically or updates cards for current discipline.
  - Connects to `ResearchManager.OnResearchUpgraded` and `OnLoreScrollsChanged` to refresh dynamically.
- `MainHUD`:
  - `[Export] public Button LibraryBtn;`
  - `[Export] public LibraryModal LibraryModal;`
  - Unlocks when `CurrentWave >= 5 || ResearchManager.Instance.LoreScrolls > 0`.
  - Tapping `LibraryBtn` opens `LibraryModal.ShowModal()`.

- [ ] **Step 1: Create `Scripts/UI/LibraryModal.cs`**

Implement Gothic styled responsive modal with 3 discipline tabs, card generation with name, description, level progress, cost badges and upgrade buttons.

- [ ] **Step 2: Create `Scenes/UI/LibraryModal.tscn`**

Create responsive Control node with dark gothic styling, header with Gold/Scroll indicators, 3 discipline tab buttons, ScrollContainer for cards, and Close button.

- [ ] **Step 3: Update `MainHUD.cs` and `MainHUD.tscn`**

Wire `LibraryBtn` to `LibraryModal.ShowModal()`. Update visibility based on wave / scrolls.
Check line count of `MainHUD.cs` to ensure <= 250 lines.

- [ ] **Step 4: Verify build**

Run: `dotnet build`
Expected: 0 warnings, 0 errors.

- [ ] **Step 5: Commit Task 5**

```bash
git add Scripts/UI/LibraryModal.cs Scenes/UI/LibraryModal.tscn Scripts/UI/MainHUD.cs Scenes/UI/MainHUD.tscn
git commit -m "feat(ui): implement Gothic LibraryModal and integrate HUD library access"
```

---

### Task 6: Comprehensive Verification Gate & Smoke Test

**Files:**
- Modify/Create: `Tests/ResearchSystemTests.cs`

- [ ] **Step 1: Run complete test suite**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --logger "console;verbosity=detailed"`
Expected: All tests pass.

- [ ] **Step 2: Run Headless Godot Smoke Test**

Run: `C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 60`
Expected: Exit code 0, no scene crashes.

- [ ] **Step 3: Check line counts for all modified classes**

Ensure no C# file exceeds 250 lines (`AGENTS.md` and `GEMINI.md` hard limit).

- [ ] **Step 4: Commit and finalize**

```bash
git commit -m "chore: complete verification gate for manor library research tree"
```
