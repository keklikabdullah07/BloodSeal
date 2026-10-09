# 🩸 BloodSeal: Görsel & Sprite Pipeline Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prototip aşamasındaki tüm `Polygon2D` aktör ve arka plan çizimlerini, yüksek kontrastlı Dark Gothic 2D sprite ve döşenebilir paralaks dokularıyla değiştirerek görsel kaliteyi prodüksiyon seviyesine yükseltmek.

**Architecture:** Karakter sahnelerinde (`Hero.tscn`, `Enemy.tscn`, `BossEnemy.tscn`, `PetCompanion.tscn`) `VisualRoot` düğümü ve C# tween animasyon mantığı bozulmadan poligonlar `Sprite2D` düğümleriyle değiştirilecektir. `MainCombat.tscn` içindeki paralaks katmanları ise döşenebilir dokularla donatılacaktır.

**Tech Stack:** Godot 4.7.x Mono (C#), .NET 10.0 (`net10.0`), Python 3.14 + Pillow 12.2 (asset normalization), NUnit/xUnit (`dotnet test`).

## Global Constraints
- Target Framework: `net10.0`.
- Dosya satır sınırı: Hiçbir C# dosyası 250 satırı aşamaz.
- "Call Down, Signal Up" ve Zero-Breakage: Kod içi `VisualRoot` ve `Modulate` referansları korunacak, C# tween mantığı aynen çalışacaktır.
- `Engine.TimeScale`'e asla dokunulmaz.
- Mobil kontrast kuralı: Koyu arka plan üzerinde karakter siluetlerini ayırmak için kemik/kızıl kenar ışığı (rim-light) korunacaktır.

---

### Task 1: Asset Üretimi, Normalizasyonu ve Hazırlığı

**Files:**
- Script: `scripts/normalize_sprites.py`
- Output:
  - `Assets/Sprites/Characters/hero_knight.png`
  - `Assets/Sprites/Characters/enemy_cultist.png`
  - `Assets/Sprites/Characters/boss_abomination.png`
  - `Assets/Sprites/Pet/pet_blood_raven.png`
  - `Assets/Sprites/Background/bg_sky_bloodmoon.png`
  - `Assets/Sprites/Background/bg_ruins_spires.png`
  - `Assets/Sprites/Background/bg_ground_cobblestone.png`

**Interfaces:**
- Produces: 7 adet şeffaf arka planlı, karanlık gotik renk paletine (`#D91A2A`, `#0A080F`, `#1A1624`, `#2A222B`, `#E8E0D5`) uygun optimize PNG dosyası.

- [ ] **Step 1: Görsel varlıkları oluştur (generate_image aracı ile)**
  - Hero Knight: Dark plate armor, torn crimson cape, glowing red visor, greatsword.
  - Enemy Cultist: Hooded robe, horn/skull bone mask, claws.
  - Boss Abomination: Giant gothic blood lord, glowing chest core, horned spikes.
  - Pet Blood Raven: Gothic flying crow with ethereal crimson highlights.
  - Parallax Backgrounds: Blood moon sky, gothic ruin spires, cobblestone ground.

- [ ] **Step 2: Normalizasyon script'i yaz (`scripts/normalize_sprites.py`)**
  - Python Pillow kullanarak üretilen görselleri transparan PNG formatına dönüştür, boyutlandır (128x128 hero, 96x96 cultist, 256x256 boss, 64x64 pet, 1920x1080 / 2400x600 bg katmanları) ve `Assets/Sprites/` klasörlerine kaydet.

- [ ] **Step 3: Script'i çalıştır ve dosya varlıklarını doğrula**
  - `python scripts/normalize_sprites.py` çalıştır.
  - Tüm 7 PNG dosyasının doğru dizinlerde ve geçerli boyutlarda olduğunu doğrula.

- [ ] **Step 4: Commit**
  - `git add Assets/Sprites/ scripts/normalize_sprites.py && git commit -m "feat: generate and normalize dark gothic sprite assets"`

---

### Task 2: Hero Sahnesinin Sprite2D ile Yenilenmesi (`Hero.tscn`)

**Files:**
- Modify: `Scenes/Hero.tscn`
- Verify: `Scripts/Combat/Hero.cs`

**Interfaces:**
- Consumes: `Assets/Sprites/Characters/hero_knight.png`
- Produces: `VisualRoot` altında poligonlar yerine `Sprite2D` kullanan ve C# animasyonlarıyla (tilt, attack recoil, hit flash) kusursuz çalışan `Hero.tscn`.

- [ ] **Step 1: `Hero.tscn` sahnesini güncelle**
  - Eski poligonları (`Cloak`, `RimLight`, `Body`, `Head`, `GiantSword`, vb.) kaldır.
  - `VisualRoot` altına `hero_knight.png` referanslı `Sprite2D` (`HeroSprite`) ve elips gölge `Shadow` yerleştir.
  - `CollisionShape2D` (kapsül) ve `HealthBar` pozisyonlarını yeni sprite'a göre hizala.

- [ ] **Step 2: C# entegrasyonunu doğrula**
  - `Hero.cs` içindeki `_visualRoot.Modulate` ve tween kodlarının hatasız çalıştığını doğrula.

- [ ] **Step 3: Headless test ile doğrula**
  - `dotnet test` ve Godot `--headless --quit-after 10` ile `Hero.tscn`'nin çökmeksizin yüklendiğini teyit et.

- [ ] **Step 4: Commit**
  - `git add Scenes/Hero.tscn && git commit -m "feat: integrate hero knight 2d sprite into Hero scene"`

---

### Task 3: Minyon ve Boss Sahnelerinin Yenilenmesi (`Enemy.tscn`, `BossEnemy.tscn`)

**Files:**
- Modify: `Scenes/Enemy.tscn`
- Modify: `Scenes/BossEnemy.tscn`
- Verify: `Scripts/Combat/Enemy.cs`, `Scripts/Combat/BossEnemy.cs`

**Interfaces:**
- Consumes: `Assets/Sprites/Characters/enemy_cultist.png`, `Assets/Sprites/Characters/boss_abomination.png`
- Produces: Poligonlar yerine yüksek kaliteli `Sprite2D` kullanan düşman ve boss sahneleri.

- [ ] **Step 1: `Enemy.tscn` sahnesini güncelle**
  - `VisualRoot` altındaki poligonları kaldır, `enemy_cultist.png` referanslı `Sprite2D` ekle.
  - `Shadow`, `CollisionShape2D` ve `HealthBar` konumlarını ayarla.

- [ ] **Step 2: `BossEnemy.tscn` sahnesini güncelle**
  - `VisualRoot` altındaki poligonları kaldır, `boss_abomination.png` referanslı heybetli `Sprite2D` ekle.
  - `Shadow`, `CollisionShape2D` ve `HealthBar` konumlarını ayarla.

- [ ] **Step 3: Squash & stretch, hit-flash ve nesne havuzlama testleri**
  - `dotnet test` çalıştır (düşman oluşturma ve hasar testleri).
  - Godot headless test ile minyon ve boss spawn döngüsünü doğrula.

- [ ] **Step 4: Commit**
  - `git add Scenes/Enemy.tscn Scenes/BossEnemy.tscn && git commit -m "feat: integrate cultist and boss abomination sprites"`

---

### Task 4: Pet Companion Sahnesinin Yenilenmesi (`PetCompanion.tscn`)

**Files:**
- Modify: `Scenes/PetCompanion.tscn`
- Verify: `Scripts/Combat/PetCompanion.cs`

**Interfaces:**
- Consumes: `Assets/Sprites/Pet/pet_blood_raven.png`
- Produces: Poligon küre yerine kan kargası sprite'ı kullanan `PetCompanion.tscn`.

- [ ] **Step 1: `PetCompanion.tscn` sahnesini güncelle**
  - `VisualRoot` altındaki poligonları (`Core`, `RuneRing`) kaldır.
  - `pet_blood_raven.png` referanslı `Sprite2D` yerleştir.

- [ ] **Step 2: Süzülme ve ateşleme animasyonunu doğrula**
  - `PetCompanion.cs` içerisindeki dalgalanma (sinüzoidal bobbing) ve mermi fırlatma fonksiyonlarının çalıştığını teyit et.

- [ ] **Step 3: Commit**
  - `git add Scenes/PetCompanion.tscn && git commit -m "feat: integrate blood raven sprite into PetCompanion scene"`

---

### Task 5: Katmanlı Paralaks Arka Planının Yenilenmesi (`MainCombat.tscn`)

**Files:**
- Modify: `Scenes/MainCombat.tscn`
- Verify: `Scripts/Combat/ParallaxScroller.cs`

**Interfaces:**
- Consumes: `Assets/Sprites/Background/bg_sky_bloodmoon.png`, `Assets/Sprites/Background/bg_ruins_spires.png`, `Assets/Sprites/Background/bg_ground_cobblestone.png`
- Produces: Poligon geometrileri yerine döşenebilir gotik çizim dokuları kullanan 3 katmanlı paralaks arka planı.

- [ ] **Step 1: `SkyLayer`'ı güncelle**
  - Poligon ayı ve düz rengi kaldır; `bg_sky_bloodmoon.png` kullanan `Sprite2D` ekle (`motion_scale = (0.1, 0.1)`).

- [ ] **Step 2: `RuinsLayer`'ı güncelle**
  - Poligon kuleleri kaldır; `bg_ruins_spires.png` kullanan `Sprite2D` ekle (`motion_scale = (0.4, 0.4)`).

- [ ] **Step 3: `ForegroundLayer`'ı güncelle**
  - Poligon zemin bloklarını kaldır; `bg_ground_cobblestone.png` kullanan `Sprite2D` ekle (`motion_scale = (1.0, 1.0)`).

- [ ] **Step 4: Paralaks kaydırma döngüsünü doğrula**
  - `ParallaxScroller.cs` hızları ile `motion_mirroring` uyumunu test et.

- [ ] **Step 5: Commit**
  - `git add Scenes/MainCombat.tscn && git commit -m "feat: integrate gothic parallax background sprite textures"`

---

### Task 6: Bütünsel Doğrulama ve Smoke Test Kapısı

**Files:**
- Test / Verify: Tüm sistem ve sahneler

- [ ] **Step 1: `dotnet test` ile 62 testin tamamını çalıştır**
  - Tüm testlerin başarılı olduğunu (0 fail) doğrula.

- [ ] **Step 2: `dotnet build` ile derlemeyi doğrula**
  - 0 warning, 0 error çıktısı al.

- [ ] **Step 3: Godot Headless Smoke Test**
  - `Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 60` çalıştır.
  - Sahne ağacında hiçbir eksik kaynak, kırık referans veya çökme olmadığını doğrula.

- [ ] **Step 4: Handover ve dokümantasyon güncellemesi**
  - `docs/superpowers/HANDOVER.md` ve `PROJECT_STATE.md` dosyalarını Adım C tamamlandı olarak güncelle ve commit et.
