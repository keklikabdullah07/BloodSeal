# Yoldaş (Familiar) Sinerji, Seviye & Yetenek Sistemi Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Kahramanın yanındaki tekil yardımcı peti, 4 farklı Gotik yoldaşa (Kan Kargası, Gölge Yarasası, Kan Tazısı, Gece Heykeli/Gargoyle), seviye ve tier atlama ekonomisine ($Cost = BaseCost \times 1.15^{(Level-1)}$ ve 10. seviyelerde Kan Parşömeni), taktiksel auralara ve Gotik bir seçim/geliştirme arayüzüne (`FamiliarModal`) sahip tam teşekküllü bir sisteme dönüştürmek.

**Architecture:** Saf C# singleton servisi (`FamiliarManager`), veri modelleri (`FamiliarModels.cs`, `FamiliarDatabase.cs`), xUnit testleri (`FamiliarSystemTests.cs`), nesne havuzlu dinamik savaş görseli (`PetCompanion.cs`), bağımsız CanvasLayer katmanı (`FamiliarController.cs`, `FamiliarModal.cs`) ve kalıcı JSON serileştirmesi (`SaveData.cs`).

**Tech Stack:** C# 12 / .NET 10.0, Godot 4.7.x Mono, xUnit.

## Global Constraints
- Sınıf satır sınırı: Hiçbir sınıf 250 satırı aşamaz (`MainHUD.cs` ve `GameManager.cs` 248 satırın altında tutulacaktır).
- `Engine.TimeScale` değiştirilemez.
- Para ve maliyet hesaplamalarında `double` kullanılacaktır.
- "Call down, signal up": `FamiliarManager` C# event'leri yayar, UI ve savaş düğümleri dinler.

---

### Task 1: Veri Modelleri ve Veritabanı (`FamiliarModels.cs`, `FamiliarDatabase.cs`)

**Files:**
- Create: `Scripts/Core/FamiliarModels.cs`
- Create: `Scripts/Core/FamiliarDatabase.cs`

**Interfaces:**
- Produces: `enum FamiliarType`, `class FamiliarDefinition`, `class FamiliarProgress`, `class FamiliarDatabase`.

- [ ] **Step 1: `Scripts/Core/FamiliarModels.cs` dosyasını oluştur**
```csharp
using Godot;

namespace BloodSeal.Core
{
    public enum FamiliarType
    {
        BloodRaven,
        ShadowBat,
        CrimsonHound,
        StoneGargoyle
    }

    public class FamiliarDefinition
    {
        public FamiliarType Type { get; set; }
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string PassiveDescription { get; set; } = "";
        public string IconPath { get; set; } = "";
        public Color AuraColor { get; set; } = Colors.Red;
        public float BaseAttackMultiplier { get; set; } = 0.4f;
        public float BaseAttackInterval { get; set; } = 1.2f;
        public double BaseUpgradeCost { get; set; } = 100.0;
        public int UnlockWaveRequirement { get; set; } = 1;
    }

    public class FamiliarProgress
    {
        public string Id { get; set; } = "";
        public int Level { get; set; } = 1;
        public bool IsUnlocked { get; set; } = false;

        public int Tier => (Level - 1) / 10 + 1;
        public bool IsMilestoneLevel => (Level % 10) == 0;
    }
}
```

