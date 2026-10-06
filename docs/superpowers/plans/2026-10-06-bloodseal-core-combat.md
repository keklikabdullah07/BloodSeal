# BloodSeal Core Combat Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a fully playable 2D Gothic Idle RPG vertical slice in Godot 4 (.NET / C#) featuring hero combat, 2 hovering pet companions, wave progression, a Boss with progressive enrage, safe farm retreat upon defeat, tap damage, rage mode, and a responsive Pentagram upgrade HUD.

**Architecture:** Event-driven architecture with an Autoload `GameManager` singleton managing currencies, waves, and global states. Slices are decoupled into modular C# components (`Hero`, `PetCompanion`, `Enemy`, `BossEnemy`, `WaveSpawner`, `MainHUD`), communicating through C# events and Godot scene tree structures.

**Tech Stack:** Godot 4.7.2 Mono (C# / .NET 10.0), Compatibility Renderer (OpenGL 3), 1920x1080 canvas_items stretch mode.

## Global Constraints
- Target platform resolution: 1920x1080 (16:9 Landscape), `stretch/mode="canvas_items"`, `stretch/aspect="expand"`.
- 100% C# scripts for game logic.
- Compatibility Renderer (GL Compatibility).
- Godot executable: `C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe`.

---

### Task 1: Core State & Pentagram Models

**Files:**
- Create: `Scripts/Core/GameManager.cs`
- Create: `Scripts/Combat/PentagramStats.cs`
- Create: `Scripts/Core/SaveData.cs`
- Modify: `project.godot`

**Interfaces:**
- Produces: `GameManager.Instance`, `GameManager.OnGoldChanged`, `GameManager.OnWaveChanged`, `GameManager.OnRageChanged`, `GameManager.OnRageStateChanged`, `GameManager.OnHeroDied`
- Produces: `PentagramStats` (ATK, ATKSpeed, Lifesteal, MaxHP, Range, and upgrade methods `UpgradeAtk()`, `UpgradeAtkSpeed()`, `UpgradeLifesteal()`, `UpgradeMaxHp()`, `UpgradeRange()`)

- [ ] **Step 1: Create `PentagramStats.cs` data and scaling model**

```csharp
using System;

namespace BloodSeal.Combat
{
    public class PentagramStats
    {
        public int AtkLevel { get; set; } = 1;
        public int AtkSpeedLevel { get; set; } = 1;
        public int LifestealLevel { get; set; } = 1;
        public int MaxHpLevel { get; set; } = 1;
        public int RangeLevel { get; set; } = 1;

        // Stat Calculations
        public float Atk => 10f + (AtkLevel - 1) * 3f;
        public float AtkSpeed => Math.Min(3.5f, 1.0f + (AtkSpeedLevel - 1) * 0.05f);
        public float LifestealPercent => Math.Min(25f, 1.0f + (LifestealLevel - 1) * 0.5f);
        public float MaxHp => 100f + (MaxHpLevel - 1) * 25f;
        public float Range => Math.Min(350f, 180f + (RangeLevel - 1) * 10f);

        // Upgrade Costs (Base * 1.15^(level-1))
        public long GetAtkCost() => (long)(20 * Math.Pow(1.15, AtkLevel - 1));
        public long GetAtkSpeedCost() => (long)(30 * Math.Pow(1.18, AtkSpeedLevel - 1));
        public long GetLifestealCost() => (long)(40 * Math.Pow(1.22, LifestealLevel - 1));
        public long GetMaxHpCost() => (long)(25 * Math.Pow(1.15, MaxHpLevel - 1));
        public long GetRangeCost() => (long)(20 * Math.Pow(1.14, RangeLevel - 1));
    }
}
```

- [ ] **Step 2: Create `SaveData.cs`**

```csharp
namespace BloodSeal.Core
{
    public class SaveData
    {
        public long Gold { get; set; } = 0;
        public int CurrentWave { get; set; } = 1;
        public int HighestWave { get; set; } = 1;
        public int AtkLevel { get; set; } = 1;
        public int AtkSpeedLevel { get; set; } = 1;
        public int LifestealLevel { get; set; } = 1;
        public int MaxHpLevel { get; set; } = 1;
        public int RangeLevel { get; set; } = 1;
    }
}
```

- [ ] **Step 3: Create `GameManager.cs`**

```csharp
using Godot;
using System;
using BloodSeal.Combat;

namespace BloodSeal.Core
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        public PentagramStats Stats { get; private set; } = new PentagramStats();

        public long Gold { get; private set; } = 100; // Starter gold for immediate test
        public int CurrentWave { get; private set; } = 1;
        public int HighestWave { get; private set; } = 1;
        public bool IsInSafeFarmMode { get; private set; } = false;
        public float RagePercentage { get; private set; } = 0f;
        public bool IsRageActive { get; private set; } = false;

        public event Action<long> OnGoldChanged;
        public event Action<int, bool> OnWaveChanged;
        public event Action<float> OnRageChanged;
        public event Action<bool> OnRageStateChanged;
        public event Action OnHeroDied;
        public event Action OnStatsUpgraded;

        private double _rageActiveTimer = 0.0;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public override void _Process(double delta)
        {
            if (IsRageActive)
            {
                _rageActiveTimer -= delta;
                if (_rageActiveTimer <= 0)
                {
                    IsRageActive = false;
                    RagePercentage = 0f;
                    OnRageStateChanged?.Invoke(false);
                    OnRageChanged?.Invoke(0f);
                }
            }
        }

        public void AddGold(long amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool SpendGold(long amount)
        {
            if (Gold >= amount)
            {
                Gold -= amount;
                OnGoldChanged?.Invoke(Gold);
                return true;
            }
            return false;
        }

        public void AddRage(float amount)
        {
            if (IsRageActive) return;
            RagePercentage = Mathf.Clamp(RagePercentage + amount, 0f, 100f);
            OnRageChanged?.Invoke(RagePercentage);
        }

        public bool TriggerRage()
        {
            if (RagePercentage >= 100f && !IsRageActive)
            {
                IsRageActive = true;
                _rageActiveTimer = 10.0;
                OnRageStateChanged?.Invoke(true);
                return true;
            }
            return false;
        }

        public void SetWave(int wave, bool isSafeFarm = false)
        {
            CurrentWave = wave;
            IsInSafeFarmMode = isSafeFarm;
            if (wave > HighestWave) HighestWave = wave;
            bool isBoss = (wave % 10 == 0);
            OnWaveChanged?.Invoke(CurrentWave, isBoss);
        }

        public void AdvanceWave()
        {
            SetWave(CurrentWave + 1, false);
        }

        public void NotifyHeroDied()
        {
            OnHeroDied?.Invoke();
            // Retreat to wave 9 or previous safe wave if defeated
            int retreatWave = Math.Max(1, CurrentWave - 1);
            SetWave(retreatWave, true);
        }

        public void RetryBoss()
        {
            if (IsInSafeFarmMode)
            {
                int bossWave = ((CurrentWave / 10) + 1) * 10;
                SetWave(bossWave, false);
            }
        }

        public bool UpgradeAtk()
        {
            long cost = Stats.GetAtkCost();
            if (SpendGold(cost))
            {
                Stats.AtkLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeAtkSpeed()
        {
            long cost = Stats.GetAtkSpeedCost();
            if (SpendGold(cost))
            {
                Stats.AtkSpeedLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeLifesteal()
        {
            long cost = Stats.GetLifestealCost();
            if (SpendGold(cost))
            {
                Stats.LifestealLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeMaxHp()
        {
            long cost = Stats.GetMaxHpCost();
            if (SpendGold(cost))
            {
                Stats.MaxHpLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }

        public bool UpgradeRange()
        {
            long cost = Stats.GetRangeCost();
            if (SpendGold(cost))
            {
                Stats.RangeLevel++;
                OnStatsUpgraded?.Invoke();
                return true;
            }
            return false;
        }
    }
}
```

- [ ] **Step 4: Update `project.godot` to register Autoload for `GameManager` and set window stretch settings**

```ini
[autoload]

GameManager="*res://Scripts/Core/GameManager.cs"

[display]

window/size/viewport_width=1920
window/size/viewport_height=1080
window/size/mode=0
window/size/resizable=true
window/stretch/mode="canvas_items"
window/stretch/aspect="expand"
```

- [ ] **Step 5: Run `dotnet build` to verify compilation**

Run: `dotnet build`
Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 6: Commit**

```bash
git add project.godot Scripts/Core/ Scripts/Combat/PentagramStats.cs
git commit -m "feat: implement GameManager, PentagramStats, and autoload configuration"
```

---

### Task 2: Combat Actors - Hero & Pet Companions

**Files:**
- Create: `Scripts/Combat/Hero.cs`
- Create: `Scripts/Combat/PetCompanion.cs`
- Create: `Scripts/Combat/BloodProjectile.cs`
- Create: `Scenes/Hero.tscn`
- Create: `Scenes/PetCompanion.tscn`
- Create: `Scenes/Projectile.tscn`

**Interfaces:**
- Consumes: `GameManager.Instance.Stats`, `GameManager.Instance.AddRage()`, `GameManager.Instance.NotifyHeroDied()`
- Produces: `Hero.CurrentHp`, `Hero.MaxHp`, `Hero.TakeDamage(float)`, `Hero.GetAttackRange()`

- [ ] **Step 1: Create `BloodProjectile.cs`**

```csharp
using Godot;

namespace BloodSeal.Combat
{
    public partial class BloodProjectile : Node2D
    {
        [Export] public float Speed = 650f;
        public Node2D Target { get; set; }
        public float Damage { get; set; } = 5f;

        public override void _Process(double delta)
        {
            if (!IsInstanceValid(Target))
            {
                QueueFree();
                return;
            }

            Vector2 dir = (Target.GlobalPosition - GlobalPosition).Normalized();
            GlobalPosition += dir * Speed * (float)delta;

            if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 20f)
            {
                if (Target is Enemy enemy)
                {
                    enemy.TakeDamage(Damage, false);
                }
                QueueFree();
            }
        }
    }
}
```

- [ ] **Step 2: Create `PetCompanion.cs`**

```csharp
using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class PetCompanion : Node2D
    {
        [Export] public Vector2 BaseOffset = new Vector2(-70, -80);
        [Export] public float HoverPhase = 0f;
        [Export] public Color AuraColor = new Color(0.9f, 0.1f, 0.2f, 0.9f);
        [Export] public PackedScene ProjectileScene;

        private Node2D _hero;
        private double _shootTimer = 0.0;
        private double _timePassed = 0.0;

        public void Setup(Node2D hero)
        {
            _hero = hero;
        }

        public override void _Process(double delta)
        {
            _timePassed += delta;
            if (IsInstanceValid(_hero))
            {
                float hoverY = Mathf.Sin((float)_timePassed * 3.5f + HoverPhase) * 14f;
                GlobalPosition = _hero.GlobalPosition + BaseOffset + new Vector2(0, hoverY);
            }

            _shootTimer += delta;
            if (_shootTimer >= 1.4)
            {
                _shootTimer = 0.0;
                TryShoot();
            }
        }

        private void TryShoot()
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            Enemy nearest = null;
            float minDist = float.MaxValue;

            foreach (var node in enemies)
            {
                if (node is Enemy e && !e.IsDead)
                {
                    float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = e;
                    }
                }
            }

            if (nearest != null && ProjectileScene != null)
            {
                var proj = ProjectileScene.Instantiate<BloodProjectile>();
                proj.GlobalPosition = GlobalPosition;
                proj.Target = nearest;
                proj.Damage = GameManager.Instance.Stats.Atk * 0.4f;
                GetParent().AddChild(proj);
            }
        }
    }
}
```

- [ ] **Step 3: Create `Hero.cs`**

```csharp
using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class Hero : CharacterBody2D
    {
        public float CurrentHp { get; private set; }
        public float MaxHp => GameManager.Instance.Stats.MaxHp;

        public event Action<float, float> OnHealthChanged; // current, max

        private double _attackCooldown = 0.0;
        private Vector2 _originalLocalPos;
        private Node2D _visualRoot;

        public override void _Ready()
        {
            _originalLocalPos = Position;
            _visualRoot = GetNodeOrNull<Node2D>("VisualRoot");
            CurrentHp = MaxHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

            GameManager.Instance.OnStatsUpgraded += OnStatsChanged;
            GameManager.Instance.OnHeroDied += OnHeroRespawned;
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsUpgraded -= OnStatsChanged;
                GameManager.Instance.OnHeroDied -= OnHeroRespawned;
            }
        }

        private void OnStatsChanged()
        {
            if (CurrentHp > MaxHp) CurrentHp = MaxHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        private void OnHeroRespawned()
        {
            CurrentHp = MaxHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public override void _Process(double delta)
        {
            _attackCooldown -= delta;
            float currentAtkSpeed = GameManager.Instance.Stats.AtkSpeed;
            if (GameManager.Instance.IsRageActive) currentAtkSpeed *= 2f;

            float cooldownTime = 1f / currentAtkSpeed;

            if (_attackCooldown <= 0.0)
            {
                Enemy target = FindTargetInAttackRange();
                if (target != null)
                {
                    PerformAttack(target);
                    _attackCooldown = cooldownTime;
                }
            }
        }

        private Enemy FindTargetInAttackRange()
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            Enemy closest = null;
            float minDist = GameManager.Instance.Stats.Range;

            foreach (var node in enemies)
            {
                if (node is Enemy e && !e.IsDead)
                {
                    float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
                    if (dist <= minDist)
                    {
                        minDist = dist;
                        closest = e;
                    }
                }
            }
            return closest;
        }

        private void PerformAttack(Enemy target)
        {
            bool isRage = GameManager.Instance.IsRageActive;
            float damage = GameManager.Instance.Stats.Atk;
            bool isCrit = isRage;
            if (isCrit) damage *= 2f;

            // Slash tween animation
            if (_visualRoot != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(_visualRoot, "position:x", 35f, 0.08f)
                     .SetTrans(Tween.TransitionType.Back)
                     .SetEase(Tween.EaseType.Out);
                tween.TweenProperty(_visualRoot, "position:x", 0f, 0.12f);
            }

            target.TakeDamage(damage, isCrit);

            // Lifesteal
            float lifestealRate = GameManager.Instance.Stats.LifestealPercent / 100f;
            float heal = damage * lifestealRate;
            Heal(heal);

            // Rage gain
            GameManager.Instance.AddRage(2.0f);
        }

        public void Heal(float amount)
        {
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public void TakeDamage(float amount)
        {
            CurrentHp -= amount;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                GameManager.Instance.NotifyHeroDied();
            }
        }
    }
}
```

- [ ] **Step 4: Create `Projectile.tscn`, `PetCompanion.tscn`, and `Hero.tscn` scenes with stylized 2D visuals and particles**

- [ ] **Step 5: Run `dotnet build` to verify Task 2 compilation**

Run: `dotnet build`
Expected: 0 errors, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add Scenes/ Scripts/Combat/
git commit -m "feat: implement Hero, PetCompanion and BloodProjectile actors"
```

---

### Task 3: Enemies, Boss with Enrage, and Wave Spawner

**Files:**
- Create: `Scripts/Combat/Enemy.cs`
- Create: `Scripts/Combat/BossEnemy.cs`
- Create: `Scripts/Combat/WaveSpawner.cs`
- Create: `Scenes/Enemy.tscn`
- Create: `Scenes/BossEnemy.tscn`

**Interfaces:**
- Consumes: `Hero`, `GameManager.Instance.CurrentWave`, `GameManager.Instance.AddGold()`
- Produces: `Enemy.TakeDamage()`, `BossEnemy.EnrageMultiplier`, `WaveSpawner.SpawnWave()`

- [ ] **Step 1: Create `Enemy.cs`**

```csharp
using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class Enemy : CharacterBody2D
    {
        [Export] public float MoveSpeed = 120f;
        public float MaxHp { get; protected set; }
        public float CurrentHp { get; protected set; }
        public float AttackDamage { get; protected set; }
        public bool IsDead { get; protected set; } = false;

        public event Action<float, float> OnHealthChanged;

        protected Hero _heroTarget;
        protected double _attackTimer = 0.0;

        public virtual void Setup(int wave, Hero hero)
        {
            _heroTarget = hero;
            MaxHp = wave * 25f + 50f;
            CurrentHp = MaxHp;
            AttackDamage = wave * 3f + 5f;
            AddToGroup("Enemies");
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public override void _Process(double delta)
        {
            if (IsDead) return;

            if (IsInstanceValid(_heroTarget))
            {
                float dist = GlobalPosition.DistanceTo(_heroTarget.GlobalPosition);
                if (dist > 90f)
                {
                    Velocity = new Vector2(-MoveSpeed, 0);
                    MoveAndSlide();
                }
                else
                {
                    _attackTimer += delta;
                    if (_attackTimer >= 1.0)
                    {
                        _attackTimer = 0.0;
                        _heroTarget.TakeDamage(AttackDamage);
                    }
                }
            }
        }

        public virtual void TakeDamage(float amount, bool isCrit)
        {
            if (IsDead) return;

            CurrentHp -= amount;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

            // Pop-up floating damage text
            FloatingTextManager.Instance?.SpawnDamage(GlobalPosition + new Vector2(0, -40), amount, isCrit);

            if (CurrentHp <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            if (IsDead) return;
            IsDead = true;

            long goldReward = GameManager.Instance.CurrentWave * 5 + 10;
            GameManager.Instance.AddGold(goldReward);
            FloatingTextManager.Instance?.SpawnGold(GlobalPosition, goldReward);

            QueueFree();
        }
    }
}
```

- [ ] **Step 2: Create `BossEnemy.cs` with Enrage scaling**

```csharp
using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class BossEnemy : Enemy
    {
        public float EnrageMultiplier { get; private set; } = 1.0f;
        private double _enrageTimer = 0.0;

        public override void Setup(int wave, Hero hero)
        {
            _heroTarget = hero;
            MoveSpeed = 80f;
            MaxHp = wave * 200f + 400f;
            CurrentHp = MaxHp;
            AttackDamage = wave * 15f + 20f;
            AddToGroup("Enemies");
            Scale = new Vector2(1.8f, 1.8f);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (IsDead) return;

            _enrageTimer += delta;
            if (_enrageTimer >= 5.0)
            {
                _enrageTimer = 0.0;
                EnrageMultiplier += 0.25f;
                AttackDamage *= 1.25f;

                FloatingTextManager.Instance?.SpawnEnrage(GlobalPosition + new Vector2(0, -90), EnrageMultiplier);
            }
        }

        protected override void Die()
        {
            if (IsDead) return;
            IsDead = true;

            long bossGold = GameManager.Instance.CurrentWave * 50 + 200;
            GameManager.Instance.AddGold(bossGold);
            FloatingTextManager.Instance?.SpawnGold(GlobalPosition, bossGold);

            QueueFree();
        }
    }
}
```

- [ ] **Step 3: Create `WaveSpawner.cs`**

```csharp
using Godot;
using System.Collections.Generic;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class WaveSpawner : Node2D
    {
        [Export] public PackedScene EnemyScene;
        [Export] public PackedScene BossScene;
        [Export] public Hero TargetHero;
        [Export] public Vector2 SpawnPosition = new Vector2(2050, 700);

        private int _enemiesRemainingToSpawn = 0;
        private double _spawnCooldown = 0.0;
        private bool _isWaveActive = false;

        public override void _Ready()
        {
            GameManager.Instance.OnWaveChanged += StartWave;
            GameManager.Instance.OnHeroDied += ClearAllEnemies;
            StartWave(GameManager.Instance.CurrentWave, GameManager.Instance.CurrentWave % 10 == 0);
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveChanged -= StartWave;
                GameManager.Instance.OnHeroDied -= ClearAllEnemies;
            }
        }

        public void StartWave(int wave, bool isBoss)
        {
            ClearAllEnemies();
            _isWaveActive = true;

            if (isBoss)
            {
                _enemiesRemainingToSpawn = 1;
                SpawnBoss(wave);
            }
            else
            {
                _enemiesRemainingToSpawn = 5;
                _spawnCooldown = 0.3;
            }
        }

        public override void _Process(double delta)
        {
            if (!_isWaveActive) return;

            if (_enemiesRemainingToSpawn > 0)
            {
                _spawnCooldown -= delta;
                if (_spawnCooldown <= 0.0)
                {
                    _spawnCooldown = 1.2;
                    SpawnEnemy(GameManager.Instance.CurrentWave);
                    _enemiesRemainingToSpawn--;
                }
            }
            else
            {
                // Check if all enemies defeated
                var activeEnemies = GetTree().GetNodesInGroup("Enemies");
                if (activeEnemies.Count == 0)
                {
                    _isWaveActive = false;
                    GameManager.Instance.AdvanceWave();
                }
            }
        }

        private void SpawnEnemy(int wave)
        {
            if (EnemyScene == null || TargetHero == null) return;
            var enemy = EnemyScene.Instantiate<Enemy>();
            enemy.GlobalPosition = SpawnPosition + new Vector2(0, GD.Randf() * 40 - 20);
            enemy.Setup(wave, TargetHero);
            AddChild(enemy);
        }

        private void SpawnBoss(int wave)
        {
            if (BossScene == null || TargetHero == null) return;
            var boss = BossScene.Instantiate<BossEnemy>();
            boss.GlobalPosition = SpawnPosition;
            boss.Setup(wave, TargetHero);
            AddChild(boss);
            _enemiesRemainingToSpawn = 0;
        }

        private void ClearAllEnemies()
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            foreach (var node in enemies)
            {
                if (node is Node n) n.QueueFree();
            }
            _enemiesRemainingToSpawn = 0;
        }
    }
}
```

- [ ] **Step 4: Create `Enemy.tscn` and `BossEnemy.tscn` scenes**

- [ ] **Step 5: Run `dotnet build` to verify Task 3 compilation**

Run: `dotnet build`
Expected: 0 errors, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add Scenes/ Scripts/Combat/
git commit -m "feat: implement Enemy, BossEnemy with Enrage, and WaveSpawner"
```

