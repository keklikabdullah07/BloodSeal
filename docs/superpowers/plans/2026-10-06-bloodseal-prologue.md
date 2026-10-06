# BloodSeal Prologue & Lineage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the atmospheric Gothic Prologue screen enabling the player to name their hero, select 1 of 5 Bloodlines and 1 of 5 Street Origins, applying permanent passive combat bonuses and transitioning to combat.

**Architecture:** Data models defined in `CharacterProfile.cs`, stored in `GameManager` and persisted via `SaveData`. `PrologueController` manages multi-step UI flow with reusable `SelectionCard` widgets. Combat actors and stats consume the active profile.

**Tech Stack:** Godot 4.7.2 Mono (.NET 10 / C#), Compatibility Renderer, 1920x1080.

## Global Constraints
- Target platform: 1920x1080, .NET 10, C# 12+.
- Follow "Call Down, Signal Up" and keep classes under 250 lines (AGENTS.md).
- Zero compilation errors and zero warnings.

---

### Task 1: C# Data Models & Combat Passive Integration

**Files:**
- Create: `Scripts/Core/CharacterProfile.cs`
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Combat/PentagramStats.cs`
- Modify: `Scripts/Core/GameManager.cs`
- Modify: `Scripts/Combat/Hero.cs`
- Modify: `Scripts/Combat/PetCompanion.cs`
- Modify: `Scripts/Combat/TapCombatArea.cs`
- Modify: `Scripts/Combat/Enemy.cs`

- [ ] **Step 1: Create `Scripts/Core/CharacterProfile.cs` with enums and profile class**

```csharp
namespace BloodSeal.Core
{
    public enum BloodlineType
    {
        BoneWeaver,    // Kemik Dokulu (+%10 Maksimum Can)
        ShadowVeined,  // Gölge Damarlı (+35px Saldırı Menzili)
        BloodClawed,   // Kan Pençeli (+%2.5 Doğuştan Can Çalma)
        SteelFleshed,  // Çelik Dokulu (Gelen hasardan -3 düz azaltma)
        SoulDrinker    // Ruh Emici (-%15 Saldırı bekleme süresi)
    }

    public enum StreetOriginType
    {
        PitFighter,      // Kafes Dövüşçüsü (+%10 Saldırı Gücü)
        StreetThief,     // Sokak Hırsızı (+%15 Altın Kazanımı)
        ExMercenary,     // Eski Paralı Asker (+%8 Saldırı Hızı)
        UnderAlchemist,  // Yeraltı Kimyageri (+%25 Tıklama Hasarı)
        GangLeader       // Çete Lideri (+%25 Ruh/Pet Atış Hızı)
    }

    public class CharacterProfile
    {
        public string PlayerName { get; set; } = "Valerius";
        public BloodlineType Bloodline { get; set; } = BloodlineType.BoneWeaver;
        public StreetOriginType Origin { get; set; } = StreetOriginType.PitFighter;
        public bool HasCompletedPrologue { get; set; } = false;

        public static string GetBloodlineName(BloodlineType type) => type switch
        {
            BloodlineType.BoneWeaver => "Kemik Dokulu",
            BloodlineType.ShadowVeined => "Gölge Damarlı",
            BloodlineType.BloodClawed => "Kan Pençeli",
            BloodlineType.SteelFleshed => "Çelik Dokulu",
            BloodlineType.SoulDrinker => "Ruh Emici",
            _ => type.ToString()
        };

        public static string GetOriginName(StreetOriginType type) => type switch
        {
            StreetOriginType.PitFighter => "Kafes Dövüşçüsü",
            StreetOriginType.StreetThief => "Sokak Hırsızı",
            StreetOriginType.ExMercenary => "Eski Paralı Asker",
            StreetOriginType.UnderAlchemist => "Yeraltı Kimyageri",
            StreetOriginType.GangLeader => "Çete Lideri",
            _ => type.ToString()
        };
    }
}
```

- [ ] **Step 2: Update `SaveData.cs` and `GameManager.cs` to store `CharacterProfile`**
- [ ] **Step 3: Update `PentagramStats.cs`, `Hero.cs`, `PetCompanion.cs`, `TapCombatArea.cs`, and `Enemy.cs` to apply the passive bonuses**
- [ ] **Step 4: Run `dotnet build`**
- [ ] **Step 5: Commit**

---

### Task 2: Reusable Selection Card Widget

**Files:**
- Create: `Scripts/UI/SelectionCard.cs`
- Create: `Scenes/UI/SelectionCard.tscn`

- [ ] **Step 1: Create `SelectionCard.cs` with title, passive description, and selected highlight**
- [ ] **Step 2: Create `SelectionCard.tscn` with gothic dark card style and border highlight**
- [ ] **Step 3: Run `dotnet build`**
- [ ] **Step 4: Commit**

---

### Task 3: Prologue Scene & Awakening Flow

**Files:**
- Create: `Scripts/UI/PrologueController.cs`
- Create: `Scenes/Prologue/PrologueScene.tscn`
- Modify: `Scripts/UI/MainHUD.cs`
- Modify: `Scenes/UI/MainHUD.tscn`
- Modify: `project.godot`

- [ ] **Step 1: Create `PrologueController.cs` to drive step transitions (Name -> Bloodline -> Origin -> Awakening)**
- [ ] **Step 2: Create `Scenes/Prologue/PrologueScene.tscn` with gothic pentagram atmosphere and cards**
- [ ] **Step 3: Update `MainHUD.cs` and `MainHUD.tscn` to display PlayerName and active title**
- [ ] **Step 4: Set `run/main_scene="res://Scenes/Prologue/PrologueScene.tscn"` in `project.godot`**
- [ ] **Step 5: Run `dotnet build` and headless verification**
- [ ] **Step 6: Commit and Push**
