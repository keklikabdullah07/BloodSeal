# 🩸 BloodSeal: Görsel & Sprite Pipeline Tasarım Belgesi (Visual & Sprite Pipeline Design)

**Belge Tarihi:** 2026-10-09  
**Durum:** Onaylandı (Approved)  
**Yazar:** Antigravity  
**Kapsam:** Adım C - Prototip Polygon2D çizimlerinin profesyonel Gotik Dark Fantasy 2D sprite ve katmanlı paralaks dokularına dönüştürülmesi.

---

## 1. Genel Bakış ve Amaç

BloodSeal projesinin çekirdek mekanikleri, nesne havuzlama optimizasyonu (GC sıfırlama), ses mimarisi, dalga akışı ve idle ekonomisi tamamlanmıştır. Ancak sahnede yer alan tüm aktörler (Kahraman, Düşman, Boss, Pet) ve arka plan katmanları geçici `Polygon2D` geometrilerinden ibarettir.

Bu tasarımın amacı; oyunun görsel seviyesini gerçek bir **Gotik Dark Fantasy Idle RPG** standartlarına ulaştırmak, tüm geçici poligonları yüksek kontrastlı, mobil cihazlarda okunabilirliği yüksek 2D sprite'larla değiştirmek ve mevcut kod/animasyon altyapısıyla sıfır kırılma (zero-breakage) ile entegre etmektir.

---

## 2. Sanat Yönetimi ve Renk Paleti (Art Direction)

`bloodseal-art-style` direktiflerine birebir uyulacaktır:

### A. Temel Renk Paleti
- **Blood Crimson (`#D91A2A`):** Kan rünleri, kılıç savurma ışıkları, kritik vuruş efektleri, Kızıl Ay ve enerji hareleri.
- **Abyssal Void (`#0A080F`):** Arka plan gökyüzü, katedral gölgeleri ve derin karanlık.
- **Ruin Slate (`#1A1624`):** Gotik kule siluetleri, sivri katedral kemerleri, demir parmaklıklar.
- **Cobblestone Ground (`#2A222B`):** Arnavut kaldırımı döşemeler, antik mezar taşları ve yürüyüş yolu.
- **Bone Pale (`#E8E0D5`):** Kemik maskeler, açık zırh hatları, kafatası sembolleri ve hasar metinleri.
- **Runic Amber Gold (`#FFC845`):** Rünik mühürler, kutsal parıltılar ve ganimet detayları.

### B. Mobil Ekran Kontrastı ve Kenar Işığı (Rim Lighting)
- **Siluet Ayrımı:** Arka plan koyu tonlarda (`#0A080F` ~ `#2A222B`) olduğu için karakterlerin OLED/IPS ekranlarda erimesini önlemek amacıyla karakter dış hatlarında 1-2 piksel açık kemik/kızıl kenar ışığı (Rim Light) bulunacaktır.
- **Zemin Sisi (Ground Fog):** Karakterlerin ayak hizasında zeminden ayrışmayı güçlendiren hafif bir ambient ışık/sis geçişi bulunacaktır.

---

## 3. Üretilecek Varlık Listesi (Asset Manifest)

Tüm varlıklar `Assets/Sprites/` hiyerarşisi altında organize edilecektir:

### A. Karakterler (`Assets/Sprites/Characters/`)
1. **`hero_knight.png` (Kan Şövalyesi / Blood Knight):**
   - **Rol:** Ana oyuncu karakteri.
   - **Görsel:** Sivri gotik koyu plaka zırh, yırtık dalgalanan kızıl pelerin, miğfer vizöründen parlayan kızıl göz ışığı, elde tutulan büyük rünik kılıç (Greatsword).
   - **Boyut & Oran:** ~128x128 px (veya 256x256 px yüksek çözünürlüklü ölçeklendirilmiş).
   - **Entegrasyon:** `Hero.tscn` -> `VisualRoot/HeroSprite`.

2. **`enemy_cultist.png` (Minyon Tarikatçı / Cultist of the Void):**
   - **Rol:** Standart dalga düşmanı.
   - **Görsel:** Yırtık morumsu-siyah cübbe, kemikten yapılmış boynuzlu kafatası maskesi, soluk pençeler, kızıl rünik büyü kitabı/hançer.
   - **Boyut & Oran:** ~96x96 px (veya 192x192 px).
   - **Entegrasyon:** `Enemy.tscn` -> `VisualRoot/EnemySprite`.

3. **`boss_abomination.png` (Kan Lordu / Abyssal Blood Golem):**
   - **Rol:** Her 10 dalgada bir çıkan heybetli Boss.
   - **Görsel:** Minyondan ~2.2 kat daha büyük, heybetli omuzluklar, göğsünde atan çatlak kızıl kan çekirdeği, devasa şeytani boynuzlar ve zırh dikenleri.
   - **Boyut & Oran:** ~192x192 px (veya 384x384 px).
   - **Entegrasyon:** `BossEnemy.tscn` -> `VisualRoot/BossSprite`.