---

### Task 4: Interactive Combat & Polish - Tap Damage & Floating Numbers

**Files:**
- Create: `Scripts/UI/FloatingText.cs`
- Create: `Scripts/UI/FloatingTextManager.cs`
- Create: `Scripts/Combat/TapCombatArea.cs`
- Create: `Scenes/UI/FloatingText.tscn`

**Interfaces:**
- Produces: `FloatingTextManager.Instance.SpawnDamage()`, `FloatingTextManager.Instance.SpawnGold()`, `FloatingTextManager.Instance.SpawnEnrage()`
- Produces: `TapCombatArea` input event handler that damages front enemy and triggers blood splatter VFX

- [ ] **Step 1: Create `FloatingText.cs` and `FloatingTextManager.cs`**

```csharp
using Godot;

namespace BloodSeal.UI
{
    public partial class FloatingText : Node2D
    {
        [Export] public Label LabelNode;

        public void Setup(string text, Color color, float scale = 1.0f)
        {
            if (LabelNode != null)
            {
                LabelNode.Text = text;
                LabelNode.Modulate = color;
            }
            Scale = Vector2.One * scale;

            var tween = CreateTween().SetParallel(true);
            tween.TweenProperty(this, "position:y", Position.Y - 60f, 0.7f)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(this, "modulate:a", 0f, 0.7f)
                 .SetEase(Tween.EaseType.In);
            tween.Chain().TweenCallback(Callable.From(QueueFree));
        }
    }
}
```

