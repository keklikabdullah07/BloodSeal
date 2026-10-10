# 📱 Android Mobil Optimizasyonu & Dokunmatik Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Android mobil platformu için haptik geri bildirim servisi (`HapticManager`), çoklu dokunmatik savaş alanı (`TapCombatArea`), dinamik ekran çentiği / safe area koruyucusu (`SafeAreaHandler`) ve Android export izin/ayarlarını ekleyerek eksiksiz bir mobil deneyim sağlamak.

**Architecture:** Modüler ve saf C# tabanlı `HapticManager` donanım titreşimlerini yönetir; `SaveSystem` ve `SettingsModal` ile oyuncu tercihine bağlanır. `SafeAreaHandler`, `DisplayServer.GetDisplaySafeArea()` ile kamera çentiklerini algılayıp HUD kenar marjinlerini dinamik genişletir. `TapCombatArea` çoklu parmak dokunuşlarını (`touchEvent.Index`) bağımsız işler.

**Tech Stack:** Godot 4.7 Mono (C#), .NET 10.0, xUnit, DisplayServer API, Input.VibrateHandheld API.

## Global Constraints
- Engine.TimeScale'e ASLA dokunma.
- Tüm sınıflar kesinlikle **250 satır sınırının altında** kalmalıdır.
- `dotnet build` 0 uyarı ve 0 hata ile tamamlanmalıdır.
- Tüm xUnit birim testleri (en az 94 mevcut + yeni testler) %100 başarıyla geçmelidir.

---

### Task 1: Haptik Servisi (`HapticManager.cs`) ve Birim Testleri

**Files:**
- Create: `Scripts/Core/HapticManager.cs`
- Create: `Tests/HapticAndMobileTests.cs`
- Modify: `Tests/BloodSeal.Tests.csproj`

**Interfaces:**
- Produces: `HapticManager.Instance.IsHapticsEnabled`, `VibrateLight()`, `VibrateMedium()`, `VibrateHeavy()`, `Vibrate(int ms)`

- [ ] **Step 1: Write the failing xUnit test for HapticManager**

Create `Tests/HapticAndMobileTests.cs`:
```csharp
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class HapticAndMobileTests
    {
        [Fact]
        public void HapticManager_DefaultsToEnabled_AndTogglesCorrectly()
        {
            var haptic = HapticManager.Instance;
            Assert.NotNull(haptic);
            haptic.IsHapticsEnabled = true;
            Assert.True(haptic.IsHapticsEnabled);

            haptic.IsHapticsEnabled = false;
            Assert.False(haptic.IsHapticsEnabled);

            // Re-enable for subsequent tests
            haptic.IsHapticsEnabled = true;
        }

        [Fact]
        public void HapticManager_SafeVibrateCalls_DoNotThrowExceptions()
        {
            var haptic = HapticManager.Instance;
            haptic.IsHapticsEnabled = true;

            // Safe calls on non-mobile/headless environment must never crash
            var ex1 = Record.Exception(() => haptic.VibrateLight());
            var ex2 = Record.Exception(() => haptic.VibrateMedium());
            var ex3 = Record.Exception(() => haptic.VibrateHeavy());
            var ex4 = Record.Exception(() => haptic.Vibrate(50));

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Null(ex4);
        }

        [Fact]
        public void HapticManager_WhenDisabled_DoesNotExecuteVibration()
        {
            var haptic = HapticManager.Instance;
            haptic.IsHapticsEnabled = false;

            bool called = haptic.Vibrate(20);
            Assert.False(called);

            haptic.IsHapticsEnabled = true;
        }
    }
}
```

- [ ] **Step 2: Add test file to Tests/BloodSeal.Tests.csproj & run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Expected: Compile error because `HapticManager` does not exist yet.

- [ ] **Step 3: Implement HapticManager.cs**

Create `Scripts/Core/HapticManager.cs`:
```csharp
using System;
using Godot;

namespace BloodSeal.Core
{
    public class HapticManager
    {
        private static HapticManager _instance;
        public static HapticManager Instance => _instance ??= new HapticManager();

        public bool IsHapticsEnabled { get; set; } = true;

        public const int LightDurationMs = 15;
        public const int MediumDurationMs = 35;
        public const int HeavyDurationMs = 75;

        public event Action<bool> OnHapticsToggled;

        public void SetHapticsEnabled(bool enabled)
        {
            if (IsHapticsEnabled == enabled) return;
            IsHapticsEnabled = enabled;
            OnHapticsToggled?.Invoke(IsHapticsEnabled);
        }

        public bool VibrateLight() => Vibrate(LightDurationMs);
        public bool VibrateMedium() => Vibrate(MediumDurationMs);
        public bool VibrateHeavy() => Vibrate(HeavyDurationMs);

        public bool Vibrate(int durationMs)
        {
            if (!IsHapticsEnabled || durationMs <= 0) return false;

            try
            {
                // In Godot 4, Input.VibrateHandheld invokes hardware vibration on mobile
                Input.VibrateHandheld(durationMs);
                return true;
            }
            catch
            {
                // Graceful fallback on unsupported desktop/headless platforms
                return false;
            }
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Expected: PASS (97/97 tests pass).

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/HapticManager.cs Tests/HapticAndMobileTests.cs Tests/BloodSeal.Tests.csproj
git commit -m "feat: implement HapticManager service and unit tests"
```

---

### Task 2: Haptik Ayarlarının Kalıcılığı (`SaveData`, `SaveSystem`, `SettingsModal`)

**Files:**
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Core/SaveSystem.cs`
- Modify: `Scripts/UI/SettingsModal.cs`
- Modify: `Tests/HapticAndMobileTests.cs`

**Interfaces:**
- Consumes: `HapticManager.Instance.IsHapticsEnabled`
- Produces: `SaveData.IsHapticsEnabled`, `SettingsModal` vibration toggle button

- [ ] **Step 1: Write test for Haptics Save/Load serialization**

Add to `Tests/HapticAndMobileTests.cs`:
```csharp
[Fact]
public void SaveData_SerializesAndRestores_IsHapticsEnabled()
{
    var data = new SaveData { IsHapticsEnabled = false };
    string json = System.Text.Json.JsonSerializer.Serialize(data);
    var loaded = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json);

    Assert.NotNull(loaded);
    Assert.False(loaded.IsHapticsEnabled);
}
```

- [ ] **Step 2: Update SaveData.cs and SaveSystem.cs**

In `Scripts/Core/SaveData.cs`:
Add property:
```csharp
public bool IsHapticsEnabled { get; set; } = true;
```

In `Scripts/Core/SaveSystem.cs`:
In `SaveGame()`:
`data.IsHapticsEnabled = HapticManager.Instance.IsHapticsEnabled;`
In `LoadGame()`:
`HapticManager.Instance.SetHapticsEnabled(data.IsHapticsEnabled);`

- [ ] **Step 3: Update SettingsModal.cs with Haptics Toggle Button**

In `Scripts/UI/SettingsModal.cs`:
Add a button for Haptics toggle in the settings card list:
`📳 Titreşim: [AÇIK / KAPALI]`.
When clicked: toggles `HapticManager.Instance.SetHapticsEnabled(!current)`, plays click sound, and triggers `HapticManager.Instance.VibrateLight()`.

- [ ] **Step 4: Verify tests and line counts**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Ensure `SettingsModal.cs` and `SaveSystem.cs` are strictly < 250 lines.

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/SaveData.cs Scripts/Core/SaveSystem.cs Scripts/UI/SettingsModal.cs Tests/HapticAndMobileTests.cs
git commit -m "feat: persist haptics state in SaveSystem and add toggle to SettingsModal"
```

