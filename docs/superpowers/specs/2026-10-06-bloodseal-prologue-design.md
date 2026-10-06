# BloodSeal: Prologue (Kan Soyu & Sokak Geçmişi Seçimi) Tasarım Dokümanı (Spec)

- **Proje:** BloodSeal (2D Gotik Idle RPG)
- **Motor / Dil:** Godot 4.7.2 (.NET 10 / C#)
- **Platform:** 1920x1080 (16:9 Yatay), GL Compatibility Renderer
- **Tarih:** 2026-10-06
- **Durum:** Onaylandı

---

## 1. Amaç & Kapsam
Bu doküman, BLOODSEAL GDD belgesindeki Karakter Gelişimi (Aşama 1: Mutasyon Şekli ve Aşama 2: Sokak Geçmişi) ve Prologue hikaye akışının tasarım spesifikasyonlarını tanımlar.

Amaç: Oyuncu ilk kez oyuna girdiğinde (veya yeni oyun başlattığında) atmosferik bir gotik sahnede adını girmesi, 5 Kan Soyu ve 5 Sokak Geçmişi arasından seçim yaparak karakterine kalıcı pasif savaş bonusları kazandırması ve ardından ana savaşa aktarılmasıdır.

---

## 2. Sistem Mimarisi & Dosya Düzeni

```text
blood-seal/
├── Scenes/
│   └── Prologue/
│       ├── PrologueScene.tscn        # 3 adımlı gotik seçim ve hikaye sahnesi
│       └── SelectionCard.tscn        # Tekrar kullanılabilir seçim kartı bileşeni
└── Scripts/
    ├── Core/
    │   ├── CharacterProfile.cs       # BloodlineType, StreetOriginType ve CharacterProfile veri modelleri
    │   └── SaveData.cs               # Profil verilerini saklayacak şekilde güncellenir
    └── UI/
        ├── PrologueController.cs     # Prologue sahnesi durum ve adım yöneticisi
        └── SelectionCard.cs          # Kart seçim ve parıldama mantığı
```

---

## 3. Veri Modelleri ve Pasif Bonuslar

### 3.1 Kan Soyu (Bloodline)
1. **BoneWeaver (Kemik Dokulu):** +%10 Maksimum Can (`MaxHp *= 1.10f`).
2. **ShadowVeined (Gölge Damarlı):** +35px Saldırı Menzili (`Range += 35f`).
3. **BloodClawed (Kan Pençeli):** +%2.5 Doğuştan Can Çalma (`LifestealPercent += 2.5f`).
4. **SteelFleshed (Çelik Dokulu):** Gelen hasardan -3 düz azaltma (`DamageTaken = Math.Max(1, DamageTaken - 3)`).
5. **SoulDrinker (Ruh Emici):** -%15 Saldırı bekleme süresi (`AtkCooldown *= 0.85f`).

### 3.2 Sokak Geçmişi (Street Origin)
1. **PitFighter (Kafes Dövüşçüsü):** +%10 Saldırı Gücü (`Atk *= 1.10f`).
2. **StreetThief (Sokak Hırsızı):** +%15 Altın Kazanımı (`GoldReward *= 1.15f`).
3. **ExMercenary (Eski Paralı Asker):** +%8 Saldırı Hızı (`AtkSpeed *= 1.08f`).
4. **UnderAlchemist (Yeraltı Kimyageri):** +%25 Tıklama (Tap) Hasarı (`TapDamage *= 1.25f`).
5. **GangLeader (Çete Lideri):** +%25 Ruh/Pet Atış Hızı (`PetCooldown *= 0.75f`).

### 3.3 Entegrasyon Mantığı
- `PentagramStats.cs` hesaplamaları, aktif `CharacterProfile`'ı referans alarak bu çarpanları otomatik uygular.
- `Hero.cs` darbe aldığında `SteelFleshed` indirimi uygular.
- `PetCompanion.cs` ateş ederken `GangLeader` hız bonusunu kullanır.
- `TapCombatArea.cs` tıklama hasarında `UnderAlchemist` bonusunu kullanır.
- `Enemy.cs` öldüğünde `StreetThief` altın bonusunu ekler.

---

## 4. Prologue Arayüz ve Akış Tasarımı (`PrologueScene.tscn`)

### 4.1 Aşamalar
1. **Adım 1: Uyanış & İsimlendirme:**
   - Gotik hikaye metni.
   - `LineEdit` oyuncu isim girişi (Varsayılan: "Valerius").
2. **Adım 2: Kan Soyu Seçimi:**
   - 5 adet `SelectionCard` (İkon, Ad, Pasif Açıklaması).
   - Tıklanan kart kırmızı çerçeveyle parlar.
3. **Adım 3: Sokak Geçmişi Seçimi:**
   - 5 adet `SelectionCard`.
4. **Adım 4: Özet & Uyanış:**
   - Özet kartı ve devasa parlayan buton: **"KÖKEN MÜHRÜNÜ UYANDIR"**.
   - Tıklandığında seçimler `GameManager`'a kaydedilir, `HasCompletedPrologue = true` yapılır ve `GetTree().ChangeSceneToFile("res://Scenes/MainCombat.tscn")` çağrılır.

### 4.2 HUD Entegrasyonu
- `MainHUD.tscn` sol üst köşesine oyuncu profili etiketi eklenir:
  `Valerius [Kemik Dokulu | Kafes Dövüşçüsü]`.

---

## 5. Doğrulama ve Test Planı
1. `dotnet build`: 0 hata, 0 uyarı.
2. `project.godot`: `run/main_scene` ilk başlangıçta `res://Scenes/Prologue/PrologueScene.tscn` olacak şekilde yapılandırılır. Eğer daha önce tamamlanmış bir profil varsa doğrudan `MainCombat.tscn` sahnesine yönlendirir.
3. Headless ve F5 testi ile isim girilip soy ve geçmiş seçilerek savaşa geçilebildiği, HUD'da unvanın görüntülendiği ve pasiflerin statlara yansıdığı doğrulanır.
