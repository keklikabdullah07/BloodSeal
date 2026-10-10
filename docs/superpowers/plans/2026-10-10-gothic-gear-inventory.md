# Gotik Ekipman & Eşya Kuşanma (Gothic Gear & Inventory) Sistemi Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** BloodSeal projesine 4 kuşanılabilir slot (Silah, Zırh, Tılsım, Yüzük), 5 kademeli Gotik nadirlik derecesi, 16 özgün Gotik eşya tanımı, Boss infazlarında ganimet düşüşü, Demirhane bileme (+0..+10) ve parçalama ekonomisi ile tam teşekküllü 24 yuvalı bir Gotik Çanta/Envanter arayüzü (`InventoryModal`) kazandırmak.

**Architecture:** Saf C# domain servisi (`EquipmentManager`), veri modelleri (`EquipmentModels.cs`, `EquipmentDatabase.cs`), xUnit birim testleri (`EquipmentSystemTests.cs`), bağımsız CanvasLayer katmanı (`InventoryController.cs`, `InventoryModal.cs`) ve kalıcı JSON serileştirmesi (`SaveData.cs`).

**Tech Stack:** C# 12 / .NET 10.0, Godot 4.7.x Mono, xUnit.

## Global Constraints
- Sınıf satır sınırı: Hiçbir sınıf 250 satırı aşamaz (`MainHUD.cs` ve `GameManager.cs` 248 satırın altında tutulacaktır).
- `Engine.TimeScale` değiştirilemez.
- Para ve maliyet hesaplamalarında `double` kullanılacaktır.
- "Call down, signal up": `EquipmentManager` C# event'leri yayar, UI ve savaş düğümleri dinler.

---

### Task 1: Veri Modelleri ve Veritabanı (`EquipmentModels.cs`, `EquipmentDatabase.cs`)

**Files:**
- Create: `Scripts/Core/EquipmentModels.cs`
- Create: `Scripts/Core/EquipmentDatabase.cs`

**Interfaces:**
- Produces: `enum EquipmentSlot`, `enum EquipmentRarity`, `class EquipmentItem`, `class EquipmentDefinition`, `class EquipmentDatabase`.

- [ ] **Step 1: `Scripts/Core/EquipmentModels.cs` dosyasını oluştur (< 120 satır, 0 Godot bağımlılığı)**
```csharp
#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Amulet,
        Ring
    }

    public enum EquipmentRarity
    {
        Common,
        Rare,
        Epic,
        Legendary,
        AncientBlood
    }

    public class EquipmentDefinition
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public EquipmentSlot Slot { get; set; }
        public string IconSymbol { get; set; } = "⚔️";
        public string Lore { get; set; } = "";
        public float BasePrimaryValue { get; set; }
        public string PrimaryStatLabel { get; set; } = "ATK";
        public double BaseUpgradeCost { get; set; } = 150.0;
    }

    public class EquipmentItem
    {
        public string InstanceId { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string DefinitionId { get; set; } = "";
        public EquipmentSlot Slot { get; set; }
        public EquipmentRarity Rarity { get; set; } = EquipmentRarity.Common;
        public int Level { get; set; } = 0; // +0 .. +10
        public float BaseValue { get; set; }
        public Dictionary<string, float> SecondaryBonuses { get; set; } = new();

        public float FinalPrimaryValue => BaseValue * (1.0f + 0.10f * Level);

        public string GetRarityHex() => Rarity switch
        {
            EquipmentRarity.Common => "#B0B0B0",
            EquipmentRarity.Rare => "#3A86FF",
            EquipmentRarity.Epic => "#9D4EDD",
            EquipmentRarity.Legendary => "#FFB703",
            EquipmentRarity.AncientBlood => "#D00000",
            _ => "#FFFFFF"
        };
    }
}
```

- [ ] **Step 2: `Scripts/Core/EquipmentDatabase.cs` dosyasını oluştur (16 Gotik eşya tanımı, < 150 satır)**
- [ ] **Step 3: `dotnet build` çalıştırarak modelleri doğrula**
- [ ] **Step 4: Commit (`feat: add Equipment models and 16 gothic item definitions`)**

---

### Task 2: `EquipmentManager` Servisi ve Ekonomi/Stat Formülleri (`EquipmentManager.cs`)

**Files:**
- Create: `Scripts/Core/EquipmentManager.cs`

**Interfaces:**
- Produces: `class EquipmentManager`, `Equip(instanceId)`, `Unequip(slot)`, `UpgradeItem(instanceId, ref gold)`, `DismantleItem(instanceId, ref gold)`, `RollDrop(wave, isBoss)`, `GetTotalPrimaryBonus(slot)`, `GetSecondaryBonus(statName)`.

- [ ] **Step 1: `Scripts/Core/EquipmentManager.cs` dosyasını oluştur (< 220 satır)**
- [ ] **Step 2: `dotnet build` çalıştır**
- [ ] **Step 3: Commit (`feat: implement EquipmentManager with drops, upgrades and inventory state`)**

---

### Task 3: Birim Testleri (`Tests/EquipmentSystemTests.cs`)

**Files:**
- Create: `Tests/EquipmentSystemTests.cs`
- Modify: `Tests/BloodSeal.Tests.csproj`

**Interfaces:**
- Tests all equipment databases, drop distribution logic, equipping/unequipping, upgrade costs, salvage returns, and stat summation in pure C# with 0 engine dependencies.

