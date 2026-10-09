# 🩸 BloodSeal: Öğretici & İlk Kullanıcı Deneyimi (FTUE) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a completely decoupled, non-intrusive onboarding/tutorial system (FTUE) that guides new players through core mechanics (tap combat, ATK upgrade, Berserk activation, Manor Gate encounter) with pulsing gothic callouts without modifying `MainHUD.cs` or violating the 250-line rule.

**Architecture:** A pure C# domain manager (`TutorialManager`) handles state transitions and persistence via `SaveSystem`. A decoupled UI controller (`TutorialController`) and floating callout widget (`TutorialHintCallout`) observe state and point to relevant UI elements or combat areas with `MouseFilter = Ignore`.

**Tech Stack:** C# 12 / .NET 10.0, Godot 4.7 Mono, xUnit test suite.

## Global Constraints
- Maximum 250 lines per C# source file.
- `Engine.TimeScale` must NEVER be modified.
- All tutorial overlays must use `MouseFilter = MouseFilterEnum.Ignore` so gameplay input is never blocked.
- "Call Down, Signal Up" decoupling: `MainHUD.cs` must NOT be bloated or directly coupled to tutorial logic.
- All numbers use `double` or `float` with appropriate bounds.
- Full unit test coverage in `BloodSeal.Tests`.

---

### Task 1: Core Tutorial Models and TutorialManager

**Files:**
- Create: `Scripts/Core/TutorialModels.cs`
- Create: `Scripts/Core/TutorialManager.cs`
- Test: `Tests/TutorialSystemTests.cs`

**Interfaces:**
- Consumes: None
- Produces:
  - `enum TutorialStep { NotStarted = 0, TapToAttack = 1, UpgradeAttack = 2, ActivateBerserk = 3, VisitManorGate = 4, Completed = 5 }`
  - `class TutorialManager`:
    - `public static TutorialManager Instance { get; }`
    - `public TutorialStep CurrentStep { get; private set; }`
    - `public int TapCount { get; private set; }`
    - `public bool IsCompleted { get; }`
    - `public event Action<TutorialStep> OnTutorialStepChanged;`
    - `public event Action OnTutorialCompleted;`
    - `public void RegisterTap()`
    - `public void RecordEnemyDefeated()`
    - `public void CheckGoldProgress(double currentGold)`
    - `public void RecordStatUpgraded()`
    - `public void CheckRageProgress(bool isRageActive)`
    - `public void RecordGateVisited()`
    - `public void SetStep(TutorialStep step)`
    - `public void Reset()`

- [ ] **Step 1: Write the failing tests for TutorialManager**

