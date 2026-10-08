# 🩸 BloodSeal: Lore Eserleri & Malikane Mahzeni Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** GDD Bölüm 2 doğrultusunda her 10 dalgada bir kesilen Boss'lardan düşen 10 adet Gotik Lore Eserini (Darius'un Kanlı Mührü, Yırtık Aile Portresi vb.), kalıcı pasif bonuslarını, bağımsız `RelicManager` Core servisini, kalıcı kayıt sistemini ve Gotik `RelicVaultModal` vitrin arayüzünü inşa etmek.

**Architecture:** `ResearchManager` ve `AwakeningManager` deseniyle tam uyumlu bağımsız `RelicManager` Singleton servisi; 10 eserin sabit tanımlarını ve metinlerini içeren `RelicModels.cs` & `RelicDatabase`; `BossEnemy.cs` ilk zafer tetiklemesi; `PentagramStats.cs` ve `GameManager.cs` içine dinamik bonus enjeksiyonu; ve vitrin tarzı iki panelli Gotik `RelicVaultModal`.

**Tech Stack:** C# 12 / .NET 10.0 (`net10.0`), Godot 4.7.x Mono, xUnit test paketi (`Tests/BloodSeal.Tests.csproj`).

## Global Constraints
- **AGENTS.md Satır Sınırı:** Hiçbir C# dosyası 250 satırı aşamaz.
- **TimeScale Yasağı:** `Engine.TimeScale` asla değiştirilemez.
- **Sayı Hijyeni:** Para birimlerinde ve hasar çarpanlarında `double` ve `float` kullanılır.
- **Kalıcı İlerleme:** Lore eserleri Uyanış (Rebirth) sırasında ASLA sıfırlanmaz.
- **0 Uyarı 0 Hata:** `dotnet build` ve `dotnet test` her görev sonunda 0 hata ile geçmelidir.

---

### Task 1: Lore Eser Tanımları ve Veritabanı (`RelicModels.cs`)

**Files:**
- Create: `Scripts/Core/RelicModels.cs`
- Create: `Tests/RelicSystemTests.cs`
- Modify: `Tests/BloodSeal.Tests.csproj`

**Interfaces:**
- Produces: `enum RelicStatType`, `class RelicDefinition`, `class RelicDatabase` (`AllRelics`, `GetRelic(string id)`, `GetRelicForWave(int wave)`)

- [ ] **Step 1: Write the failing test for Relic models and database**

```csharp
// In Tests/RelicSystemTests.cs
using System;
using System.Linq;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class RelicSystemTests
    {
        [Fact]
        public void RelicDatabase_Contains_All_Ten_Relics()
        {
            var relics = RelicDatabase.AllRelics;
            Assert.Equal(10, relics.Count);

            for (int wave = 10; wave <= 100; wave += 10)
            {
                var relic = RelicDatabase.GetRelicForWave(wave);
                Assert.NotNull(relic);
                Assert.Equal(wave, relic.MilestoneWave);
                Assert.False(string.IsNullOrWhiteSpace(relic.Name));
                Assert.False(string.IsNullOrWhiteSpace(relic.LoreText));
            }
        }

        [Theory]
        [InlineData(10, "Relic_DariusRing", RelicStatType.Damage, 0.05f)]
        [InlineData(20, "Relic_TornPortrait", RelicStatType.MaxHp, 0.05f)]
        [InlineData(30, "Relic_CovenantMedallion", RelicStatType.Gold, 0.05f)]
        [InlineData(70, "Relic_BoneChalice", RelicStatType.Lifesteal, 0.5f)]
        [InlineData(100, "Relic_FirstScroll", RelicStatType.AllStats, 0.15f)]
        public void RelicDatabase_Maps_Wave_And_Bonuses_Accurately(int wave, string expectedId, RelicStatType expectedType, float expectedVal)
        {
            var relic = RelicDatabase.GetRelicForWave(wave);
            Assert.NotNull(relic);
            Assert.Equal(expectedId, relic.Id);
            Assert.Equal(expectedType, relic.StatType);
            Assert.Equal(expectedVal, relic.BonusValue, 0.001f);
        }
    }
}
```

- [ ] **Step 2: Update `Tests/BloodSeal.Tests.csproj` to link `RelicModels.cs` and `RelicManager.cs`**

```xml
<Compile Include="..\Scripts\Core\RelicModels.cs" Link="Core\RelicModels.cs" />
<Compile Include="..\Scripts\Core\RelicManager.cs" Link="Core\RelicManager.cs" />
```

- [ ] **Step 3: Implement `Scripts/Core/RelicModels.cs`**

