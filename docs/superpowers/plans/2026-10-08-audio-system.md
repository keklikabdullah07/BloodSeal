# 🩸 BloodSeal: Ses Mimarisi ve Gotik Ses Deneyimi Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Gotik fantezi atmosferini güçlendiren dinamik çift kanallı BGM crossfade sistemini, 6 kanallı döngüsel SFX havuzunu, kapsamlı ses envanterini, disk kalıcılığını ve Gotik `SettingsModal` arayüzünü inşa etmek.

**Architecture:** Godot Autoload `AudioManager.cs` ile bus ve kanal kontrolü; veri tanımları için `AudioData.cs`; logaritmik desibel dönüşümü (`Mathf.LinearToDb`); `SaveData` & `SaveSystem` kalıcılığı; ve `LibraryModal` görsel standardında `SettingsModal`.

**Tech Stack:** C# 12 / .NET 10.0 (`net10.0`), Godot 4.7.x Mono AudioServer & AudioStreamPlayer, xUnit test paketi (`Tests/BloodSeal.Tests.csproj`).

## Global Constraints
- **AGENTS.md Satır Sınırı:** Hiçbir C# dosyası 250 satırı aşamaz (`AudioManager.cs`, `SettingsModal.cs`).
- **TimeScale Yasağı:** `Engine.TimeScale` değiştirilemez.
- **Logaritmik Desibel:** Desibel dönüşümünde $Linear = 0.0$ değeri eksi sonsuza gitmeyip $-80.0\text{ dB}$'e clamp edilir.
- **0 Uyarı 0 Hata:** `dotnet build` ve `dotnet test` her görev sonunda 0 hata ve 0 uyarı ile tamamlanmalıdır.

---

### Task 1: Ses Tanımları, Veri Modelleri ve Birim Testleri (`AudioData.cs` & `AudioSystemTests.cs`)

**Files:**
- Create: `Scripts/Core/AudioData.cs`
- Create: `Tests/AudioSystemTests.cs`

**Interfaces:**
- Produces: `enum BgmTrackType`, `enum AudioCueType`, `class AudioData` (`LinearToDb(float linear)`, `DbToLinear(float db)`)

- [ ] **Step 1: Write the failing test for Audio data and math calculations**

```csharp
// In Tests/AudioSystemTests.cs
using System;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class AudioSystemTests
    {
        [Theory]
        [InlineData(1.0f, 0.0f)]
        [InlineData(0.5f, -6.02f)]
        [InlineData(0.1f, -20.0f)]
        [InlineData(0.0f, -80.0f)]
        [InlineData(-0.5f, -80.0f)]
        public void AudioData_LinearToDb_CalculatesCorrectly(float linear, float expectedDb)
        {
            float db = AudioData.LinearToDb(linear);
            if (expectedDb == -80f)
            {
                Assert.Equal(-80f, db);
            }
            else
            {
                Assert.InRange(db, expectedDb - 0.2f, expectedDb + 0.2f);
            }
        }

        [Fact]
        public void AudioCueType_Contains_All_Expected_Cues()
        {
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "Slash"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "Hit"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "CritHit"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "Tap"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "RageBurst"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "BossEnrage"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "Coin"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "BossVictory"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "HeroDeath"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "RelicUnlock"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "AwakeningRitual"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "ButtonClick"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "ModalOpen"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "ModalClose"));
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~AudioSystemTests" -v minimal`  
Expected: FAIL (types do not exist yet).

- [ ] **Step 3: Implement `AudioData.cs`**

```csharp
// In Scripts/Core/AudioData.cs
#nullable enable
using System;

namespace BloodSeal.Core
{
    public enum BgmTrackType
    {
        GothicAmbient = 0,
        BossCombat = 1
    }

    public enum AudioCueType
    {
        Slash = 0,
        Hit = 1,
        CritHit = 2,
        Tap = 3,
        RageBurst = 4,
        BossEnrage = 5,
        Coin = 6,
        BossVictory = 7,
        HeroDeath = 8,
        RelicUnlock = 9,
        AwakeningRitual = 10,
        ButtonClick = 11,
        ModalOpen = 12,
        ModalClose = 13
    }

    public static class AudioData
    {
        public const float MinDb = -80.0f;
        public const float MaxDb = 0.0f;

        public static float LinearToDb(float linear)
        {
            if (linear <= 0.0001f) return MinDb;
            float db = (float)(20.0 * Math.Log10(Math.Clamp(linear, 0.0001f, 1.0f)));
            return Math.Clamp(db, MinDb, MaxDb);
        }

        public static float DbToLinear(float db)
        {
            if (db <= MinDb) return 0.0f;
            return (float)Math.Pow(10.0, Math.Clamp(db, MinDb, MaxDb) / 20.0);
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~AudioSystemTests" -v minimal`  
Expected: PASS (2 passed).

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/AudioData.cs Tests/AudioSystemTests.cs
git commit -m "feat(audio): implement AudioData models, cues and decibel conversion tests"
```

---

### Task 2: Ses Kayıt Kalıcılığı (`SaveData.cs` & `SaveSystem.cs`)

**Files:**
- Modify: `Scripts/Core/SaveData.cs:20-35`
- Modify: `Scripts/Core/SaveSystem.cs`
- Modify: `Tests/AudioSystemTests.cs`

**Interfaces:**
- Produces: `SaveData.MasterVolume`, `SaveData.BgmVolume`, `SaveData.SfxVolume`, `SaveData.IsMuted`

- [ ] **Step 1: Write test for audio settings persistence**

```csharp
// In Tests/AudioSystemTests.cs
        [Fact]
        public void SaveData_SerializesAndRestores_AudioSettings()
        {
            var data = new SaveData
            {
                MasterVolume = 0.75f,
                BgmVolume = 0.60f,
                SfxVolume = 0.90f,
                IsMuted = true
            };

            string json = System.Text.Json.JsonSerializer.Serialize(data);
            var restored = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json);

            Assert.NotNull(restored);
            Assert.Equal(0.75f, restored.MasterVolume);
            Assert.Equal(0.60f, restored.BgmVolume);
            Assert.Equal(0.90f, restored.SfxVolume);
            Assert.True(restored.IsMuted);
        }
