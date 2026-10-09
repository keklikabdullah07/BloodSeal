# 🩸 BloodSeal: Düşman Çeşitliliği & Gotik Bölge İlerlemesi Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Dalga 1-30+ arasında 3 tematik Gotik Bölgeye (Bölge I: Harabeler, Bölge II: Kripta, Bölge III: Kan Katedrali) özgü 2 yeni minyon ve 2 yeni boss sprite'ı üreterek dinamik nesne havuzu (`NodePool<Enemy>`) entegrasyonu ve HUD bölge başlığıyla oyuna kazandırmak.

**Architecture:** `Enemy.cs` ve `BossEnemy.cs` sınıflarına bölgeye göre dinamik doku (`Texture2D`) atama desteği eklenecek, yeni nesne tahsisi yapılmadan (`Zero GC allocation`) mevcut havuz geri dönüştürülecektir.

**Tech Stack:** Godot 4.7.x Mono (C#), .NET 10.0 (`net10.0`), Python 3.14 + Pillow 12.2, xUnit (`dotnet test`).

## Global Constraints
- Target Framework: `net10.0`.
- Dosya satır sınırı: Hiçbir C# dosyası 250 satırı aşamaz.
- "Call Down, Signal Up" ve Zero-Breakage: Havuzlanan minyonlar bellek tahsisi yapmadan sadece doku güncelleyecektir.
- `Engine.TimeScale`'e asla dokunulmaz.

---

### Task 1: Yeni Minyon ve Boss Sprite'larının Üretimi ve Normalizasyonu

**Files:**
- Output:
  - `Assets/Sprites/Characters/enemy_skeleton_warrior.png`
  - `Assets/Sprites/Characters/boss_crypt_revenant.png`
  - `Assets/Sprites/Characters/enemy_gargoyle.png`
  - `Assets/Sprites/Characters/boss_vampire_patriarch.png`

**Interfaces:**
- Produces: 4 adet şeffaf arka planlı, gotik palete uygun optimize PNG dosyası.

- [ ] **Step 1: Görselleri üret (`generate_image` ile)**
  - Skeleton Warrior: Paslı kalkanlı, kılıçlı, kemik çatlaklı gotik iskelet askeri.
  - Crypt Revenant Boss: Tırpanlı, kaburgasında mavi-kızıl ruh ateşi yanan iskelet kralı.
  - Gargoyle: Taşlaşmış gotik kanatlı, pençeli kızıl muhafız yaratık.
  - Vampire Patriarch Boss: Sivri omuzluklu, gotik zırhlı usta vampir lordu.

- [ ] **Step 2: Python ile şeffaf PNG normalizasyonu yap**
  - Beyaz arka planı flood-fill ile temizle, boyutlandır ve `Assets/Sprites/Characters/` altına kaydet.

- [ ] **Step 3: Commit Task 1**
  - `git add Assets/Sprites/Characters/ && git commit -m "feat: generate skeleton, gargoyle and boss variant sprites"`

---

### Task 2: Bölge (Zone) Modelleri ve Düşman Konfigürasyonu (`ZoneModels.cs`)

**Files:**
- Create: `Scripts/Combat/ZoneModels.cs`
- Create/Modify: `Tests/ZoneProgressionTests.cs`

**Interfaces:**
- Produces:
  - `enum ZoneType { Ruins = 1, Crypt = 2, Cathedral = 3 }`
  - `class ZoneHelper`:
    - `ZoneType GetZoneForWave(int wave)`
    - `string GetZoneName(ZoneType zone)`
    - `string GetMinionTexturePath(int wave)`
    - `string GetBossTexturePath(int wave)`

- [ ] **Step 1: Write failing unit test for Zone progression**
- [ ] **Step 2: Run test to verify it fails**
- [ ] **Step 3: Implement `Scripts/Combat/ZoneModels.cs`**
- [ ] **Step 4: Run test to verify it passes**
- [ ] **Step 5: Commit Task 2**

---

### Task 3: Düşman ve Boss Sınıflarında Dinamik Doku Değişimi (`Enemy.cs`, `BossEnemy.cs`)

**Files:**
- Modify: `Scripts/Combat/Enemy.cs`
- Modify: `Scripts/Combat/BossEnemy.cs`

**Interfaces:**
- `Enemy.Setup(int wave, Hero hero)`: `ZoneHelper.GetMinionTexturePath(wave)` kullanarak `_sprite.Texture` günceller.
- `BossEnemy.Setup(int wave, Hero hero)`: `ZoneHelper.GetBossTexturePath(wave)` kullanarak `_bossSprite.Texture` günceller.

- [ ] **Step 1: `Enemy.cs` içine dinamik sprite güncelleme ekle**
- [ ] **Step 2: `BossEnemy.cs` içine dinamik boss sprite güncelleme ekle**
- [ ] **Step 3: `dotnet test` ile havuzlama ve hasar testlerini doğrula**
- [ ] **Step 4: Commit Task 3**

---

### Task 4: MainHUD Bölge Başlığı Göstergesi (`MainHUD.cs`)

**Files:**
- Modify: `Scripts/UI/MainHUD.cs`

**Interfaces:**
- `WaveLabel` veya yeni `ZoneLabel` üzerinden mevcut bölge adını (`BÖLGE I: TERK EDİLMİŞ HARABELER`) dinamik güncelleme.

- [ ] **Step 1: `MainHUD.cs` içinde `UpdateWaveUI` metoduna Bölge başlığı ekle**
- [ ] **Step 2: Derleme ve satır sınırı (< 250 satır) kontrolü**
- [ ] **Step 3: Commit Task 4**

---

### Task 5: Doğrulama Kapısı ve Smoke Test

- [ ] **Step 1: `dotnet test` (tüm testler geçerli)**
- [ ] **Step 2: `dotnet build` (0 warning, 0 error)**
- [ ] **Step 3: Godot headless smoke test (60 kare hatasız)**
- [ ] **Step 4: Final commit ve tamamlama**