### B. Yoldaş Pet (`Assets/Sprites/Pet/`)
1. **`pet_blood_raven.png` (Kan Kargası / Blood Raven):**
   - **Rol:** Pasif hasar ve büyü mermisi ateşleyen refakatçi.
   - **Görsel:** Süzülen gotik karga silueti, kan kırmızısı parlayan gözler ve kanat uçlarından dökülen eterik kızıl tüy/duman zerrecikleri.
   - **Boyut & Oran:** ~64x64 px (veya 128x128 px).
   - **Entegrasyon:** `PetCompanion.tscn` -> `VisualRoot/PetSprite`.

### C. Katmanlı Paralaks Arka Planı (`Assets/Sprites/Background/`)
1. **`bg_sky_bloodmoon.png` (`SkyLayer` - Hız: 8 px/s, Motion Scale: 0.1):**
   - Koyu abyssal gökyüzü, hafif mor-kızıl nebula bulutları ve parlak kırmızı Kızıl Ay (Blood Moon) dokusu.
2. **`bg_ruins_spires.png` (`RuinsLayer` - Hız: 35 px/s, Motion Scale: 0.4):**
   - Gotik katedral kuleleri, kırık sivri kemerler, demir parmaklıklar ve asılı gotik zincir siluetleri (Döşenebilir / Seamless yatay döngü).
3. **`bg_ground_cobblestone.png` (`ForegroundLayer` - Hız: 90 px/s, Motion Scale: 1.0):**
   - Gotik arnavut kaldırımı taş zemin, kırık mezar taşları ve zemin sisi geçişi (Döşenebilir / Seamless yatay döngü).

---

## 4. Teknik Mimari ve Kod Uyumluluğu

### A. Sıfır Kırılma İlkesi (Zero-Breakage Architecture)
- C# kodları (`Hero.cs`, `Enemy.cs`, `BossEnemy.cs`, `PetCompanion.cs`, `WaveSpawner.cs`, `FXManager.cs`) aktörlerin görsel kökünü `VisualRoot` (`Node2D`) olarak referans alır.
- Bu düğüm altındaki poligonlar `Sprite2D` düğümleriyle değiştirilecektir; ancak `VisualRoot` referansı, hiyerarşik konumu ve dönüşüm (transform) özellikleri korunacaktır.
- Mevcut C# tabanlı prosedürel animasyonlar (Tween tabanlı squash & stretch, vurulma sarsıntısı, tilt, hit-flash `Modulate`) hiçbir kod değişikliğine gerek kalmadan `Sprite2D` üzerinde de kusursuz çalışacaktır.
- `CharacterBody2D` çarpışma sınırları (`CollisionShape2D`) yeni sprite boyutlarına göre ayarlanacaktır.

### B. Godot 4.7 Uyumluluk ve Render Ayarları
- **Format:** Şeffaf PNG (RGBA, 8-bit per channel).
- **Doku Filtreleme (Texture Filter):** `Linear With Mipmaps` veya `Nearest With Mipmaps` (Net kenar hatları ve düzgün ölçekleme için).
- **Paralaks Döngüsü:** Arka plan katmanlarında `motion_mirroring = Vector2(2400, 0)` veya `1920, 0` parametreleri sprite genişliğine göre optimize edilecektir.

---

## 5. Doğrulama ve Kabul Kriterleri (Acceptance Criteria)

1. **Görsel Kalite:** Tüm sahnede hiçbir düz poligon (`Polygon2D`) kalmayacak, karanlık gotik fantezi temasına uygun yüksek kaliteli 2D görseller yer alacaktır.
2. **Animasyon Bütünlüğü:**
   - Kahraman vuruş yaptığında kılıç savurma açısı ve beden tilt hareketi düzgün çalışmalıdır.
   - Düşman ve Boss hasar aldığında beyaz flaş (`Modulate`) ve mikro squash/stretch animasyonları çalışmalıdır.
   - Pet kahramanın arkasında süzülme salınımını korumalıdır.
3. **Kod ve Performans:**
   - `dotnet build` 0 hata ve 0 uyarı ile tamamlanmalıdır.
   - Mevcut 62/62 xUnit testinin tamamı başarıyla geçmelidir (`dotnet test`).
   - Nesne havuzlama (`NodePool<T>`) yeni sprite aktörleriyle kusursuz çalışmalı, GC takılmaları yaşanmamalıdır.
   - Godot konsolunda headless modda (`--headless --quit-after 60`) hiçbir sahne kırılması veya kaynak eksikliği yaşanmamalıdır.