```csharp
using Godot;

namespace BloodSeal.Combat
{
    public partial class FloatingTextManager : Node2D
    {
        public static FloatingTextManager Instance { get; private set; }
        [Export] public PackedScene FloatingTextScene;

        public override void _EnterTree() => Instance = this;

        public void SpawnDamage(Vector2 pos, float amount, bool isCrit)
        {
            if (FloatingTextScene == null) return;
            var text = FloatingTextScene.Instantiate<UI.FloatingText>();
            text.GlobalPosition = pos + new Vector2(GD.Randf() * 30 - 15, GD.Randf() * 20 - 10);
            Color col = isCrit ? new Color(1f, 0.2f, 0.2f) : Colors.White;
            string prefix = isCrit ? "CRIT! " : "";
            text.Setup($"{prefix}{amount:F0}", col, isCrit ? 1.4f : 1.0f);
            AddChild(text);
        }

        public void SpawnGold(Vector2 pos, long amount)
        {
            if (FloatingTextScene == null) return;
            var text = FloatingTextScene.Instantiate<UI.FloatingText>();
            text.GlobalPosition = pos;
            text.Setup($"+{amount} 🪙", new Color(1f, 0.85f, 0.2f), 1.1f);
            AddChild(text);
        }

        public void SpawnEnrage(Vector2 pos, float multiplier)
        {
            if (FloatingTextScene == null) return;
            var text = FloatingTextScene.Instantiate<UI.FloatingText>();
            text.GlobalPosition = pos;
            text.Setup($"ENRAGE x{multiplier:F1}!", new Color(1f, 0.1f, 0.1f), 1.5f);
            AddChild(text);
        }
    }
}
```

