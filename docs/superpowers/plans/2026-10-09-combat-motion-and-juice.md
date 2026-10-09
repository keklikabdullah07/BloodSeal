# 🩸 BloodSeal: Karakter & Vuruş Animasyon Zenginleştirmesi Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Kahraman, Minyonlar ve Boss'lar için prosedürel nefes alma (breathing idle), kılıç savurma eğilmesi (lunge & tilt), darbe squash & stretch tepkisi ve ölüm savrulması (death spin-fade) ekleyerek dövüş hissini (game feel) prodüksiyon seviyesine yükseltmek.

**Architecture:** Godot 4.7 `Tween` API'si ve sinüzoidal formüller kullanılarak mevcut C# sınıfları (`Hero.cs`, `Enemy.cs`, `BossEnemy.cs`) zenginleştirilecek; nesne havuzlama ve `Engine.TimeScale` kuralları korunacaktır.

**Tech Stack:** Godot 4.7.x Mono (C#), .NET 10.0 (`net10.0`), xUnit (`dotnet test`).

## Global Constraints
- Target Framework: `net10.0`.
- Dosya satır sınırı: Hiçbir C# dosyası 250 satırı aşamaz (`Hero.cs` ve `Enemy.cs` dikkatle optimize edilmelidir).
- `Engine.TimeScale`'e asla dokunulmaz.
- Berserk kuralları: Berserk süresince vuruş başı sarsıntı eklenmez, sabit 0.15 base trauma korunur.

---

### Task 1: Kahraman Canlı Duruş ve Gelişmiş Vuruş Yaylanması (`Hero.cs`)

**Files:**
- Modify: `Scripts/Combat/Hero.cs`

**Interfaces:**
- Breathing: `_Process(delta)` içinde saldırı beklemesindeyken sinüzoidal nefes alma salınımı.
- PerformAttack: İleri atılma (`position:x: 35f`) ile birlikte öne eğilme (`rotation: 0.08f`) ve esnek geri dönüş.

- [ ] **Step 1: `Hero.cs` içine breathing idle ve tilt lunge ekle**
- [ ] **Step 2: Satır sınırını (< 250 satır) ve derlemeyi doğrula**
- [ ] **Step 3: Commit Task 1**
  - `git add Scripts/Combat/Hero.cs && git commit -m "feat(combat): add combat breathing idle and tilt lunge to Hero"`

---

### Task 2: Düşman Hasar Tepkisi, Squash-Stretch ve Saldırı Atılması (`Enemy.cs`)

**Files:**
- Modify: `Scripts/Combat/Enemy.cs`

**Interfaces:**
- TakeDamage: Vurulma anında `scale = (1.14f, 0.88f)` -> `(1f, 1f)` squash-stretch ve 10px mikro-geri tepme.
- PerformAttackOnHero: Vuruş yaparken öne yaylanma (`position:x -= 20f`) ve geri çekilme tween'i.

- [ ] **Step 1: `Enemy.cs` içine squash-stretch ve saldırı yaylanması ekle**
- [ ] **Step 2: Satır sınırını (< 250 satır) ve derlemeyi doğrula**
- [ ] **Step 3: Commit Task 2**
  - `git add Scripts/Combat/Enemy.cs && git commit -m "feat(combat): add squash-stretch hit reaction and attack lunge to Enemy"`

---

### Task 3: Düşman Ölüm Savrulması (Death Spin-Fade) ve Havuzlama Uyumu (`Enemy.cs`, `BossEnemy.cs`)

**Files:**
- Modify: `Scripts/Combat/Enemy.cs`
- Modify: `Scripts/Combat/BossEnemy.cs`

**Interfaces:**
- Die: Düşman canı bittiğinde 0.12s savrulma (`rotation = -0.3f`, `modulate.a -> 0`) ardından `ReleaseEnemy` çağrısı; havuza dönerken `rotation = 0` ve `modulate.a = 1` sıfırlanması.

- [ ] **Step 1: `Enemy.cs` içine ölüm savrulması tween'i ekle**
- [ ] **Step 2: `ResetForPool()` içinde rotasyon ve alfa sıfırlamasını sağla**
- [ ] **Step 3: Commit Task 3**
  - `git add Scripts/Combat/Enemy.cs Scripts/Combat/BossEnemy.cs && git commit -m "feat(combat): add death spin-fade animation with safe pool recycle"`

---

### Task 4: Doğrulama Kapısı & Smoke Test

- [ ] **Step 1: `dotnet test` (68/68 test başarılı)**
- [ ] **Step 2: `dotnet build` (0 warning, 0 error)**
- [ ] **Step 3: Godot headless smoke test (60 frames hatasız)**
- [ ] **Step 4: Satır sayıları kontrolü (tüm dosyalar <= 250)**
- [ ] **Step 5: Final commit ve tamamlama**
