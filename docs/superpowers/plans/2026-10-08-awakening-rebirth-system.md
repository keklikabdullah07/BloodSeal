# 🩸 BloodSeal: Uyanış (Awakening / Rebirth) Sistemi Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** GDD Bölüm 4.5 doğrultusunda Dalga 20+ Uyanış (Awakening / Rebirth) prestij sistemini, Uyanış Puanı (AP) kazanımını, 3 dallı ve 7 pasifli Kadim Mühürler Ağacını, sıfırlama mekaniğini, kayıt sistemini ve Gotik `AwakeningModal` arayüzünü inşa etmek.

**Architecture:** `ResearchManager` desenini takip eden bağımsız `AwakeningManager` Core servisi; statik tanımlar için `AwakeningModels.cs` ve `AwakeningDatabase`; `GameManager`'ı 250 satır sınırının altında tutmak için delegasyon metodu (`ResetForAwakening`); savaş sistemlerine (`PentagramStats`, `WaveSpawner`, `TapCombatArea`) dinamik çarpan enjeksiyonu; ve `LibraryModal` görsel standardında iki panelli Gotik `AwakeningModal`.

**Tech Stack:** C# 12 / .NET 10.0 (`net10.0`), Godot 4.7.x Mono, xUnit test paketi (`Tests/BloodSeal.Tests.csproj`).

## Global Constraints
- **AGENTS.md Satır Sınırı:** Hiçbir C# dosyası 250 satırı aşamaz (`GameManager.cs` şu an 236 satırda olduğundan ek mantık `AwakeningManager.cs` içine alınır).
- **TimeScale Yasağı:** `Engine.TimeScale` değiştirilemez.
- **Sayı Hijyeni:** Para birimlerinde `double` ve `long` kullanılır.
- **Call Down, Signal Up:** Modallar ve alt düğümler yukarıya event yayar, ebeveynler alt nesneleri çağırır.
- **0 Uyarı 0 Hata:** `dotnet build` ve `dotnet test` her görev sonunda 0 hata ile geçmelidir.

---

### Task 1: Kadim Mühür Tanımları ve Veritabanı (`AwakeningModels.cs`)

**Files:**
- Create: `Scripts/Core/AwakeningModels.cs`
- Create: `Tests/AwakeningSystemTests.cs`

**Interfaces:**
- Produces: `enum AwakeningBranch`, `class AwakeningNodeDefinition`, `class AwakeningDatabase` (`GetNode(string id)`, `GetNodesForBranch(AwakeningBranch branch)`, `AllNodes`)

- [ ] **Step 1: Write the failing test for Awakening models and database**

```csharp
// In Tests/AwakeningSystemTests.cs
using System;
using System.Linq;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class AwakeningSystemTests
    {
        [Fact]
        public void AwakeningDatabase_Contains_All_Seven_Nodes()
        {
            var nodes = AwakeningDatabase.AllNodes;
            Assert.Equal(7, nodes.Count);

            var warNodes = AwakeningDatabase.GetNodesForBranch(AwakeningBranch.War);
            var momentumNodes = AwakeningDatabase.GetNodesForBranch(AwakeningBranch.Momentum);
            var heritageNodes = AwakeningDatabase.GetNodesForBranch(AwakeningBranch.Heritage);

            Assert.Equal(3, warNodes.Count);
            Assert.Equal(2, momentumNodes.Count);
            Assert.Equal(2, heritageNodes.Count);
        }

        [Theory]
        [InlineData("War_PrimordialMight", 1, 1)]
        [InlineData("War_PrimordialMight", 5, 5)]
        [InlineData("Flow_WaveLeap", 1, 2)]
        [InlineData("Flow_WaveLeap", 6, 12)]
        [InlineData("Heritage_BloodRecall", 1, 1)]
        [InlineData("Heritage_BloodRecall", 5, 8)]
        public void AwakeningNodes_Calculate_Correct_Costs(string id, int level, int expectedCost)
        {
            var node = AwakeningDatabase.GetNode(id);
            Assert.NotNull(node);
            Assert.Equal(expectedCost, node.GetCost(level));
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~AwakeningSystemTests" -v minimal`  
Expected: FAIL (types do not exist yet).

- [ ] **Step 3: Implement `AwakeningModels.cs`**