Create `Tests/TutorialSystemTests.cs`:
```csharp
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class TutorialSystemTests
    {
        public TutorialSystemTests()
        {
            TutorialManager.Instance.Reset();
        }

        [Fact]
        public void InitialState_DefaultsToTapToAttack()
        {
            var mgr = TutorialManager.Instance;
            Assert.Equal(TutorialStep.TapToAttack, mgr.CurrentStep);
            Assert.False(mgr.IsCompleted);
        }

        [Fact]
        public void RegisterTap_AdvancesToUpgradeAttack_AfterThreeTaps()
        {
            var mgr = TutorialManager.Instance;
            mgr.RegisterTap();
            mgr.RegisterTap();
            Assert.Equal(TutorialStep.TapToAttack, mgr.CurrentStep);

            mgr.RegisterTap();
            Assert.Equal(TutorialStep.UpgradeAttack, mgr.CurrentStep);
        }

        [Fact]
        public void RecordEnemyDefeated_ImmediatelyAdvancesStep1()
        {
            var mgr = TutorialManager.Instance;
            mgr.RecordEnemyDefeated();
            Assert.Equal(TutorialStep.UpgradeAttack, mgr.CurrentStep);
        }

        [Fact]
        public void RecordStatUpgraded_AdvancesToActivateBerserk()
        {
            var mgr = TutorialManager.Instance;
            mgr.SetStep(TutorialStep.UpgradeAttack);

            mgr.RecordStatUpgraded();
            Assert.Equal(TutorialStep.ActivateBerserk, mgr.CurrentStep);
        }

        [Fact]
        public void CheckRageProgress_AdvancesToVisitManorGate_WhenActive()
        {
            var mgr = TutorialManager.Instance;
            mgr.SetStep(TutorialStep.ActivateBerserk);

            mgr.CheckRageProgress(false);
            Assert.Equal(TutorialStep.ActivateBerserk, mgr.CurrentStep);

            mgr.CheckRageProgress(true);
            Assert.Equal(TutorialStep.VisitManorGate, mgr.CurrentStep);
        }

        [Fact]
        public void RecordGateVisited_CompletesTutorial()
        {
            var mgr = TutorialManager.Instance;
            mgr.SetStep(TutorialStep.VisitManorGate);

            bool completedFired = false;
            mgr.OnTutorialCompleted += () => completedFired = true;

            mgr.RecordGateVisited();

            Assert.Equal(TutorialStep.Completed, mgr.CurrentStep);
            Assert.True(mgr.IsCompleted);
            Assert.True(completedFired);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~TutorialSystemTests"`
Expected: FAIL (compilation error, `TutorialStep` and `TutorialManager` do not exist).

- [ ] **Step 3: Implement TutorialModels.cs and TutorialManager.cs**

Create `Scripts/Core/TutorialModels.cs`:
```csharp
namespace BloodSeal.Core
{
    public enum TutorialStep
    {
        NotStarted = 0,
        TapToAttack = 1,
        UpgradeAttack = 2,
        ActivateBerserk = 3,
        VisitManorGate = 4,
        Completed = 5
    }
}
```

Create `Scripts/Core/TutorialManager.cs`:
```csharp
using System;

namespace BloodSeal.Core
{
    public class TutorialManager
    {
        private static TutorialManager _instance;
        public static TutorialManager Instance => _instance ??= new TutorialManager();

        public const int RequiredTapCount = 3;

        public TutorialStep CurrentStep { get; private set; } = TutorialStep.TapToAttack;
        public int TapCount { get; private set; } = 0;
        public bool IsCompleted => CurrentStep == TutorialStep.Completed;

        public event Action<TutorialStep> OnTutorialStepChanged;
        public event Action OnTutorialCompleted;

        public static void SetInstance(TutorialManager instance) => _instance = instance;

        public void Reset()
        {
            CurrentStep = TutorialStep.TapToAttack;
            TapCount = 0;
        }

        public void SetStep(TutorialStep step)
        {
            if (CurrentStep == step) return;
            CurrentStep = step;
            OnTutorialStepChanged?.Invoke(CurrentStep);
            if (step == TutorialStep.Completed)
            {
                OnTutorialCompleted?.Invoke();
            }
        }

        public void RegisterTap()
        {
            if (CurrentStep != TutorialStep.TapToAttack) return;
            TapCount++;
            if (TapCount >= RequiredTapCount)
            {
                SetStep(TutorialStep.UpgradeAttack);
            }
        }

        public void RecordEnemyDefeated()
        {
            if (CurrentStep == TutorialStep.TapToAttack)
            {
                SetStep(TutorialStep.UpgradeAttack);
            }
        }

        public void CheckGoldProgress(double currentGold)
        {
            // Gold check helper if needed for dynamic cueing
        }

        public void RecordStatUpgraded()
        {
            if (CurrentStep == TutorialStep.UpgradeAttack)
            {
                SetStep(TutorialStep.ActivateBerserk);
            }
        }

        public void CheckRageProgress(bool isRageActive)
        {
            if (CurrentStep == TutorialStep.ActivateBerserk && isRageActive)
            {
                SetStep(TutorialStep.VisitManorGate);
            }
        }

        public void RecordGateVisited()
        {
            if (CurrentStep == TutorialStep.VisitManorGate)
            {
                SetStep(TutorialStep.Completed);
            }
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~TutorialSystemTests"`
Expected: PASS (6 passed).