```

- [ ] **Step 2: Run test to verify failure**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~SaveData_SerializesAndRestores_AudioSettings" -v minimal`  
Expected: FAIL (properties do not exist on `SaveData`).

- [ ] **Step 3: Modify `SaveData.cs` to add audio properties**

Add properties with defaults:
```csharp
public float MasterVolume { get; set; } = 1.0f;
public float BgmVolume { get; set; } = 0.8f;
public float SfxVolume { get; set; } = 1.0f;
public bool IsMuted { get; set; } = false;
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~AudioSystemTests" -v minimal`  
Expected: PASS (3 passed).

- [ ] **Step 5: Commit**

```bash
git add Scripts/Core/SaveData.cs Tests/AudioSystemTests.cs
git commit -m "feat(save): add audio volume and mute settings persistence to SaveData"
```

---

### Task 3: Eksik Ses Efektlerinin ve Boss Müziğinin Prosedürel Üretimi (`Audio/`)

**Files:**
- Create: `Audio/BGM/boss_combat.wav`
- Create: `Audio/SFX/rage_burst.wav`
- Create: `Audio/SFX/boss_victory.wav`
- Create: `Audio/SFX/hero_death.wav`
- Create: `Audio/SFX/relic_unlock.wav`
- Create: `Audio/SFX/awakening_ritual.wav`
- Create: `Audio/SFX/button_click.wav`
- Create: `Audio/SFX/modal_open.wav`
- Create: `Audio/SFX/modal_close.wav`
- Modify: `Audio/BGM/gothic_ambient.wav.import` (set `loop_mode = 1`)

**Interfaces:**
- Produces: Prosedürel 16-bit 44.1kHz mono WAV dosyaları ve Godot import ayarları

- [ ] **Step 1: Write a temporary C# audio generator script and synthesize clean WAV assets**

Generate high quality sound waves:
- `boss_combat.wav`: Yoğun bas ve ritmik karanlık gotik savaş teması (12 saniyelik kesintisiz loop).
- `rage_burst.wav`: Derin patlama ve boğuk kükreme rezonansı (1.2 sn).
- `boss_victory.wav`: Zafer ve ganimet akoru (1.5 sn).
- `hero_death.wav`: Ağır düşüş ve karanlık darbe (1.0 sn).
- `relic_unlock.wav`: Antik çan ve mistik yankı (1.8 sn).
- `awakening_ritual.wav`: Derin kan ahdi gongu ve rüzgar (2.2 sn).
- `button_click.wav`: Kısa, tok gotik tık sesi (0.08 sn).
- `modal_open.wav`: Sayfa/taş sürtünme sesi (0.25 sn).
- `modal_close.wav`: Pencere kapanma sesi (0.20 sn).

- [ ] **Step 2: Update `.import` files for BGM loops**

In `Audio/BGM/gothic_ambient.wav.import` and `Audio/BGM/boss_combat.wav.import`:
Ensure `edit/loop_mode=1` is set so Godot automatically loops background music.

- [ ] **Step 3: Run Godot headless to import all generated audio assets**

Run: `& "C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe" --headless --quit-after 10`  
Expected: Exit code 0 (all WAV files imported).

- [ ] **Step 4: Commit audio assets**

```bash
git add Audio/
git commit -m "feat(audio): generate procedural gothic WAV sound effects and boss combat BGM"
```

---

### Task 4: Çok Kanallı Havuz ve Çift Kanallı Crossfade ile `AudioManager.cs` Yenilenmesi

**Files:**
- Modify: `Scripts/Core/AudioManager.cs`

**Interfaces:**
- Produces:
  - `PlayBGM(BgmTrackType track, float duration = 1.2f)`
  - `PlaySFX(AudioCueType cue, float pitchMin = 0.95f, float pitchMax = 1.05f)`
  - `ApplySettings(float master, float bgm, float sfx, bool isMuted)`
  - `SetMasterVolume(float vol)`, `SetBgmVolume(float vol)`, `SetSfxVolume(float vol)`, `SetMuted(bool muted)`