---

### Task 3: Çoklu Dokunmatik (Multi-Touch) ve Vuruş Hissiyatı (`TapCombatArea.cs`)

**Files:**
- Modify: `Scripts/Combat/TapCombatArea.cs`
- Modify: `Scripts/Combat/Hero.cs`
- Modify: `Scripts/Combat/BossEnemy.cs`

**Interfaces:**
- Consumes: `HapticManager.Instance`
- Produces: Multi-finger touch support with haptic pulse on combat events

- [ ] **Step 1: Update TapCombatArea.cs to handle multi-touch & haptics**

In `Scripts/Combat/TapCombatArea.cs`:
- Support multi-finger touches via `InputEventScreenTouch`:
  Track active touch pointers so multiple fingers simultaneously tapping spawn `TapRipple` and deal tap damage.
- Add rate limiter (max 16 taps per second to prevent automated macro spam while allowing fast multi-finger frenzy).
- Call `HapticManager.Instance.VibrateLight()` on tap hit.

- [ ] **Step 2: Integrate Medium & Heavy Haptics into Hero and Boss**

In `Scripts/Combat/Hero.cs`:
- When a critical hit lands (`isCrit == true`), invoke `HapticManager.Instance.VibrateMedium()`.
- When Berserk mode is activated, invoke `HapticManager.Instance.VibrateHeavy()`.