- [ ] **Step 5: Commit Task 1**

```bash
git add Scripts/Core/TutorialModels.cs Scripts/Core/TutorialManager.cs Tests/TutorialSystemTests.cs
git commit -m "feat: add TutorialManager and TutorialStep domain models with unit tests"
```

---

### Task 2: Persistence Integration in SaveData and SaveSystem

**Files:**
- Modify: `Scripts/Core/SaveData.cs`
- Modify: `Scripts/Core/SaveSystem.cs`
- Modify: `Tests/TutorialSystemTests.cs`

**Interfaces:**
- Consumes: `TutorialManager.Instance`, `SaveData`, `SaveSystem`
- Produces:
  - `SaveData.TutorialStep`: integer representation of tutorial progress.
  - `SaveSystem.CaptureSaveData`: serializes `TutorialManager.Instance.CurrentStep`.
  - `SaveSystem.ApplySaveData`: deserializes and applies to `TutorialManager.Instance`.

- [ ] **Step 1: Write the failing test for tutorial persistence**

Add to `Tests/TutorialSystemTests.cs`:
```csharp
[Fact]
public void SaveAndApply_PreservesTutorialStep()
{
    var mgr = TutorialManager.Instance;
    mgr.SetStep(TutorialStep.ActivateBerserk);

    var data = SaveSystem.CaptureSaveData(null);
    Assert.Equal((int)TutorialStep.ActivateBerserk, data.TutorialStep);

    mgr.Reset();
    Assert.Equal(TutorialStep.TapToAttack, mgr.CurrentStep);

    SaveSystem.ApplySaveData(data, null);
    Assert.Equal(TutorialStep.ActivateBerserk, mgr.CurrentStep);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Tests/BloodSeal.Tests.csproj --filter "FullyQualifiedName~SaveAndApply_PreservesTutorialStep"`
Expected: FAIL (compilation error, `data.TutorialStep` does not exist).

- [ ] **Step 3: Update SaveData.cs and SaveSystem.cs**

In `Scripts/Core/SaveData.cs`, add property:
```csharp
public int TutorialStep { get; set; } = 1; // Defaults to TapToAttack for new games
```

In `Scripts/Core/SaveSystem.cs`:
In `CaptureSaveData(GameManager gm)`:
```csharp
data.TutorialStep = (int)(TutorialManager.Instance?.CurrentStep ?? TutorialStep.TapToAttack);
```
In `ApplySaveData(SaveData data, GameManager gm)`:
```csharp
var tut = TutorialManager.Instance;
if (tut != null)
{
    tut.SetStep(data.TutorialStep <= 0 ? TutorialStep.TapToAttack : (TutorialStep)data.TutorialStep);
}
```
Verify `SaveSystem.cs` line count remains under 250 lines.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Expected: PASS (All tests pass including SaveSystem and Tutorial tests).

- [ ] **Step 5: Commit Task 2**

```bash
git add Scripts/Core/SaveData.cs Scripts/Core/SaveSystem.cs Tests/TutorialSystemTests.cs
git commit -m "feat: persist tutorial progression in SaveData and SaveSystem"
```

---

### Task 3: Non-Intrusive Gothic UI Hint Callout Component

**Files:**
- Create: `Scripts/UI/TutorialHintCallout.cs`

**Interfaces:**
- Consumes: Godot 4 `Control`, `Label`, `PanelContainer`, `Tween`
- Produces:
  - `public partial class TutorialHintCallout : Control`
  - `public void ShowHint(string title, string message, Vector2 targetScreenPos, bool pointUp = true)`
  - `public void HideHint()`
  - `MouseFilter = MouseFilterEnum.Ignore` on self and all children.

