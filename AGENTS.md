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

## 3. Idle RPG Economy & Math Standards (GDC Anthony Pecorella)

### A. Progression Growth Formulas
- **Upgrade Cost (Exponential):**
  $$Cost = BaseCost \times GrowthFactor^{(Level - 1)}$$
  - Standard base stats (ATK, Max HP, Range): `GrowthFactor = 1.15`
  - High-impact scaling stats (ATK Speed, Lifesteal): `GrowthFactor = 1.18 - 1.22`
- **Enemy Health & Damage Scaling (Linear-Polynomial Waves):**
  - Minion HP: $Wave \times 25 + 50$
  - Minion ATK: $Wave \times 3 + 5$
  - Boss HP: $Wave \times 220 + 450$
  - Boss ATK: $Wave \times 14 + 25$
- **Boss Enrage Mechanic:**
  - Savaş uzadıkça Boss her 5 saniyede bir $+25\%$ çarpanla güçlenir ($EnrageMultiplier \times 1.25$).

### B. Big Number & Currency Hygiene
- Use `long` or `double` for currency and damage calculations to prevent 32-bit integer overflow.
- All UI representations must use human-readable standard abbreviations:
  - `< 1,000`: Exact number (`842`)
  - `>= 1,000`: `K` (`1.2K`)
  - `>= 1,000,000`: `M` (`4.5M`)
  - `>= 1,000,000,000`: `B` (`12.8B`)
  - `>= 1,000,000,000,000`: `T` (`3.1T`)

### C. Offline Progress Calculation
- Save timestamps using `DateTimeOffset.UtcNow.ToUnixTimeSeconds()`.
- Hard-cap offline progress time to **6 hours** (21,600 seconds) to maintain retention and economy balance.
- Calculate offline gold based on the highest stable cleared wave earnings per second.

---

## 4. Combat Juice & Game Feel (GDC "Juice It or Lose It")

Every action must generate sensory feedback:
1. **Screen Shake:** Managed via trauma-decay formula:
   $$Offset = Random(-1, 1) \times MaxOffset \times Trauma^2$$
   - Light (tap / standard hit): $+0.08 - 0.12$ trauma
   - Heavy (crit / berserk hit / player damage): $+0.25 - 0.35$ trauma
2. **Hitstop / Micro Freeze:** Critical hits trigger a 40–50ms freeze (`Engine.TimeScale = 0.05`) to give weight to slashing impacts.
3. **Slash Arc VFX:** Slashing swords must generate directional crescent light arcs that scale during Berserk mode.
4. **Independent Particle Lifecycles:** Particles (blood splatter, death explosion) must be spawned under `FXManager` or the scene root so they do not prematurely vanish when the dying enemy calls `QueueFree()`.

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
