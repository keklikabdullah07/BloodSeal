# 🩸 BloodSeal: Professional Game Architecture & AI Developer Guide

This document establishes the binding architectural standards, game design patterns, and engineering rules for **BloodSeal**. Any human engineer or AI coding assistant (Antigravity, Cursor, Claude Code, Copilot) working on this repository **MUST** strictly adhere to these guidelines.

---

## 1. Tech Stack & Environment Foundations
- **Engine:** Godot 4.7.x Mono (C#)
- **Target Runtime:** .NET 10.0 (`net10.0`, C# 12+)
- **Renderer:** Compatibility (GL Compatibility / OpenGL 3)
- **Resolution & Aspect:** 1920x1080 (16:9 Landscape), `stretch/mode = "canvas_items"`, `stretch/aspect = "expand"`
- **External CLI:** `C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe`

---

## 2. Core Architectural Principles (Chickensoft & Industry Standards)

### A. "Call Down, Signal Up" (Decoupling Rule)
- **Downwards Communication (Parent to Child):** Parents hold direct references to their children and may call their public methods directly (e.g. `Hero` calls `TargetEnemy.TakeDamage()`).
- **Upwards Communication (Child to Parent):** Children **NEVER** traverse up the scene tree with fragile paths like `GetParent().GetParent()`. Instead, children emit **C# Events** (`event Action`) or Godot Signals (`[Signal]`).
- **Global Communication:** For cross-cutting concerns (currencies, wave advancement, death events), use the Autoload `GameManager.Instance` event bus.

### B. Composition Over Inheritance (No God Objects)
- **File Length Limit:** Any script approaching **250 lines** must be refactored into focused components (e.g., `HealthComponent`, `CombatVfxComponent`, `MovementComponent`).
- Prefer scene composition over deep inheritance trees. Inherit from Godot nodes (`CharacterBody2D`, `Node2D`, `Control`) only for structural needs; encapsulate gameplay behaviors into reusable components.

### C. Node Reference Safety
- Do not use brittle hardcoded string paths like `GetNode("../../../SomeNode")`.
- Use `[Export]` node references or `GetNodeOrNull<T>("ChildName")` in `_Ready()`.
- Always check `IsInstanceValid(node)` before operating on dynamically spawned actors or targets.

---

## 3. Idle RPG Economy & Math Standards

### A. Progression Growth Formulas
- **Upgrade Cost (Exponential):**
  $$Cost = BaseCost \times 1.15^{(Level - 1)}$$
  *(Stat-specific scaling multipliers can be tuned via configuration resources).*
- **Enemy Health & Damage Scaling (Linear-Polynomial Waves):**
  - Minion HP: $Wave \times 25 + 50$
  - Minion ATK: $Wave \times 3 + 5$
  - Boss HP: $Wave \times 220 + 450$
  - Boss ATK: $Wave \times 14 + 25$
- **Boss Enrage Mechanic:**
  - Savaş uzadıkça Boss her 5 saniyede bir taban hasarın $+25\%$ çarpan adımıyla güçlenir ($BossDamage = BaseBossDamage \times (1.0 + 0.25 \times EnrageAdimi)$). Formül `Data/BalanceConfig.json` içindeki `bossEnrage.isMultiplicative` ile belirlenir (varsayılan: toplamsal, 60. saniyede 4.0x çarpan).

### B. Big Number & Currency Hygiene
- Use `long` or `double` for currency and damage calculations to prevent 32-bit integer overflow.
- All UI representations must use human-readable standard abbreviations:
  - `< 1,000`: Exact number (`842`)
  - `>= 1,000`: `K` (`1.2K`)
  - `>= 1,000,000`: `M` (`4.5M`)
  - `>= 1,000,000,000`: `B` (`12.8B`)
  - `>= 1,000,000,000,000`: `T` (`3.1T`)

### C. Offline Progress Calculation & Anti-Cheat
- Save timestamps using `DateTimeOffset.UtcNow.ToUnixTimeSeconds()`.
- Clamp elapsed time between 0 and 21,600 seconds to protect against clock manipulation and cap at 6 hours:
  $$ElapsedSeconds = \operatorname{Math.Clamp}(Now - SavedTime, 0, 21600)$$
- Calculate offline gold based on the highest stable cleared wave earnings per second.

---

## 4. Game Feel Kuralları (Bağlayıcı)

- **Engine.TimeScale'e ASLA dokunma.** Hit-freeze yalnızca vuran ve vurulan aktörün animasyonu/tween'ini ~40 ms durdurur (yerel).
- **Hit-freeze sadece boss kritiği ve boss ölümünde**, en az 0.4 sn aralıkla çalışır. Berserk modunda kesinlikle kapalıdır.
- **Camera trauma 0..1 arasında Clamp edilir**, sabit sönüm hızı vardır (başlangıç: 1.5/sn).
  - Başlangıç değerleri: normal vuruş 0.10, kritik 0.20 (Berserk dışı), alınan hasar 0.25, boss ölümü 0.50.
  - Berserk'te vuruş başına shake yoktur; sabit taban 0.15 trauma uygulanır.
- **Enrage (5 sn), Berserk (10 sn), gelir ve offline sayaçları gerçek delta ile ilerler.**
  - Hiçbir efekt bu sayaçları durduramaz veya yavaşlatamaz. Gelir hesabı için `Timer` node'u kullanma.
- **Slash Arc VFX & Bağımsız Parçacıklar:** Kılıç savurmalarında hilal ışık efekti; parçacıklar `FXManager` altına doğarak aktör `QueueFree` olsa bile yaşam döngüsünü tamamlar.

---

## 5. Coding Style & C# Conventions

```csharp
namespace BloodSeal.Subsystem
{
    // 1. Classes, Interfaces, Enums, Structs: PascalCase
    public partial class CombatActor : CharacterBody2D
    {
        // 2. Public Properties and Events: PascalCase
        public float CurrentHp { get; private set; }
        public event Action<float> OnDied;

        // 3. Exported Godot Fields: PascalCase
        [Export] public float MoveSpeed = 150f;

        // 4. Private / Protected Fields: _camelCase
        private double _attackTimer = 0.0;
        private Node2D _visualRoot;

        // 5. Methods: PascalCase, Local Variables: camelCase
        public void TakeDamage(float damageAmount)
        {
            float finalDamage = Mathf.Max(1f, damageAmount);
            CurrentHp -= finalDamage;
        }
    }
}
```

---

## 6. Build, Verification & Testing Workflow

Before committing any change:
1. **Compile:** Run `dotnet build` from repository root. Must succeed with **0 warnings and 0 errors**.
2. **Headless Smoke Test:** Run Godot console with `--headless --quit-after 60` to ensure no scene-tree crashes or missing resource links.
3. **Commit Cleanly:** Use Conventional Commits (`feat:`, `fix:`, `refactor:`, `docs:`, `chore:`).