- [ ] **Step 1: Write TutorialHintCallout.cs**

Create `Scripts/UI/TutorialHintCallout.cs`:
```csharp
using Godot;

namespace BloodSeal.UI
{
    public partial class TutorialHintCallout : Control
    {
        private PanelContainer _container;
        private Label _titleLabel;
        private Label _messageLabel;
        private Label _arrowLabel;
        private Tween _pulseTween;
        private Tween _fadeTween;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            BuildUI();
            Modulate = new Color(1, 1, 1, 0);
            Visible = false;
        }

        private void BuildUI()
        {
            _container = new PanelContainer
            {
                MouseFilter = MouseFilterEnum.Ignore,
                CustomMinimumSize = new Vector2(260, 80)
            };

            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.04f, 0.05f, 0.92f),
                BorderColor = new Color(0.78f, 0.15f, 0.20f, 0.95f),
                CornerRadiusBottomLeft = 6,
                CornerRadiusBottomRight = 6,
                CornerRadiusTopLeft = 6,
                CornerRadiusTopRight = 6,
                BorderWidthBottom = 2,
                BorderWidthTop = 2,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                ContentMarginBottom = 10,
                ContentMarginTop = 10,
                ContentMarginLeft = 14,
                ContentMarginRight = 14
            };
            _container.AddThemeStyleboxOverride("panel", style);

            var vbox = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            _container.AddChild(vbox);

            _titleLabel = new Label
            {
                MouseFilter = MouseFilterEnum.Ignore,
                Text = "ÖĞRETİCİ"
            };
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.45f));
            _titleLabel.AddThemeFontSizeOverride("font_size", 14);
            vbox.AddChild(_titleLabel);

            _messageLabel = new Label
            {
                MouseFilter = MouseFilterEnum.Ignore,
                Text = "",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _messageLabel.AddThemeColorOverride("font_color", new Color(0.92f, 0.92f, 0.92f));
            _messageLabel.AddThemeFontSizeOverride("font_size", 13);
            vbox.AddChild(_messageLabel);

            _arrowLabel = new Label
            {
                MouseFilter = MouseFilterEnum.Ignore,
                Text = "▼",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _arrowLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.25f, 0.25f));
            _arrowLabel.AddThemeFontSizeOverride("font_size", 22);

            AddChild(_container);
            AddChild(_arrowLabel);
        }

        public void ShowHint(string title, string message, Vector2 targetScreenPos, bool pointUp = false)
        {
            _titleLabel.Text = title;
            _messageLabel.Text = message;

            _arrowLabel.Text = pointUp ? "▲" : "▼";

            Vector2 boxSize = _container.GetCombinedMinimumSize();
            if (pointUp)
            {
                _container.Position = new Vector2(targetScreenPos.X - boxSize.X * 0.5f, targetScreenPos.Y + 28);
                _arrowLabel.Position = new Vector2(targetScreenPos.X - 10, targetScreenPos.Y + 4);
            }
            else
            {
                _container.Position = new Vector2(targetScreenPos.X - boxSize.X * 0.5f, targetScreenPos.Y - boxSize.Y - 28);
                _arrowLabel.Position = new Vector2(targetScreenPos.X - 10, targetScreenPos.Y - 26);
            }

            Visible = true;
            _fadeTween?.Kill();
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 1.0f, 0.25f);

            StartPulse();
        }

        private void StartPulse()
        {
            _pulseTween?.Kill();
            _pulseTween = CreateTween().SetLoops();
            _pulseTween.TweenProperty(_arrowLabel, "scale", new Vector2(1.2f, 1.2f), 0.45f).SetTrans(Tween.TransitionType.Sine);
            _pulseTween.TweenProperty(_arrowLabel, "scale", Vector2.One, 0.45f).SetTrans(Tween.TransitionType.Sine);
        }

        public void HideHint()
        {
            _pulseTween?.Kill();
            _fadeTween?.Kill();
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
            _fadeTween.Finished += () => Visible = false;
        }
    }
}
```