- [ ] **Step 2: Create `TapCombatArea.cs`**

```csharp
using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class TapCombatArea : Control
    {
        [Export] public PackedScene TapVfxScene;

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                OnTap(mouseEvent.GlobalPosition);
            }
            else if (@event is InputEventScreenTouch touchEvent && touchEvent.Pressed)
            {
                OnTap(touchEvent.Position);
            }
        }

        private void OnTap(Vector2 tapPos)
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            Enemy closest = null;
            float minX = float.MaxValue;

            foreach (var node in enemies)
            {
                if (node is Enemy e && !e.IsDead)
                {
                    if (e.GlobalPosition.X < minX)
                    {
                        minX = e.GlobalPosition.X;
                        closest = e;
                    }
                }
            }

            if (closest != null)
            {
                float tapDmg = GameManager.Instance.Stats.Atk * 0.75f;
                bool isCrit = GameManager.Instance.IsRageActive;
                if (isCrit) tapDmg *= 2f;

                closest.TakeDamage(tapDmg, isCrit);
                GameManager.Instance.AddRage(1.5f);
            }

            // VFX feedback at tap position
            FloatingTextManager.Instance?.SpawnDamage(tapPos, GameManager.Instance.Stats.Atk * 0.75f, false);
        }
    }
}
```