- [ ] **Step 2: `Scripts/Core/FamiliarDatabase.cs` dosyasını oluştur**
```csharp
using System.Collections.Generic;
using Godot;

namespace BloodSeal.Core
{
    public static class FamiliarDatabase
    {
        private static readonly Dictionary<string, FamiliarDefinition> _familiars = new()
        {
            ["blood_raven"] = new FamiliarDefinition
            {
                Type = FamiliarType.BloodRaven,
                Id = "blood_raven",
                Name = "Kan Kargası",
                Title = "Kızıl Casus",
                Description = "Göklerin kan kokusunu takip eden kadim karga. Hızlı kan küreleri fırlatır.",
                PassiveDescription = "Seviye başına yoldaş hasarı +%10 artar. Her Tier'da Kahraman ATK Hızı +%3 artar.",
                IconPath = "res://Assets/Sprites/Pet/pet_blood_raven.png",
                AuraColor = new Color(0.9f, 0.1f, 0.25f, 0.9f),
                BaseAttackMultiplier = 0.40f,
                BaseAttackInterval = 1.20f,
                BaseUpgradeCost = 100.0,
                UnlockWaveRequirement = 1
            },
            ["shadow_bat"] = new FamiliarDefinition
            {
                Type = FamiliarType.ShadowBat,
                Id = "shadow_bat",
                Name = "Gölge Yarasası",
                Title = "Gece Avcısı",
                Description = "Karanlık mahzenlerin sinsi yarasası. Çift gölge küresi fırlatır.",
                PassiveDescription = "Kahramana +%5..%20 Kritik Şans ve +%15..%60 Kritik Hasar aurası bağışlar.",
                IconPath = "res://Assets/Sprites/Pet/pet_blood_raven.png",
                AuraColor = new Color(0.55f, 0.15f, 0.85f, 0.9f),
                BaseAttackMultiplier = 0.35f,
                BaseAttackInterval = 1.40f,
                BaseUpgradeCost = 500.0,
                UnlockWaveRequirement = 15
            },
            ["crimson_hound"] = new FamiliarDefinition
            {
                Type = FamiliarType.CrimsonHound,
                Id = "crimson_hound",
                Name = "Kan Tazısı",
                Title = "Kızıl Takipçi",
                Description = "Kan kokusuyla doymayan cehennem köpeği. Güçlü kan dalgaları fırlatır.",
                PassiveDescription = "Düşmanlardan düşen Altını +%15..%60 artırır; Boss savunmasını kırar.",
                IconPath = "res://Assets/Sprites/Pet/pet_blood_raven.png",
                AuraColor = new Color(0.85f, 0.05f, 0.05f, 0.9f),
                BaseAttackMultiplier = 0.55f,
                BaseAttackInterval = 1.60f,
                BaseUpgradeCost = 2500.0,
                UnlockWaveRequirement = 25
            },
            ["stone_gargoyle"] = new FamiliarDefinition
            {
                Type = FamiliarType.StoneGargoyle,
                Id = "stone_gargoyle",
                Name = "Gece Heykeli",
                Title = "Kadim Gargoyle",
                Description = "Katedral çatılarının taştan muhafızı. Ağır taş şoku ve alan sarsıntısı yaratır.",
                PassiveDescription = "Kahramanın aldığı hasarı %10..%35 azaltır (Zırh); Berserk dolumunu hızlandırır.",
                IconPath = "res://Assets/Sprites/Pet/pet_blood_raven.png",
                AuraColor = new Color(0.7f, 0.65f, 0.55f, 0.9f),
                BaseAttackMultiplier = 0.80f,
                BaseAttackInterval = 2.20f,
                BaseUpgradeCost = 10000.0,
                UnlockWaveRequirement = 40
            }
        };

        public static IEnumerable<FamiliarDefinition> GetAll() => _familiars.Values;

        public static FamiliarDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return _familiars["blood_raven"];
            return _familiars.TryGetValue(id, out var def) ? def : _familiars["blood_raven"];
        }
    }
}
```

- [ ] **Step 3: `dotnet build` çalıştırarak modelleri doğrula**
- [ ] **Step 4: Commit (`feat: add Familiar models and static database definitions`)**

---

### Task 2: `FamiliarManager` Servisi ve Ekonomi/Stat Formülleri (`FamiliarManager.cs`)

**Files:**
- Create: `Scripts/Core/FamiliarManager.cs`

**Interfaces:**
- Produces: `class FamiliarManager`, `GetGoldCost(id)`, `GetParchmentCost(id)`, `CanUpgrade(id)`, `Upgrade(id)`, `SetActiveFamiliar(id)`, `GetActivePetDamage(heroAtk)`, `GetCritChanceBonus()`, `GetCritDamageBonus()`, `GetGoldMultiplierBonus()`, `GetDamageReductionBonus()`.

- [ ] **Step 1: `Scripts/Core/FamiliarManager.cs` dosyasını oluştur (< 250 satır kuralına uygun)**
```csharp
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public class FamiliarManager
    {
        private static FamiliarManager _instance;
        public static FamiliarManager Instance => _instance ??= new FamiliarManager();

        public string ActiveFamiliarId { get; private set; } = "blood_raven";
        private readonly Dictionary<string, FamiliarProgress> _progresses = new();

        public event Action<string> OnActiveFamiliarChanged;
        public event Action<string, int> OnFamiliarUpgraded;
        public event Action<string> OnFamiliarUnlocked;

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
```

- [ ] **Step 2: `dotnet build` çalıştır**
- [ ] **Step 3: Commit (`feat: implement FamiliarManager singleton with upgrade economy and buffs`)**

---

### Task 3: Birim Testleri (`Tests/FamiliarSystemTests.cs`)

**Files:**
- Create: `Tests/FamiliarSystemTests.cs`