In `Scripts/Combat/BossEnemy.cs`:
- In `OnDied()` or death sequence, invoke `HapticManager.Instance.VibrateHeavy()`.

- [ ] **Step 3: Run dotnet build and unit tests**

Run: `dotnet build && dotnet test Tests/BloodSeal.Tests.csproj`
Verify 0 errors, 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add Scripts/Combat/TapCombatArea.cs Scripts/Combat/Hero.cs Scripts/Combat/BossEnemy.cs
git commit -m "feat: add multi-touch tap combat support and layered haptic feedback"
```

---

### Task 4: Dinamik Mobil Safe Area / Çentik Koruyucu (`SafeAreaHandler.cs`)

**Files:**
- Create: `Scripts/UI/SafeAreaHandler.cs`
- Modify: `Scenes/MainCombat.tscn`

**Interfaces:**
- Consumes: `DisplayServer.GetDisplaySafeArea()`, `DisplayServer.WindowGetSize()`
- Produces: Dynamic margin adaptation for notch and rounded corners

- [ ] **Step 1: Implement SafeAreaHandler.cs**

Create `Scripts/UI/SafeAreaHandler.cs`:
- Inspects screen safe area on `_Ready()` and upon window resize signal.
- In landscape orientation, calculates left and right notch insets (e.g., if safe area X > 0 or safe area width < window width).
- Dynamically applies extra margin to `TopBar/Margin` and `BottomPanel/Margin`.

- [ ] **Step 2: Add SafeAreaHandler node to MainCombat.tscn**

Add `SafeAreaHandler` node in `Scenes/MainCombat.tscn` under UI layer.

- [ ] **Step 3: Verify with dotnet build and headless smoke test**

Run: `dotnet build`
Run: `Godot_console.exe --headless --quit-after 60`

- [ ] **Step 4: Commit**

```bash
git add Scripts/UI/SafeAreaHandler.cs Scenes/MainCombat.tscn
git commit -m "feat: add dynamic SafeAreaHandler for mobile camera notch insets"
```

---

### Task 5: Proje & Android Export Yapılandırması (`project.godot`, `export_presets.cfg`)

**Files:**
- Modify: `project.godot`
- Modify: `export_presets.cfg`

**Interfaces:**
- Produces: Touch emulation parity and Android VIBRATE permission

- [ ] **Step 1: Update project.godot pointing and input devices**

Add to `project.godot`:
```ini
[input_devices]

pointing/emulate_touch_from_mouse=true
pointing/emulate_mouse_from_touch=true
```

- [ ] **Step 2: Update export_presets.cfg for Android**

In `export_presets.cfg` under `[preset.1.options]`:
Ensure `permissions/vibrate=true`.

- [ ] **Step 3: Commit**

```bash
git add project.godot export_presets.cfg
git commit -m "chore: enable touch emulation and Android vibrate permission"
```

---

### Task 6: Doğrulama Kapısı & Handover Güncellemesi

**Files:**
- Modify: `docs/superpowers/HANDOVER.md`

- [ ] **Step 1: Run full verification suite**
  1. `dotnet build` (0 warning, 0 error).
  2. `dotnet test Tests/BloodSeal.Tests.csproj` (all passing).
  3. Satır sayıları kontrolü (tüm dosyalar < 250 satır).
  4. Godot headless smoke test.
- [ ] **Step 2: Update docs/superpowers/HANDOVER.md**
- [ ] **Step 3: Final Commit and Git Push to origin/developer**