```csharp
// In Scripts/Core/RelicModels.cs
#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public enum RelicStatType
    {
        Damage = 0,
        MaxHp = 1,
        Gold = 2,
        RageGain = 3,
        TapDamage = 4,
        OfflineIncome = 5,
        Lifesteal = 6,
        AttackSpeed = 7,
        AwakeningPoints = 8,
        AllStats = 9
    }

    public class RelicDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MilestoneWave { get; set; }
        public string IconSymbol { get; set; } = "💍";
        public string LoreText { get; set; } = string.Empty;
        public RelicStatType StatType { get; set; }
        public float BonusValue { get; set; }
        public string BonusDisplay => StatType switch
        {
            RelicStatType.Damage => $"+{BonusValue * 100f:F0}% Kahraman Hasarı",
            RelicStatType.MaxHp => $"+{BonusValue * 100f:F0}% Maksimum Can",
            RelicStatType.Gold => $"+{BonusValue * 100f:F0}% Altın Kazanımı",
            RelicStatType.RageGain => $"+{BonusValue * 100f:F0}% Öfke Dolum Hızı",
            RelicStatType.TapDamage => $"+{BonusValue * 100f:F0}% Tıklama Hasarı",
            RelicStatType.OfflineIncome => $"+{BonusValue * 100f:F0}% Çevrimdışı Gelir",
            RelicStatType.Lifesteal => $"+{BonusValue:F1}% Taban Can Çalma",
            RelicStatType.AttackSpeed => $"+{BonusValue * 100f:F0}% Saldırı Hızı",
            RelicStatType.AwakeningPoints => $"+{BonusValue * 100f:F0}% Uyanış Puanı Çarpanı",
            RelicStatType.AllStats => $"+{BonusValue * 100f:F0}% Tüm İstatistikler Çarpanı",
            _ => "+0%"
        };
    }

    public static class RelicDatabase
    {
        private static readonly Dictionary<string, RelicDefinition> RelicsById = new();
        private static readonly Dictionary<int, RelicDefinition> RelicsByWave = new();

        static RelicDatabase()
        {
            Register(new RelicDefinition
            {
                Id = "Relic_DariusRing",
                Name = "Darius'un Kanlı Mührü",
                MilestoneWave = 10,
                IconSymbol = "💍",
                LoreText = "\"Darius son nefesinde mührü avucuma bastırdığında kanı henüz sıcaktı. 'Ahit'i durdur,' dedi, 'küllerimiz onların sunağı olmasın.'\"",
                StatType = RelicStatType.Damage,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_TornPortrait",
                Name = "Yırtık Aile Portresi",
                MilestoneWave = 20,
                IconSymbol = "🖼️",
                LoreText = "\"Yüzleri jiletle kazınmış bir soylu ailesi. Altında soluk bir imza: 'Kan bağı asla çözülmez, sadece pıhtılaşır.'\"",
                StatType = RelicStatType.MaxHp,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_CovenantMedallion",
                Name = "Kızıl Ahit Madalyonu",
                MilestoneWave = 30,
                IconSymbol = "📿",
                LoreText = "\"Tarikatın yüksek rahiplerinin taktığı ters pentagram madalyon. Dokunduğunda parmak uçlarında açgözlü bir sızı bırakıyor.\"",
                StatType = RelicStatType.Gold,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_BlackenedBell",
                Name = "Kararmış Zangoç Çanı",
                MilestoneWave = 40,
                IconSymbol = "🔔",
                LoreText = "\"Varnath Katedrali'nin veba gecesinde çaldığı son çan. Sesi artık kulaklarda değil, doğrudan damarlarda yankılanıyor.\"",
                StatType = RelicStatType.RageGain,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_InquisitorMask",
                Name = "Engizisyon Maskesi",
                MilestoneWave = 50,
                IconSymbol = "🎭",
                LoreText = "\"Kuş gagası biçiminde dövülmüş demir maske. İç yüzeyinde kuruyan kan, takan kişinin kendi çığlıklarına ait.\"",
                StatType = RelicStatType.TapDamage,
                BonusValue = 0.10f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_CryptKey",
                Name = "Kadim Kripta Anahtarı",
                MilestoneWave = 60,
                IconSymbol = "🗝️",
                LoreText = "\"Malikane'nin unutulmuş alt mahzenlerini açan ağır pirinç anahtar. Zamanın bile unuttuğu hazinelerin bekçisi.\"",
                StatType = RelicStatType.OfflineIncome,
                BonusValue = 0.10f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_BoneChalice",
                Name = "Kemik Kadeh",
                MilestoneWave = 70,
                IconSymbol = "🍷",
                LoreText = "\"İlk mühür taşıyıcısının kaval kemiğinden oyulmuş kadeh. İçine dökülen her damla kan, içenin susuzluğunu ebediyen dindiriyor.\"",
                StatType = RelicStatType.Lifesteal,
                BonusValue = 0.5f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_KnightSpur",
                Name = "Kan Şövalyesi Mahmuzu",
                MilestoneWave = 80,
                IconSymbol = "⚔️",
                LoreText = "\"Kızıl orduların öncülerine ait paslanmış mahmuz. Savaş alanında durmak bilmeyen bir vahşetin hatırası.\"",
                StatType = RelicStatType.AttackSpeed,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_ExtinguishedLantern",
                Name = "Sönmüş Ruh Feneri",
                MilestoneWave = 90,
                IconSymbol = "🏮",
                LoreText = "\"İçinde bir zamanlar hapsolmuş yüzlerce gölge ruhunun fısıltıları olan fener. Ölüm ve yeniden doğuş arasındaki köprü.\"",
                StatType = RelicStatType.AwakeningPoints,
                BonusValue = 0.10f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_FirstScroll",
                Name = "Kökenin İlk Parşömeni",
                MilestoneWave = 100,
                IconSymbol = "📜",
                LoreText = "\"13 Mührün yaratıldığı gün yazılan ilk kutsal parşömen. Varnath'ın gerçek yaratılış sırrını fısıldıyor.\"",
                StatType = RelicStatType.AllStats,
                BonusValue = 0.15f
            });
        }

        private static void Register(RelicDefinition relic)
        {
            RelicsById[relic.Id] = relic;
            RelicsByWave[relic.MilestoneWave] = relic;
        }

        public static RelicDefinition? GetRelic(string id) => RelicsById.GetValueOrDefault(id);
        public static RelicDefinition? GetRelicForWave(int wave) => RelicsByWave.GetValueOrDefault(wave);
        public static IReadOnlyList<RelicDefinition> AllRelics => new List<RelicDefinition>(RelicsById.Values);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~RelicSystemTests" -v minimal`  