- [ ] **Step 2: Verify compilation and line count**

Run: `dotnet build`
Expected: 0 errors, 0 warnings. `TutorialHintCallout.cs` is ~110 lines.

- [ ] **Step 3: Commit Task 3**

```bash
git add Scripts/UI/TutorialHintCallout.cs
git commit -m "feat: add TutorialHintCallout non-intrusive UI component"
```

---

### Task 4: Decoupled Tutorial Scene Controller & Tap Integration

**Files:**
- Create: `Scripts/UI/TutorialController.cs`
- Modify: `Scripts/Combat/TapCombatArea.cs`
- Modify: `Scripts/Combat/Enemy.cs`
- Modify: `Scenes/MainCombat.tscn`

**Interfaces:**
- Consumes: `TutorialManager`, `GameManager`, `MainHUD`, `TapCombatArea`
- Produces:
  - `TutorialController` listens to events and positions `TutorialHintCallout`.
  - `TapCombatArea` triggers `TutorialManager.Instance.RegisterTap()`.
  - `Enemy.Die()` triggers `TutorialManager.Instance.RecordEnemyDefeated()`.

- [ ] **Step 1: Update TapCombatArea.cs to inform TutorialManager**

In `Scripts/Combat/TapCombatArea.cs`, in `OnTap`:
```csharp
TutorialManager.Instance?.RegisterTap();
```
Line count check: remains ~74 lines (well under 250).

- [ ] **Step 2: Update Enemy.cs to inform TutorialManager**

In `Scripts/Combat/Enemy.cs`, in `Die()`:
```csharp
TutorialManager.Instance?.RecordEnemyDefeated();
```
Line count check: remains ~206 lines (well under 250).

- [ ] **Step 3: Implement TutorialController.cs**