- [ ] **Step 1: Implement multi-channel pool and dual track crossfade in `AudioManager.cs`**

Gereksinimler:
- `_bgmTrackA`, `_bgmTrackB` çift oynatıcı.
- 6 kanallı döngüsel `_sfxPool`.
- `PlaySFX(AudioCueType cue)` metodunun `AudioData.LinearToDb` kullanması.
- Dosya uzunluğu kesinlikle **250 satırın altında** tutulur (~190-210 satır).

- [ ] **Step 2: Run `dotnet build` to ensure 0 errors**

Run: `dotnet build`  
Expected: 0 Warning(s), 0 Error(s).

- [ ] **Step 3: Run test suite**

Run: `dotnet test Tests/BloodSeal.Tests.csproj -v minimal`  
Expected: All tests pass.

- [ ] **Step 4: Commit**

```bash
git add Scripts/Core/AudioManager.cs
git commit -m "feat(audio): implement dual-track BGM crossfade and 6-channel SFX pool in AudioManager"
```

---

### Task 5: Gotik `SettingsModal.cs` ve HUD Entegrasyonu

**Files:**
- Create: `Scripts/UI/SettingsModal.cs`
- Modify: `Scripts/UI/MainHUD.cs`

**Interfaces:**
- Produces: `class SettingsModal : Control` (`ShowModal()`, `CloseModal()`)
- Connects: `MainHUD.SettingsBtn` $\rightarrow$ `SettingsModal.ShowModal()`

- [ ] **Step 1: Implement `SettingsModal.cs`**

Bileşenler:
- Gotik Panel tasarımı.
- Master, BGM, SFX ses sürgüleri (`HSlider` 0-100%).
- Yüzde etiketleri (`Label`).
- Sessize al kutucukları (`CheckBox`).
- Değişikliklerde `AudioManager.Instance.SetXVolume()` ve `SaveSystem.SaveGame()` çağrısı.
- Sürgü hareketi bittiğinde `AudioManager.Instance.PlaySFX(AudioCueType.ButtonClick)`.

- [ ] **Step 2: Integrate `SettingsBtn` and `SettingsModal` into `MainHUD.cs`**

- `[Export] public Button? SettingsBtn;`
- `[Export] public SettingsModal? SettingsModal;`
- `_Ready()` içinde bağlantı ve programatik fallback desteği.

- [ ] **Step 3: Run `dotnet build`**

Run: `dotnet build`  
Expected: 0 Warning(s), 0 Error(s).

- [ ] **Step 4: Commit**

```bash
git add Scripts/UI/SettingsModal.cs Scripts/UI/MainHUD.cs
git commit -m "feat(ui): implement Gothic SettingsModal with volume sliders and HUD access"
```

---

### Task 6: Oyun İçi Ses Tetikleyicilerinin Bağlanması (Gameplay Triggers)

**Files:**
- Modify: `Scripts/Combat/Hero.cs` (HeroDeath)
- Modify: `Scripts/Combat/BossEnemy.cs` (BossVictory, BossEnrage)
- Modify: `Scripts/Core/GameManager.cs` (RageBurst, Dalga BGM crossfade)
- Modify: `Scripts/Core/AwakeningManager.cs` (AwakeningRitual)
- Modify: `Scripts/Core/RelicManager.cs` (RelicUnlock)

**Interfaces:**
- Entegrasyon: Olaylar gerçekleştiğinde `AudioManager.Instance?.PlaySFX(...)` veya `PlayBGM(...)` çağrılır.

- [ ] **Step 1: Wire Hero, Boss and Combat audio events**
- [ ] **Step 2: Wire Rage and Boss wave BGM crossfades**
- [ ] **Step 3: Wire Awakening ritual and Relic unlock audio triggers**
- [ ] **Step 4: Run `dotnet build` and `dotnet test`**

Run: `dotnet build`  
Run: `dotnet test Tests/BloodSeal.Tests.csproj -v minimal`  
Expected: 0 Errors, 0 Warnings, All tests pass.

- [ ] **Step 5: Commit**

```bash
git add Scripts/Combat/ Scripts/Core/
git commit -m "feat(audio): wire gameplay triggers for rage, boss combat, relics and rebirth rituals"
```

---

### Task 7: Tam Doğrulama ve Headless Test (Verification Gate)

**Files:**
- Test all modified files

- [ ] **Step 1: Run comprehensive test suite**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --logger "console;verbosity=detailed"`  
Expected: All tests pass.

- [ ] **Step 2: Run Godot headless smoke test**

Run: `& "C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe" --headless --quit-after 60`  
Expected: Exit code 0, no crashes.

- [ ] **Step 3: Update `Reports/PROJECT_STATE.md` with completed Audio system**

- [ ] **Step 4: Final commit**

```bash
git add Reports/PROJECT_STATE.md
git commit -m "docs: update PROJECT_STATE.md with completed Audio Architecture and Gothic Sound Experience"
```