Expected: PASS (2 passed).

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/RelicModels.cs Tests/RelicSystemTests.cs Tests/BloodSeal.Tests.csproj
git commit -m "feat(relics): implement RelicModels and RelicDatabase definitions with tests"
```

---

### Task 2: Mahzen Yöneticisi (`RelicManager.cs`) ve Stat Bonusları

**Files:**
- Create: `Scripts/Core/RelicManager.cs`
- Modify: `Tests/RelicSystemTests.cs`

**Interfaces:**
- Consumes: `RelicDatabase`, `RelicDefinition`
- Produces: `RelicManager.Instance`, `UnlockRelicForWave(wave)`, `HasRelic(id)`, `GetDamageBonus()`, `GetHpBonus()`, `GetGoldBonus()`, `GetAttackSpeedBonus()`, `GetLifestealBonus()`, `GetRageGainBonus()`, `GetTapDamageBonus()`, `GetOfflineIncomeBonus()`, `GetAwakeningBonus()`, `event Action<RelicDefinition> OnRelicUnlocked`

- [ ] **Step 1: Write failing tests for RelicManager unlock logic and bonus accumulations**

```csharp
// Append to Tests/RelicSystemTests.cs
[Fact]
public void RelicManager_Unlocks_Relic_And_Calculates_Cumulative_Bonuses()
{
    var rm = new RelicManager();
    Assert.False(rm.HasRelic("Relic_DariusRing"));
    Assert.Equal(0, rm.GetCollectedCount());
    Assert.Equal(0f, rm.GetDamageBonus());

    bool unlocked = rm.UnlockRelicForWave(10);
    Assert.True(unlocked);
    Assert.True(rm.HasRelic("Relic_DariusRing"));
    Assert.Equal(1, rm.GetCollectedCount());
    Assert.Equal(0.05f, rm.GetDamageBonus(), 0.001f);

    // Duplicate unlock should be no-op
    Assert.False(rm.UnlockRelicForWave(10));
    Assert.Equal(1, rm.GetCollectedCount());

    // Wave 100 provides AllStats (+15%) which stacks with Damage (+5%)
    rm.UnlockRelicForWave(100);
    Assert.Equal(0.20f, rm.GetDamageBonus(), 0.001f);
    Assert.Equal(0.15f, rm.GetHpBonus(), 0.001f);
}
```

- [ ] **Step 2: Implement `Scripts/Core/RelicManager.cs`**

```csharp
// In Scripts/Core/RelicManager.cs
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
```

- [ ] **Step 3: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~RelicSystemTests" -v minimal`  
Expected: PASS (3 passed).