**Interfaces:**
- Tests all mathematical formulas, unlock triggers, upgrade costs, and buffs in pure C# without Godot engine dependencies.

- [ ] **Step 1: `Tests/FamiliarSystemTests.cs` test sınıfını oluştur**
```csharp
using System;
using BloodSeal.Core;
using Xunit;

namespace BloodSeal.Tests
{
    public class FamiliarSystemTests
    {
        [Fact]
        public void Database_ContainsAllFourGothicFamiliars()
        {
            var raven = FamiliarDatabase.Get("blood_raven");
            var bat = FamiliarDatabase.Get("shadow_bat");
            var hound = FamiliarDatabase.Get("crimson_hound");
            var gargoyle = FamiliarDatabase.Get("stone_gargoyle");

            Assert.NotNull(raven);
            Assert.NotNull(bat);
            Assert.NotNull(hound);
            Assert.NotNull(gargoyle);

            Assert.Equal(1, raven.UnlockWaveRequirement);
            Assert.Equal(15, bat.UnlockWaveRequirement);
            Assert.Equal(25, hound.UnlockWaveRequirement);
            Assert.Equal(40, gargoyle.UnlockWaveRequirement);
        }

        [Fact]
        public void UpgradeCost_FollowsExponentialFormula()
        {
            var manager = new FamiliarManager();
            FamiliarManager.SetInstanceForTesting(manager);

            double costLv1 = manager.GetGoldCost("blood_raven");
            Assert.Equal(100.0, costLv1);

            double gold = 100.0;
            int parch = 0;
            bool upgraded = manager.Upgrade("blood_raven", ref gold, ref parch);

            Assert.True(upgraded);
            Assert.Equal(0.0, gold);
            Assert.Equal(2, manager.GetProgress("blood_raven").Level);

            double costLv2 = manager.GetGoldCost("blood_raven");
            Assert.Equal(Math.Floor(100.0 * 1.15), costLv2);
        }

        [Fact]
        public void TierMilestone_RequiresParchment()
        {
            var manager = new FamiliarManager();
            FamiliarManager.SetInstanceForTesting(manager);

            var prog = manager.GetProgress("blood_raven");
            prog.Level = 9; // 9 -> 10 requires Tier 1 milestone parchment (5 parchments)

            Assert.Equal(5, manager.GetParchmentCost("blood_raven"));

            double gold = 10000.0;
            int parch = 2; // insufficient
            Assert.False(manager.CanUpgrade("blood_raven", gold, parch));

            parch = 5; // sufficient
            Assert.True(manager.CanUpgrade("blood_raven", gold, parch));
            Assert.True(manager.Upgrade("blood_raven", ref gold, ref parch));
            Assert.Equal(10, prog.Level);
            Assert.Equal(0, parch);
        }

        [Fact]
        public void WaveProgression_UnlocksFamiliarsCorrectly()
        {
            var manager = new FamiliarManager();
            FamiliarManager.SetInstanceForTesting(manager);

            Assert.True(manager.GetProgress("blood_raven").IsUnlocked);
            Assert.False(manager.GetProgress("shadow_bat").IsUnlocked);

            manager.CheckWaveUnlocks(14);
            Assert.False(manager.GetProgress("shadow_bat").IsUnlocked);

            manager.CheckWaveUnlocks(15);
            Assert.True(manager.GetProgress("shadow_bat").IsUnlocked);

            manager.CheckWaveUnlocks(40);
            Assert.True(manager.GetProgress("stone_gargoyle").IsUnlocked);
        }

        [Fact]
        public void ActiveFamiliarBuffs_ApplyExpectedMultipliers()
        {
            var manager = new FamiliarManager();
            FamiliarManager.SetInstanceForTesting(manager);

            // Default raven
            Assert.Equal(0f, manager.GetCritChanceBonus());
            Assert.Equal(0f, manager.GetDamageReductionBonus());

            // Unlock and switch to bat
            manager.CheckWaveUnlocks(15);
            Assert.True(manager.SetActiveFamiliar("shadow_bat"));
            Assert.True(manager.GetCritChanceBonus() > 0.05f);

            // Unlock and switch to gargoyle
            manager.CheckWaveUnlocks(40);
            Assert.True(manager.SetActiveFamiliar("stone_gargoyle"));
            Assert.True(manager.GetDamageReductionBonus() >= 0.10f);
        }
    }
}
```

- [ ] **Step 2: `dotnet test` çalıştırarak birim testleri doğrula**
- [ ] **Step 3: Commit (`test: add xUnit tests for FamiliarManager database, formulas and unlocks`)**