- [ ] **Step 1: `Tests/BloodSeal.Tests.csproj` içine `EquipmentModels.cs`, `EquipmentDatabase.cs` ve `EquipmentManager.cs` ekle**
- [ ] **Step 2: `Tests/EquipmentSystemTests.cs` test sınıfını oluştur (en az 6 kapsamlı birim testi)**
- [ ] **Step 3: `dotnet test Tests/BloodSeal.Tests.csproj` çalıştır (tüm testler yeşil, 0 hata, 0 uyarı)**
- [ ] **Step 4: Commit (`test: add xUnit tests for EquipmentManager, drops, upgrade formulas and inventory`)**

---

### Task 4: Kalıcılık & Save Entegrasyonu (`SaveData.cs`, `SaveSystem.cs`)

**Files:**
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Core/SaveSystem.cs`

**Interfaces:**
- Serileştirme: `SaveData.EquippedItems`, `SaveData.BagItems`.
- Yükleme: `EquipmentManager.Instance.LoadState(data.EquippedItems, data.BagItems)`.
- Satır Sınırı: `SaveSystem.cs` < 250 satır kuralına kesinlikle uymalıdır.

- [ ] **Step 1: `SaveData.cs` içine envanter ve kuşanılmış eşya listelerini ekle**
- [ ] **Step 2: `SaveSystem.cs` içinde `CaptureSaveData` ve `ApplySaveData` metodlarına entegre et (< 248 satır)**
- [ ] **Step 3: `dotnet test Tests/BloodSeal.Tests.csproj` ile doğrula**
- [ ] **Step 4: Commit (`feat: persist equipped items and inventory bag across saves and rebirths`)**

---

### Task 5: Savaş, Boss Ganimet & Kahraman Stat Entegrasyonu (`BossEnemy.cs`, `Hero.cs`, `GameManager.cs`)

**Files:**
- Modify: `Scripts/Combat/BossEnemy.cs`
- Modify: `Scripts/Combat/Hero.cs`
- Modify: `Scripts/Core/GameManager.cs`

**Interfaces:**
- `BossEnemy`: Boss öldüğünde `EquipmentManager.Instance.RollDrop(wave, true)` çağrılır, yeni eşya çantaya eklenir veya yüzen metinle bildirilir.
- `Hero`:
  - `Hero.PerformAttack`: Kuşanılmış Silah ATK bonusunu ve ikincil statları uygular.
  - `Hero.TakeDamage`: Kuşanılmış Zırh hasar indirimini uygular.
  - `Hero.MaxHp`: Kuşanılmış Zırh Max HP bonusunu yansıtır.
- `GameManager`:
  - Altın ödülü verirken Kuşanılmış Yüzük `GoldMultiplier` bonusunu uygular.
- Satır Sınırı: `Hero.cs` ve `GameManager.cs` < 248 satır kalacaktır.

- [ ] **Step 1: `BossEnemy.cs` ölüm akışına garantili ganimet düşüşünü bağla**
- [ ] **Step 2: `Hero.cs` ve `GameManager.cs` içine ekipman bonuslarını ekle**
- [ ] **Step 3: `dotnet build` çalıştır**
- [ ] **Step 4: Commit (`feat: integrate boss gear drops and hero equipment stat bonuses`)**

---

### Task 6: Gotik Envanter Modalı ve Bağımsız Katman (`InventoryModal.cs`, `InventoryController.cs`, `MainCombat.tscn`)

**Files:**
- Create: `Scripts/UI/InventoryModal.cs`
- Create: `Scripts/UI/InventoryController.cs`
- Modify: `Scenes/MainCombat.tscn`

**Interfaces:**
- `InventoryController`: CanvasLayer 102 üzerinde `Position = Vector2(1430, 16)` konumunda `🛡️ ENVANTER` butonu barındırır.
- `InventoryModal`: 4 kuşanılmış slot, 24 yuvalı çanta ızgarası (`GridContainer`), eşya detay paneli, "Kuşan/Çıkar", "Bile (+1)", "Parçala" aksiyonları barındırır (< 245 satır).

- [ ] **Step 1: `Scripts/UI/InventoryModal.cs` sınıfını oluştur (< 245 satır)**
- [ ] **Step 2: `Scripts/UI/InventoryController.cs` sınıfını oluştur (< 90 satır)**
- [ ] **Step 3: `Scenes/MainCombat.tscn` sahnesine `InventoryController` CanvasLayer düğümünü ekle**
- [ ] **Step 4: `dotnet build` çalıştır**
- [ ] **Step 5: Commit (`feat: create gothic InventoryModal and independent InventoryController`)**

---

### Task 7: Tam Doğrulama ve Entegrasyon Kapısı (Verification Gate)

- [ ] **Step 1: Derleme Doğrulaması:** `dotnet build` (0 hata, 0 uyarı).
- [ ] **Step 2: Birim Testleri:** `dotnet test` (Tüm xUnit testleri 100% başarılı).
- [ ] **Step 3: Headless Smoke Test:** `Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 60` ile sahne çökmesi veya eksik kaynak olmadığını doğrula.
- [ ] **Step 4: Satır Sayısı Denetimi:** `MainHUD.cs`, `GameManager.cs`, `InventoryModal.cs`, `SaveSystem.cs` tüm dosyaların < 250 satır olduğunu teyit et.
- [ ] **Step 5: HANDOVER.md güncellemesi ve final commit.**