- [ ] **Step 4: Commit**

```bash
git add Scripts/Core/RelicManager.cs Tests/RelicSystemTests.cs
git commit -m "feat(relics): implement RelicManager collection and cumulative bonuses"
```

---

### Task 3: Boss Düşüşü, Pentagram ve Savaş Entegrasyonu

**Files:**
- Modify: `Scripts/Combat/BossEnemy.cs`
- Modify: `Scripts/Combat/PentagramStats.cs`
- Modify: `Scripts/Core/GameManager.cs`
- Modify: `Scripts/Combat/TapCombatArea.cs`
- Modify: `Scripts/Combat/Hero.cs`
- Modify: `Scripts/Core/OfflineProgressCalculator.cs`

**Interfaces:**
- Consumes: `RelicManager.Instance`, `RelicDatabase`
- Modifies: `BossEnemy.Die()`, `PentagramStats`, `GameManager.CalculateGoldReward`, `OfflineProgressCalculator.Calculate`

- [ ] **Step 1: Write integration tests for PentagramStats with Relic bonuses**

```csharp
// Append to Tests/RelicSystemTests.cs
[Fact]
public void PentagramStats_Incorporates_Relic_Damage_And_Hp_Bonuses()
{
    var rm = new RelicManager();
    RelicManager.SetInstance(rm);
    rm.UnlockRelicForWave(10); // +5% Damage
    rm.UnlockRelicForWave(20); // +5% HP
    rm.UnlockRelicForWave(70); // +0.5% Lifesteal
    rm.UnlockRelicForWave(80); // +5% AtkSpeed

    var stats = new BloodSeal.Combat.PentagramStats();
    Assert.True(stats.Atk > 10f);
    Assert.True(stats.MaxHp > 100f);
    Assert.True(stats.AtkSpeed > 1.0f);
    Assert.True(stats.LifestealPercent >= 1.5f);
}
```

- [ ] **Step 2: Update `Scripts/Combat/BossEnemy.cs`**

When milestone boss dies for the first time:
```csharp
bool isFirstDefeat = ResearchManager.Instance != null && !ResearchManager.Instance.HasDefeatedMilestoneBoss(_wave);
if (isFirstDefeat)
{
    ResearchManager.Instance.AddLoreScrolls(1);
    ResearchManager.Instance.RecordMilestoneBossDefeated(_wave);
    RelicManager.Instance?.UnlockRelicForWave(_wave);
}
```

- [ ] **Step 3: Update `Scripts/Combat/PentagramStats.cs`**

Include `RelicManager` bonuses in `Atk`, `MaxHp`, `AtkSpeed`, `LifestealPercent`:
```csharp
if (RelicManager.Instance != null)
{
    val *= (1.0f + RelicManager.Instance.GetDamageBonus());
}
```

- [ ] **Step 4: Update `Scripts/Core/GameManager.cs`**

In `CalculateGoldReward`:
```csharp
if (RelicManager.Instance != null) mult *= (1.0 + RelicManager.Instance.GetGoldBonus());
```

- [ ] **Step 5: Update `Scripts/Combat/TapCombatArea.cs` and `Hero.cs`**

In `TapCombatArea.cs`:
```csharp
tapDmg *= (1.0f + (RelicManager.Instance?.GetTapDamageBonus() ?? 0f));
float relicRage = RelicManager.Instance?.GetRageGainBonus() ?? 0f;
GameManager.Instance.AddRage(1.5f * rageMult * (1.0f + relicRage));
```
In `Hero.cs`:
```csharp
float relicRage = RelicManager.Instance?.GetRageGainBonus() ?? 0f;
GameManager.Instance.AddRage(2.0f * rageMult * (1.0f + relicRage));
```

- [ ] **Step 6: Update `Scripts/Core/OfflineProgressCalculator.cs`**

In `Calculate`:
```csharp
float relicBonus = RelicManager.Instance?.GetOfflineIncomeBonus() ?? 0f;
goldEarned *= (1.0 + relicBonus);
```

- [ ] **Step 7: Run tests and build**

Run: `dotnet test Tests/BloodSeal.Tests.csproj -v minimal`  
Run: `dotnet build`  
Expected: PASS with 0 warnings, 0 errors.

- [ ] **Step 8: Commit**