- [ ] **Step 3: Run `dotnet build` to verify Task 4 compilation**

Run: `dotnet build`
Expected: 0 errors, 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add Scenes/ Scripts/
git commit -m "feat: implement TapCombatArea and FloatingText popups"
```

---

### Task 5: Gothic Atmosphere & Parallax Scene

**Files:**
- Create: `Shaders/BloodVignette.gdshader`
- Create: `Shaders/ParallaxFog.gdshader`
- Create: `Scenes/MainCombat.tscn`

**Interfaces:**
- Produces: The primary root scene of the game combining Parallax layers, Hero, Pets, Spawner, TapArea, and HUD.

- [ ] **Step 1: Create `BloodVignette.gdshader` and `ParallaxFog.gdshader`**

```glsl
shader_type canvas_item;

uniform float vignette_intensity : hint_range(0.0, 1.0) = 0.5;
uniform vec4 blood_color : source_color = vec4(0.8, 0.05, 0.1, 1.0);
uniform float pulse_speed : hint_range(0.0, 10.0) = 4.0;

void fragment() {
    vec2 uv = UV - vec2(0.5);
    float dist = length(uv) * 1.414;
    float pulse = sin(TIME * pulse_speed) * 0.1 + 0.9;
    float alpha = smoothstep(0.4, 0.9, dist) * vignette_intensity * pulse;
    COLOR = vec4(blood_color.rgb, alpha);
}
```

- [ ] **Step 2: Construct `MainCombat.tscn` with 3 Parallax Layers (Blood Moon sky, gothic ruins, cobblestone ground)**

- [ ] **Step 3: Configure `project.godot` to set `Scenes/MainCombat.tscn` as `run/main_scene`**

- [ ] **Step 4: Run `dotnet build`**

Run: `dotnet build`
Expected: 0 errors, 0 warnings.

- [ ] **Step 5: Commit**

```bash
git add Shaders/ Scenes/MainCombat.tscn project.godot
git commit -m "feat: implement Gothic parallax atmosphere and MainCombat scene"
```

---

### Task 6: Responsive UI / HUD & Full Game Loop Integration

**Files:**
- Create: `Scripts/UI/MainHUD.cs`
- Create: `Scenes/UI/MainHUD.tscn`
- Modify: `Scenes/MainCombat.tscn`

**Interfaces:**
- Consumes: `GameManager.Instance`, `GameManager.OnGoldChanged`, `GameManager.OnWaveChanged`, `GameManager.OnRageChanged`, `GameManager.OnRageStateChanged`
- Produces: Complete playable HUD with gold formatting, wave indicator, 5 Pentagram upgrade cards, Rage button, Boss Retry button.

- [ ] **Step 1: Create `MainHUD.cs`**

```csharp
using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class MainHUD : CanvasLayer
    {
        [Export] public Label GoldLabel;
        [Export] public Label WaveLabel;
        [Export] public Button RageButton;
        [Export] public ProgressBar RageProgressBar;
        [Export] public Button RetryBossButton;
        [Export] public ColorRect RageVignetteRect;

        // Pentagram Buttons
        [Export] public Button UpgradeAtkBtn;
        [Export] public Button UpgradeAtkSpdBtn;
        [Export] public Button UpgradeLifestealBtn;
        [Export] public Button UpgradeMaxHpBtn;
        [Export] public Button UpgradeRangeBtn;

        public override void _Ready()
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldUI;
            GameManager.Instance.OnWaveChanged += UpdateWaveUI;
            GameManager.Instance.OnRageChanged += UpdateRageUI;
            GameManager.Instance.OnRageStateChanged += UpdateRageStateUI;
            GameManager.Instance.OnStatsUpgraded += UpdateAllStatButtons;

            // Connect button signals
            RageButton?.Connect("pressed", Callable.From(OnRagePressed));
            RetryBossButton?.Connect("pressed", Callable.From(OnRetryBossPressed));
            UpgradeAtkBtn?.Connect("pressed", Callable.From(() => GameManager.Instance.UpgradeAtk()));
            UpgradeAtkSpdBtn?.Connect("pressed", Callable.From(() => GameManager.Instance.UpgradeAtkSpeed()));
            UpgradeLifestealBtn?.Connect("pressed", Callable.From(() => GameManager.Instance.UpgradeLifesteal()));
            UpgradeMaxHpBtn?.Connect("pressed", Callable.From(() => GameManager.Instance.UpgradeMaxHp()));
            UpgradeRangeBtn?.Connect("pressed", Callable.From(() => GameManager.Instance.UpgradeRange()));

            UpdateGoldUI(GameManager.Instance.Gold);
            UpdateWaveUI(GameManager.Instance.CurrentWave, GameManager.Instance.CurrentWave % 10 == 0);
            UpdateRageUI(GameManager.Instance.RagePercentage);
            UpdateRageStateUI(GameManager.Instance.IsRageActive);
            UpdateAllStatButtons();
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -= UpdateGoldUI;
                GameManager.Instance.OnWaveChanged -= UpdateWaveUI;
                GameManager.Instance.OnRageChanged -= UpdateRageUI;
                GameManager.Instance.OnRageStateChanged -= UpdateRageStateUI;
                GameManager.Instance.OnStatsUpgraded -= UpdateAllStatButtons;
            }
        }

        private void UpdateGoldUI(long gold)
        {
            if (GoldLabel != null) GoldLabel.Text = $"🪙 {FormatNumber(gold)} Altın";
            UpdateAllStatButtons();
        }

        private void UpdateWaveUI(int wave, bool isBoss)
        {
            if (WaveLabel != null)
            {
                if (isBoss)
                    WaveLabel.Text = $"⚠️ BOSS SAVAŞI: DALGA {wave} ⚠️";
                else if (GameManager.Instance.IsInSafeFarmMode)
                    WaveLabel.Text = $"⚔️ GÜVENLİ FARM: DALGA {wave} ⚔️";
                else
                    WaveLabel.Text = $"DALGA {wave} / {((wave / 10) + 1) * 10}";
            }

            if (RetryBossButton != null)
            {
                RetryBossButton.Visible = GameManager.Instance.IsInSafeFarmMode;
            }
        }

        private void UpdateRageUI(float percentage)
        {
            if (RageProgressBar != null) RageProgressBar.Value = percentage;
            if (RageButton != null)
            {
                RageButton.Disabled = percentage < 100f || GameManager.Instance.IsRageActive;
                RageButton.Text = GameManager.Instance.IsRageActive ? "BERSERK AKTİF!" : (percentage >= 100f ? "ÖFKEYİ SERBEST BIRAK!" : $"Öfke: %{percentage:F0}");
            }
        }

        private void UpdateRageStateUI(bool isActive)
        {
            if (RageVignetteRect != null)
            {
                RageVignetteRect.Visible = isActive;
            }
            UpdateRageUI(GameManager.Instance.RagePercentage);
        }

        private void UpdateAllStatButtons()
        {
            var stats = GameManager.Instance.Stats;
            long gold = GameManager.Instance.Gold;

            UpdateButton(UpgradeAtkBtn, "ATK (Güç)", stats.AtkLevel, stats.Atk, stats.GetAtkCost(), gold);
            UpdateButton(UpgradeAtkSpdBtn, "HIZ (Vuruş/s)", stats.AtkSpeedLevel, stats.AtkSpeed, stats.GetAtkSpeedCost(), gold);
            UpdateButton(UpgradeLifestealBtn, "CAN ÇALMA", stats.LifestealLevel, stats.LifestealPercent, stats.GetLifestealCost(), gold, "%");
            UpdateButton(UpgradeMaxHpBtn, "MAX CAN", stats.MaxHpLevel, stats.MaxHp, stats.GetMaxHpCost(), gold);
            UpdateButton(UpgradeRangeBtn, "MENZİL", stats.RangeLevel, stats.Range, stats.GetRangeCost(), gold, "px");
        }

        private void UpdateButton(Button btn, string statName, int lvl, float val, long cost, long currentGold, string unit = "")
        {
            if (btn == null) return;
            btn.Text = $"{statName} Lv.{lvl}\n({val:F1}{unit}) - 🪙 {FormatNumber(cost)}";
            btn.Disabled = currentGold < cost;
        }

        private void OnRagePressed() => GameManager.Instance.TriggerRage();
        private void OnRetryBossPressed() => GameManager.Instance.RetryBoss();

        private string FormatNumber(long num)
        {
            if (num >= 1_000_000_000) return $"{(num / 1_000_000_000f):F2}B";
            if (num >= 1_000_000) return $"{(num / 1_000_000f):F2}M";
            if (num >= 1_000) return $"{(num / 1_000f):F1}K";
            return num.ToString();
        }
    }
}
```

- [ ] **Step 2: Create `MainHUD.tscn` with responsive anchors and Gothic dark theme styling**

- [ ] **Step 3: Run `dotnet build` to verify full compilation**

Run: `dotnet build`
Expected: 0 errors, 0 warnings.

- [ ] **Step 4: End-to-end launch test with Godot console executable**

Run: `& "C:\Users\Partridge\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe" --headless --quit-after 100`
Expected: Game initializes without crashing and exits cleanly.

- [ ] **Step 5: Commit**

```bash
git add Scenes/ Scripts/ project.godot
git commit -m "feat: integrate MainHUD and complete playable core combat loop"
```