```csharp
// In Scripts/Core/AwakeningModels.cs
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
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~AwakeningSystemTests" -v minimal`  
Expected: PASS (2 passed).

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/AwakeningModels.cs Tests/AwakeningSystemTests.cs
git commit -m "feat(awakening): add AwakeningModels and AwakeningDatabase with definitions"
```

---

### Task 2: Uyanış Yöneticisi (`AwakeningManager.cs`) ve Formül Hesaplaması

**Files:**
- Create: `Scripts/Core/AwakeningManager.cs`
- Modify: `Tests/AwakeningSystemTests.cs`

**Interfaces:**
- Consumes: `AwakeningDatabase`, `AwakeningNodeDefinition`
- Produces: `AwakeningManager.Instance`, `CalculatePendingPoints(int wave)`, `CanUpgradeSeal(id)`, `TryUpgradeSeal(id)`, `GetDamageMultiplier()`, `GetMaxHpMultiplier()`, `GetBonusLifesteal()`, `GetWaveLeapChance()`, `GetRageGainMultiplier()`, `GetStartingGold()`, `GetPointsMultiplier()`

- [ ] **Step 1: Write the failing tests for Awakening points formula and seal upgrades**

```csharp
// Append to Tests/AwakeningSystemTests.cs
[Theory]
[InlineData(19, 0)]
[InlineData(20, 1)]
[InlineData(25, 2)]
[InlineData(30, 3)]
[InlineData(40, 6)]
[InlineData(50, 10)]
[InlineData(75, 21)]
[InlineData(100, 36)]
public void CalculatePendingPoints_Matches_Formula_Table(int wave, int expectedPoints)
{
    var manager = new AwakeningManager();
    Assert.Equal(expectedPoints, manager.CalculatePendingPoints(wave));
}