---

### Task 4: Kalıcılık & Save Entegrasyonu (`SaveData.cs`, `SaveSystem.cs`)

**Files:**
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Core/SaveSystem.cs`

**Interfaces:**
- Serileştirme: `SaveData.ActiveFamiliarId`, `SaveData.FamiliarProgresses`.
- Yükleme: Oyun başladığında `FamiliarManager.Instance.LoadProgresses` çağrılır.

- [ ] **Step 1: `Scripts/Core/SaveData.cs` dosyasına yoldaş alanlarını ekle**
- [ ] **Step 2: `Scripts/Core/SaveSystem.cs` içine yoldaş durumunu kaydetme/yükleme mantığını ekle (250 satır sınırına dikkat ederek)**
- [ ] **Step 3: `dotnet build` ve `dotnet test` ile doğrula**
- [ ] **Step 4: Commit (`feat: persist active familiar and progress across saves and rebirths`)**

---

### Task 5: Savaş & Görsel Entegrasyon (`PetCompanion.cs`, `Hero.cs`, `GameManager.cs`)

**Files:**
- Modify: `Scripts/Combat/PetCompanion.cs`
- Modify: `Scripts/Combat/Hero.cs`
- Modify: `Scripts/Core/GameManager.cs`

**Interfaces:**
- `PetCompanion`: `OnActiveFamiliarChanged` dinler; sprite, parçacık rengi (`AuraParticles`), mermi hasarı ve atış frekansı dinamik güncellenir.
- `Hero`: Hasar alırken `FamiliarManager.Instance.GetDamageReductionBonus()` uygular; kritik vuruşta `GetCritChanceBonus()` ve `GetCritDamageBonus()` uygular.
- `GameManager`: Altın ödülü verirken `GetGoldMultiplierBonus()` uygular; dalga atlandığında `FamiliarManager.Instance.CheckWaveUnlocks(wave)` tetikler.

- [ ] **Step 1: `Scripts/Combat/PetCompanion.cs` dosyasını `FamiliarManager` ile dinamik hale getir (< 200 satır)**
- [ ] **Step 2: `Hero.cs` ve `GameManager.cs` içine aura çarpanlarını bağla (her dosyanın satır sayısının < 250 kaldığını doğrula)**
- [ ] **Step 3: `dotnet build` çalıştır**
- [ ] **Step 4: Commit (`feat: integrate PetCompanion combat shooting, hero buffs and gold aura`)**

---

### Task 6: Gotik UI Modalı ve Bağımsız Katman (`FamiliarModal.cs`, `FamiliarController.cs`)

**Files:**
- Create: `Scripts/UI/FamiliarModal.cs`
- Create: `Scripts/UI/FamiliarController.cs`
- Modify: `Scenes/MainCombat.tscn`

**Interfaces:**
- `FamiliarController`: CanvasLayer 103 üzerinde HUD sağ üst barında `🦇 YOLDAŞ` butonu barındırır.
- `FamiliarModal`: 4 yoldaş kartı, seviye yükseltme, kuşanma, istatistik önizlemeleri ve kapatma butonu barındırır.

- [ ] **Step 1: `Scripts/UI/FamiliarModal.cs` sınıfını oluştur (< 250 satır)**
- [ ] **Step 2: `Scripts/UI/FamiliarController.cs` sınıfını oluştur (< 120 satır)**
- [ ] **Step 3: `MainCombat.tscn` sahnesine `FamiliarController` CanvasLayer düğümünü ekle**
- [ ] **Step 4: `dotnet build` çalıştır**
- [ ] **Step 5: Commit (`feat: create gothic FamiliarModal and independent FamiliarController`)**

---

### Task 7: Tam Doğrulama ve Entegrasyon Kapısı (Verification Gate)

- [ ] **Step 1: Derleme Doğrulaması:** `dotnet build` (0 hata, 0 uyarı).
- [ ] **Step 2: Birim Testleri:** `dotnet test` (Tüm xUnit testleri 100% başarılı).
- [ ] **Step 3: Headless Smoke Test:** `Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 60` ile sahne çökmesi veya eksik kaynak olmadığını doğrula.
- [ ] **Step 4: Satır Sayısı Denetimi:** `MainHUD.cs`, `GameManager.cs`, `FamiliarModal.cs`, `SaveSystem.cs` tüm dosyaların < 250 satır olduğunu teyit et.
- [ ] **Step 5: HANDOVER.md güncellemesi ve final commit.**