Create `Scripts/UI/TutorialController.cs`:
```csharp
using Godot;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class TutorialController : CanvasLayer
    {
        private TutorialHintCallout _callout;

        public override void _Ready()
        {
            Layer = 105; // Placed above standard HUD
            _callout = new TutorialHintCallout();
            AddChild(_callout);

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnTutorialStepChanged += OnStepChanged;
                TutorialManager.Instance.OnTutorialCompleted += OnTutorialCompleted;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsUpgraded += OnStatsUpgraded;
                GameManager.Instance.OnRageStateChanged += OnRageStateChanged;
                GameManager.Instance.OnGateNotificationAvailable += OnGateAvailable;
            }

            // Defer step update to ensure HUD nodes have initialized positions
            Callable.From(() => RefreshCurrentStepUI()).CallDeferred();
        }

        public override void _ExitTree()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnTutorialStepChanged -= OnStepChanged;
                TutorialManager.Instance.OnTutorialCompleted -= OnTutorialCompleted;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsUpgraded -= OnStatsUpgraded;
                GameManager.Instance.OnRageStateChanged -= OnRageStateChanged;
                GameManager.Instance.OnGateNotificationAvailable -= OnGateAvailable;
            }
        }

        private void OnStepChanged(TutorialStep step) => RefreshCurrentStepUI();
        private void OnTutorialCompleted() => _callout?.HideHint();

        private void OnStatsUpgraded()
        {
            if (TutorialManager.Instance?.CurrentStep == TutorialStep.UpgradeAttack)
            {
                TutorialManager.Instance.RecordStatUpgraded();
            }
        }

        private void OnRageStateChanged(bool isActive)
        {
            if (TutorialManager.Instance?.CurrentStep == TutorialStep.ActivateBerserk && isActive)
            {
                TutorialManager.Instance.CheckRageProgress(true);
            }
        }

        private void OnGateAvailable()
        {
            if (TutorialManager.Instance?.CurrentStep == TutorialStep.VisitManorGate)
            {
                RefreshCurrentStepUI();
            }
        }

        private void RefreshCurrentStepUI()
        {
            if (TutorialManager.Instance == null || _callout == null) return;
            var step = TutorialManager.Instance.CurrentStep;

            switch (step)
            {
                case TutorialStep.TapToAttack:
                    _callout.ShowHint("DÖVÜŞ", "Düşmanlara vurmak ve öfke toplamak için savaş alanına dokun!", new Vector2(960, 520), pointUp: false);
                    break;

                case TutorialStep.UpgradeAttack:
                    var atkBtn = GetTree().Root.FindChild("UpgradeAtkBtn", true, false) as Button;
                    Vector2 atkPos = atkBtn != null && atkBtn.IsVisibleInTree()
                        ? atkBtn.GlobalPosition + atkBtn.Size * 0.5f
                        : new Vector2(350, 950);
                    _callout.ShowHint("GELİŞİM", "Saldırı gücünü artırmak için Saldırı (ATK) yükselt!", atkPos, pointUp: false);
                    break;

                case TutorialStep.ActivateBerserk:
                    var rageBtn = GetTree().Root.FindChild("RageButton", true, false) as Button;
                    Vector2 ragePos = rageBtn != null && rageBtn.IsVisibleInTree()
                        ? rageBtn.GlobalPosition + rageBtn.Size * 0.5f
                        : new Vector2(960, 850);
                    _callout.ShowHint("GÜÇ PATLAMASI", "Öfken doldu! Berserk modunu serbest bırak!", ragePos, pointUp: false);
                    break;

                case TutorialStep.VisitManorGate:
                    var gateBtn = GetTree().Root.FindChild("GateNotificationBtn", true, false) as Button;
                    Vector2 gatePos = gateBtn != null && gateBtn.IsVisibleInTree()
                        ? gateBtn.GlobalPosition + gateBtn.Size * 0.5f
                        : new Vector2(960, 180);
                    _callout.ShowHint("KEŞİF", "Malikane Kapısı belirdi! Mührünü seç ve ödülünü al!", gatePos, pointUp: true);
                    break;

                case TutorialStep.Completed:
                default:
                    _callout.HideHint();
                    break;
            }
        }
    }
}
```

- [ ] **Step 4: Attach TutorialController into Scenes/MainCombat.tscn**

In `Scenes/MainCombat.tscn`:
Add `ExtResource` for `TutorialController.cs` and add child node `[node name="TutorialController" type="CanvasLayer" parent="."] script = ExtResource("...")`.

- [ ] **Step 5: Run tests and build**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Run: `dotnet build`
Expected: 0 warnings, 0 errors.

- [ ] **Step 6: Commit Task 4**

```bash
git add Scripts/UI/TutorialController.cs Scripts/Combat/TapCombatArea.cs Scripts/Combat/Enemy.cs Scenes/MainCombat.tscn
git commit -m "feat: integrate decoupled TutorialController and TapCombatArea progression"
```

---

### Task 5: End-to-End Verification and Polish

**Files:**
- Test: All tests in `Tests/`

- [ ] **Step 1: Run comprehensive xUnit test suite**

Run: `dotnet test Tests/BloodSeal.Tests.csproj`
Expected: 100% tests pass.

- [ ] **Step 2: Run Godot headless smoke test**

Run: `& "C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe" --headless --quit-after 60`
Expected: Exits cleanly with code 0, no missing resource errors, no null reference crashes.

- [ ] **Step 3: Verify line count constraints**

Verify all files <= 250 lines (`MainHUD.cs` 248 lines, `GameManager.cs` 248 lines, `SaveSystem.cs` <= 250 lines).

- [ ] **Step 4: Update HANDOVER.md**

Reflect that Step 3 (FTUE / Tutorial) is complete and specify the next steps.