[Fact]
public void Upgrading_Seal_Deducts_AP_And_Provides_Multipliers()
{
    var manager = new AwakeningManager();
    manager.AddAwakeningPoints(10);
    Assert.Equal(10, manager.AwakeningPoints);

    Assert.True(manager.CanUpgradeSeal("War_PrimordialMight"));
    Assert.True(manager.TryUpgradeSeal("War_PrimordialMight")); // costs 1 AP
    Assert.Equal(9, manager.AwakeningPoints);
    Assert.Equal(1, manager.GetSealLevel("War_PrimordialMight"));
    Assert.Equal(1.15f, manager.GetDamageMultiplier(), 0.001f);

    Assert.True(manager.TryUpgradeSeal("War_PrimordialMight")); // costs 2 AP
    Assert.Equal(7, manager.AwakeningPoints);
    Assert.Equal(2, manager.GetSealLevel("War_PrimordialMight"));
    Assert.Equal(1.30f, manager.GetDamageMultiplier(), 0.001f);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~AwakeningSystemTests" -v minimal`  
Expected: FAIL (`AwakeningManager` not found).

- [ ] **Step 3: Implement `AwakeningManager.cs`**

```csharp
// In Scripts/Core/AwakeningManager.cs
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
```

- [ ] **Step 4: Run tests to verify all pass**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~AwakeningSystemTests" -v minimal`  
Expected: PASS (4 passed).

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/AwakeningManager.cs Tests/AwakeningSystemTests.cs
git commit -m "feat(awakening): implement AwakeningManager points formula, upgrades and multipliers"
```

---

### Task 3: Savaş, İlerleme ve GameManager Entegrasyonu

**Files:**
- Modify: `Scripts/Combat/PentagramStats.cs`
- Modify: `Scripts/Combat/TapCombatArea.cs`
- Modify: `Scripts/Combat/WaveSpawner.cs`
- Modify: `Scripts/Core/GameManager.cs`
- Test: `Tests/AwakeningSystemTests.cs`

**Interfaces:**
- Consumes: `AwakeningManager.Instance`, `AwakeningDatabase`
- Modifies: `PentagramStats.ResetToDefaults()`, `GameManager.ResetForAwakening()`

- [ ] **Step 1: Write tests for PentagramStats reset and multipliers**

```csharp
// Append to Tests/AwakeningSystemTests.cs
[Fact]
public void PentagramStats_Incorporates_Awakening_Damage_And_Hp()
{
    var manager = new AwakeningManager();
    AwakeningManager.SetInstance(manager);
    manager.AddAwakeningPoints(5);
    manager.TryUpgradeSeal("War_PrimordialMight"); // lvl 1 -> +15%
    manager.TryUpgradeSeal("War_BloodAegis");       // lvl 1 -> +15%
    manager.TryUpgradeSeal("War_VampiricThirst");   // lvl 1 -> +0.5%

    var stats = new PentagramStats();
    Assert.True(stats.Atk > 10.0f);
    Assert.True(stats.MaxHp > 100.0f);
    Assert.True(stats.LifestealPercent >= 1.5f);

    stats.AtkLevel = 10;
    stats.ResetToDefaults();
    Assert.Equal(1, stats.AtkLevel);
    Assert.Equal(1, stats.MaxHpLevel);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~AwakeningSystemTests" -v minimal`  
Expected: FAIL (`ResetToDefaults` not implemented or multipliers not applied).

- [ ] **Step 3: Update `Scripts/Combat/PentagramStats.cs`**

Add `ResetToDefaults()` and inject `AwakeningManager` multipliers into `Atk`, `MaxHp`, `LifestealPercent`:
```csharp
// In Scripts/Combat/PentagramStats.cs
public void ResetToDefaults()
{
    AtkLevel = 1;
    AtkSpeedLevel = 1;
    LifestealLevel = 1;
    MaxHpLevel = 1;
    RangeLevel = 1;
}
```
In calculated properties:
- `Atk`: katla `*(AwakeningManager.Instance != null ? AwakeningManager.Instance.GetDamageMultiplier() : 1.0f)`
- `MaxHp`: katla `*(AwakeningManager.Instance != null ? AwakeningManager.Instance.GetMaxHpMultiplier() : 1.0f)`
- `LifestealPercent`: ekle `+(AwakeningManager.Instance != null ? AwakeningManager.Instance.GetBonusLifesteal() : 0.0f)`

- [ ] **Step 4: Update `Scripts/Combat/TapCombatArea.cs` and `Hero.cs` for Rage multiplier**

In `TapCombatArea.cs` (when adding rage):
```csharp
float rageMult = AwakeningManager.Instance?.GetRageGainMultiplier() ?? 1.0f;
GameManager.Instance.AddRage(1.5f * rageMult);
```
In `Hero.cs` (when hero attacks):
```csharp
float rageMult = AwakeningManager.Instance?.GetRageGainMultiplier() ?? 1.0f;
GameManager.Instance?.AddRage(0.5f * rageMult);
```

- [ ] **Step 5: Update `Scripts/Combat/WaveSpawner.cs` for Wave Leap**

When wave completes (and wave % 10 != 0, so not a boss wave):
```csharp
float leapChance = AwakeningManager.Instance?.GetWaveLeapChance() ?? 0.0f;
if (leapChance > 0f && GD.Randf() < leapChance)
{
    // Wave Leap triggered!
    GameManager.Instance?.AdvanceWave();
}
```

- [ ] **Step 6: Update `Scripts/Core/GameManager.cs`**

Add `ResetForAwakening()` keeping the file under 250 lines:
```csharp
public void ResetForAwakening()
{
    Stats.ResetToDefaults();
    CurrentWave = 1;
    IsInSafeFarmMode = false;
    double startingBonus = AwakeningManager.Instance?.GetStartingGold() ?? 0.0;
    Gold = 100.0 + startingBonus;
    RagePercentage = 0f;
    IsRageActive = false;
    OnGoldChanged?.Invoke(Gold);
    OnWaveChanged?.Invoke(CurrentWave, false);
    OnRageChanged?.Invoke(0f);
    OnRageStateChanged?.Invoke(false);
    OnStatsUpgraded?.Invoke();
    SaveGame();
}
```

- [ ] **Step 7: Verify with tests and build**

Run: `dotnet test Tests/BloodSeal.Tests.csproj -v minimal`  
Run: `dotnet build`  
Expected: PASS with 0 warnings, 0 errors.

- [ ] **Step 8: Commit**

```bash
git add Scripts/Combat/PentagramStats.cs Scripts/Combat/TapCombatArea.cs Scripts/Combat/Hero.cs Scripts/Combat/WaveSpawner.cs Scripts/Core/GameManager.cs Tests/AwakeningSystemTests.cs
git commit -m "feat(combat): integrate awakening multipliers, rage gain, wave leap and prestige reset"
```

---

### Task 4: Kayıt Sistemi Entegrasyonu (`SaveData.cs` & `SaveSystem.cs`)

**Files:**
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Core/SaveSystem.cs`
- Modify: `Tests/SaveSystemTests.cs`

**Interfaces:**
- Produces: `SaveData.AwakeningPoints`, `SaveData.TotalAwakenings`, `SaveData.AwakeningLevels` persisted and reloaded into `AwakeningManager.Instance`

- [ ] **Step 1: Write test for saving and restoring Awakening data**

```csharp
// Append to Tests/SaveSystemTests.cs
[Fact]
public void SaveSystem_Persists_And_Restores_Awakening_Data()
{
    var manager = new AwakeningManager();
    AwakeningManager.SetInstance(manager);
    manager.AddAwakeningPoints(42);
    manager.SetSealLevel("War_PrimordialMight", 3);
    manager.SetSealLevel("Flow_WaveLeap", 2);

    var data = SaveSystem.CaptureSaveData(new GameManager());
    Assert.Equal(42, data.AwakeningPoints);
    Assert.Equal(3, data.AwakeningLevels["War_PrimordialMight"]);
    Assert.Equal(2, data.AwakeningLevels["Flow_WaveLeap"]);

    // Clear manager and restore
    manager.Reset();
    Assert.Equal(0, manager.AwakeningPoints);

    SaveSystem.ApplySaveData(data, new GameManager());
    Assert.Equal(42, manager.AwakeningPoints);
    Assert.Equal(3, manager.GetSealLevel("War_PrimordialMight"));
    Assert.Equal(2, manager.GetSealLevel("Flow_WaveLeap"));
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~SaveSystem_Persists_And_Restores_Awakening_Data" -v minimal`  
Expected: FAIL (fields missing on `SaveData`).

- [ ] **Step 3: Update `Scripts/Core/SaveData.cs`**

Add:
```csharp
// Awakening / Rebirth Progression (GDD Section 4.5)
public int AwakeningPoints { get; set; } = 0;
public int TotalAwakenings { get; set; } = 0;
public System.Collections.Generic.Dictionary<string, int> AwakeningLevels { get; set; } = new();
```

- [ ] **Step 4: Update `Scripts/Core/SaveSystem.cs`**

In `CaptureSaveData`:
```csharp
if (AwakeningManager.Instance != null)
{
    data.AwakeningPoints = AwakeningManager.Instance.AwakeningPoints;
    data.TotalAwakenings = AwakeningManager.Instance.TotalAwakenings;
    data.AwakeningLevels = AwakeningManager.Instance.GetAllLevels();
}
```
In `ApplySaveData`:
```csharp
if (AwakeningManager.Instance != null)
{
    AwakeningManager.Instance.Reset();
    AwakeningManager.Instance.AddAwakeningPoints(data.AwakeningPoints);
    if (data.AwakeningLevels != null)
    {
        foreach (var kvp in data.AwakeningLevels)
        {
            AwakeningManager.Instance.SetSealLevel(kvp.Key, kvp.Value);
        }
    }
}
```

- [ ] **Step 5: Run tests to verify all pass**

Run: `dotnet test Tests/BloodSeal.Tests.csproj -v minimal`  
Expected: ALL PASS.

- [ ] **Step 6: Commit**

```bash
git add Scripts/Core/SaveData.cs Scripts/Core/SaveSystem.cs Tests/SaveSystemTests.cs
git commit -m "feat(save): add AwakeningPoints, TotalAwakenings and seal levels persistence"
```

---

### Task 5: Gotik `AwakeningModal.cs` ve `MainHUD.cs` Entegrasyonu

**Files:**
- Create: `Scripts/UI/AwakeningModal.cs`
- Modify: `Scripts/UI/MainHUD.cs`
- Modify: `Scenes/Main.tscn`

**Interfaces:**
- Consumes: `AwakeningManager.Instance`, `GameManager.Instance`
- Produces: `AwakeningModal.ShowModal()`, HUD `AwakeningBtn` click trigger

- [ ] **Step 1: Implement `Scripts/UI/AwakeningModal.cs`**

Two-panel Gothic dark UI:
- Left panel: Uyanış Ritüeli (Current wave, Pending AP calculation, soft reset warning, confirm button).
- Right panel: Kadim Mühürler Ağacı (War, Momentum, Heritage branch tabs, upgrade cards with AP costs).
- Respects 250-line limit by clean code organization.

- [ ] **Step 2: Update `Scripts/UI/MainHUD.cs`**

Wire `AwakeningBtn` and `AwakeningModal`:
- Show/Highlight `AwakeningBtn` when `CurrentWave >= 20` or `AwakeningPoints > 0`.
- Connect button to `AwakeningModal.ShowModal()`.

- [ ] **Step 3: Update `Scenes/Main.tscn` or instantiate modal programmatically in HUD**

Ensure `AwakeningModal` is mounted in CanvasLayer and exported/instantiated safely.

- [ ] **Step 4: Build and test**

Run: `dotnet build`  
Expected: 0 warnings, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add Scripts/UI/AwakeningModal.cs Scripts/UI/MainHUD.cs Scenes/Main.tscn
git commit -m "feat(ui): implement Gothic AwakeningModal and integrate HUD Awakening access"
```

---

### Task 6: Doğrulama Kapısı ve Headless Smoke Test

**Files:**
- Run automated commands across entire codebase

- [ ] **Step 1: Run full unit test suite**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --logger "console;verbosity=detailed"`  
Expected: 100% passed, 0 failures.

- [ ] **Step 2: Run dotnet build verification**

Run: `dotnet build`  
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Run Godot headless smoke test**

Run: `C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 60`  
Expected: Exit code 0, no unhandled exceptions.

- [ ] **Step 4: Final commit and status check**

```bash
git status
git log -n 5 --oneline
```
