# BloodSeal Combat Juice & VFX Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate a high-impact screen shake, slash arc VFX, directional blood splatters, death explosion particles, tap shockwaves, and critical hit-freeze to maximize the dark gothic combat feel.

**Architecture:** Centralized `FXManager` singleton for spawning, pooling, and managing visual effects and micro hit-freezes, paired with a trauma-decay `CameraShake` component attached to the active `Camera2D`.

**Tech Stack:** Godot 4.7.2 Mono (.NET 10 / C#), Compatibility Renderer, 1920x1080.

## Global Constraints
- Target platform: 1920x1080, .NET 10.
- All code in C#.
- Do not break existing gameplay loop or autoload configs.

---

### Task 1: Camera Shake Component

**Files:**
- Create: `Scripts/Combat/CameraShake.cs`
- Modify: `Scenes/MainCombat.tscn`

- [ ] **Step 1: Create `CameraShake.cs`**

```csharp
using Godot;

namespace BloodSeal.Combat
{
    public partial class CameraShake : Camera2D
    {
        public static CameraShake Instance { get; private set; }

        [Export] public float DecayRate = 1.8f;
        [Export] public float MaxOffset = 20f;

        private float _trauma = 0f;
        private Vector2 _initialPosition;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            _initialPosition = Position;
        }

        public override void _Process(double delta)
        {
            if (_trauma > 0f)
            {
                _trauma = Mathf.Max(0f, _trauma - DecayRate * (float)delta);
                float shakeAmount = _trauma * _trauma;
                float offsetX = (float)GD.RandRange(-1.0, 1.0) * MaxOffset * shakeAmount;
                float offsetY = (float)GD.RandRange(-1.0, 1.0) * MaxOffset * shakeAmount;
                Position = _initialPosition + new Vector2(offsetX, offsetY);
            }
            else
            {
                Position = _initialPosition;
            }
        }

        public void AddTrauma(float amount)
        {
            _trauma = Mathf.Clamp(_trauma + amount, 0f, 1f);
        }
    }
}
```

- [ ] **Step 2: Add Camera2D with `CameraShake.cs` to `Scenes/MainCombat.tscn` at (960, 540)**
- [ ] **Step 3: Run `dotnet build`**
- [ ] **Step 4: Commit**

---

### Task 2: VFX Prefabs & FXManager

**Files:**
- Create: `Scenes/VFX/SlashVfx.tscn`
- Create: `Scenes/VFX/BloodSplatter.tscn`
- Create: `Scenes/VFX/DeathExplosion.tscn`
- Create: `Scenes/VFX/TapRipple.tscn`
- Create: `Scripts/Combat/FXManager.cs`

- [ ] **Step 1: Create `SlashVfx.tscn` with stylized curved crimson blade polygon**
- [ ] **Step 2: Create `BloodSplatter.tscn` with directional CPUParticles2D blood droplets**
- [ ] **Step 3: Create `DeathExplosion.tscn` with radial blood burst and dark smoke**
- [ ] **Step 4: Create `TapRipple.tscn` with expanding red ring and blood sparks**
- [ ] **Step 5: Create `FXManager.cs`**

```csharp
using Godot;

namespace BloodSeal.Combat
{
    public partial class FXManager : Node2D
    {
        public static FXManager Instance { get; private set; }

        [Export] public PackedScene SlashScene;
        [Export] public PackedScene BloodSplatterScene;
        [Export] public PackedScene DeathExplosionScene;
        [Export] public PackedScene TapRippleScene;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public void PlaySlash(Vector2 pos, bool isBerserk)
        {
            if (SlashScene == null) return;
            var slash = SlashScene.Instantiate<Node2D>();
            slash.GlobalPosition = pos;
            if (isBerserk)
            {
                slash.Scale = new Vector2(1.6f, 1.6f);
                slash.Modulate = new Color(1.5f, 0.4f, 0.2f);
            }
            AddChild(slash);
        }

        public void PlayBloodSplatter(Vector2 pos, Vector2 direction)
        {
            if (BloodSplatterScene == null) return;
            var blood = BloodSplatterScene.Instantiate<Node2D>();
            blood.GlobalPosition = pos;
            blood.Rotation = direction.Angle();
            AddChild(blood);
        }

        public void PlayDeathExplosion(Vector2 pos, bool isBoss)
        {
            if (DeathExplosionScene == null) return;
            var expl = DeathExplosionScene.Instantiate<Node2D>();
            expl.GlobalPosition = pos;
            if (isBoss) expl.Scale = new Vector2(2.4f, 2.4f);
            AddChild(expl);
        }

        public void PlayTapRipple(Vector2 pos)
        {
            if (TapRippleScene == null) return;
            var rip = TapRippleScene.Instantiate<Node2D>();
            rip.GlobalPosition = pos;
            AddChild(rip);
        }

        public async void TriggerHitFreeze(float duration = 0.045f)
        {
            Engine.TimeScale = 0.05;
            await ToSignal(GetTree().CreateTimer(duration * 0.05, true, false, true), "timeout");
            Engine.TimeScale = 1.0;
        }
    }
}
```

- [ ] **Step 6: Run `dotnet build`**
- [ ] **Step 7: Commit**

---

### Task 3: Combat Integration & Verification

**Files:**
- Modify: `Scripts/Combat/Hero.cs`
- Modify: `Scripts/Combat/Enemy.cs`
- Modify: `Scripts/Combat/TapCombatArea.cs`
- Modify: `Scenes/MainCombat.tscn`

- [ ] **Step 1: Wire `Hero.cs` to trigger `FXManager.PlaySlash`, `CameraShake.AddTrauma`, and `TriggerHitFreeze` on attack**
- [ ] **Step 2: Wire `Enemy.cs` to trigger `FXManager.PlayBloodSplatter` on hit and `PlayDeathExplosion` on death**
- [ ] **Step 3: Wire `TapCombatArea.cs` to trigger `FXManager.PlayTapRipple` and `CameraShake.AddTrauma(0.08f)`**
- [ ] **Step 4: Add `FXManager` to `Scenes/MainCombat.tscn` with prefab references configured**
- [ ] **Step 5: Run `dotnet build` and headless verification**
- [ ] **Step 6: Commit and Push**