```bash
git add Scripts/Combat/BossEnemy.cs Scripts/Combat/PentagramStats.cs Scripts/Core/GameManager.cs Scripts/Combat/TapCombatArea.cs Scripts/Combat/Hero.cs Scripts/Core/OfflineProgressCalculator.cs Tests/RelicSystemTests.cs
git commit -m "feat(combat): integrate relic drops on boss defeat and stat multipliers into gameplay"
```

---

### Task 4: Kayıt Sistemi Entegrasyonu (`SaveData.cs` & `SaveSystem.cs`)

**Files:**
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Core/SaveSystem.cs`
- Modify: `Tests/SaveSystemTests.cs`

**Interfaces:**
- Produces: `SaveData.CollectedRelics` persisted and restored into `RelicManager.Instance`

- [ ] **Step 1: Write test for saving and restoring Relic data**

```csharp
// Append to Tests/SaveSystemTests.cs
[Fact]
public void SaveData_SerializesAndDeserializes_RelicFields()
{
    var data = new SaveData
    {
        CollectedRelics = new System.Collections.Generic.List<string>
        {
            "Relic_DariusRing",
            "Relic_TornPortrait"
        }
    };

    string json = JsonSerializer.Serialize(data);
    var deserialized = JsonSerializer.Deserialize<SaveData>(json);

    Assert.NotNull(deserialized);
    Assert.Contains("Relic_DariusRing", deserialized.CollectedRelics);
    Assert.Contains("Relic_TornPortrait", deserialized.CollectedRelics);
}
```

- [ ] **Step 2: Update `Scripts/Core/SaveData.cs`**

Add:
```csharp
// Lore Relics Progression (GDD Section 2)
public System.Collections.Generic.List<string> CollectedRelics { get; set; } = new();
```

- [ ] **Step 3: Update `Scripts/Core/SaveSystem.cs`**

In `CaptureSaveData`:
```csharp
var relm = RelicManager.Instance;
if (relm != null)
{
    data.CollectedRelics = relm.GetAllCollectedIds();
}
```
In `ApplySaveData`:
```csharp
var relm = RelicManager.Instance;
if (relm != null)
{
    relm.Reset();
    if (data.CollectedRelics != null)
    {
        foreach (string id in data.CollectedRelics)
            relm.UnlockRelic(id);
    }
}
```

- [ ] **Step 4: Run tests to verify all pass**

Run: `dotnet test Tests/BloodSeal.Tests.csproj -v minimal`  
Expected: ALL PASS.

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/SaveData.cs Scripts/Core/SaveSystem.cs Tests/SaveSystemTests.cs
git commit -m "feat(save): add CollectedRelics persistence to SaveData and SaveSystem"
```

---

### Task 5: Gotik `RelicVaultModal.cs` ve `MainHUD.cs` Entegrasyonu

**Files:**
- Create: `Scripts/UI/RelicVaultModal.cs`
- Create: `Scenes/UI/RelicVaultModal.tscn`
- Modify: `Scripts/UI/MainHUD.cs`
- Modify: `Scenes/UI/MainHUD.tscn`

**Interfaces:**
- Consumes: `RelicManager.Instance`, `RelicDatabase`
- Produces: `RelicVaultModal.ShowModal()`, HUD `RelicVaultBtn` click trigger

- [ ] **Step 1: Implement `Scripts/UI/RelicVaultModal.cs`**

Two-panel Gothic showcase UI:
- Left: 10 relic slots (clickable cards showing icon, name, unlock status).
- Right: Detailed inspection view for selected relic (big icon, full title, atmospheric lore fragment text, active bonus badge).
- Respects 250-line limit cleanly.

- [ ] **Step 2: Create `Scenes/UI/RelicVaultModal.tscn`**

Gothic panel styling with `DarkOverlay`, `CenterContainer`, `PanelContainer`, two-column body layout.

- [ ] **Step 3: Update `Scripts/UI/MainHUD.cs` and `Scenes/UI/MainHUD.tscn`**

- Wire `RelicVaultBtn` and `RelicVaultModal`.
- Show `RelicVaultBtn` when at least 1 relic is unlocked or milestone boss is defeated.

- [ ] **Step 4: Build and test**

Run: `dotnet build`  
Expected: 0 warnings, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add Scripts/UI/RelicVaultModal.cs Scenes/UI/RelicVaultModal.tscn Scripts/UI/MainHUD.cs Scenes/UI/MainHUD.tscn
git commit -m "feat(ui): implement Gothic RelicVaultModal and integrate HUD access"
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
Expected: Exit code 0.

- [ ] **Step 4: Final commit and status check**

```bash
git status
git log -n 5 --oneline
```
