# BloodSeal: Aşama 1 - Temel Savaş & Yükseltme Döngüsü Tasarım Dokümanı (Spec)

- **Proje:** BloodSeal (2D Gotik Idle RPG)
- **Motor / Dil:** Godot 4.x (.NET) / C# 8.0+
- **Platform:** 1920x1080 (16:9 Yatay), Compatibility Renderer (OpenGL 3)
- **Tarih:** 2026-10-06
- **Durum:** Onaylandı

---

## 1. Genel Bakış ve Amaç
Bu doküman, BLOODSEAL GDD belgesinde sabitlenen kararlar doğrultusunda, oynanabilir ilk prototip dilimini (Vertical Slice - Phase 1) oluşturmak için gerekli yazılım mimarisi, bileşenler ve tasarım spesifikasyonlarını tanımlar.

Phase 1 hedefi: F5 ile çalıştırıldığında akıcı bir şekilde oynanabilen; kahramanın otomatik savaştığı, 2 ruhun ateş destek verdiği, oyuncunun ekrana tıklayarak hasar verebildiği, düşman ve Boss dalgalarının aktığı, altın kazanılıp Pentagram statlarının yükseltilebildiği ve Berserk Öfke modunun kullanılabildiği tam bir Gotik 2D Idle savaş döngüsüdür.

---

## 2. Dizin ve Dosya Mimarisi

```text
blood-seal/
├── project.godot                     # 1920x1080, stretch canvas_items, GL Compatibility
├── BloodSeal.csproj                  # .NET 10 / C# proje dosyası
├── Scenes/
│   ├── MainCombat.tscn               # Ana savaş, Parallax ve UI konteyneri
│   ├── Hero.tscn                     # CharacterBody2D tabanlı kahraman
│   ├── PetCompanion.tscn             # Node2D tabanlı süzülen yardımcı ruh
│   ├── Enemy.tscn                    # CharacterBody2D tabanlı standart düşman
│   ├── BossEnemy.tscn                # Enrage özellikli Boss düşman
│   ├── Projectile.tscn               # Pet kan küresi mermisi
│   └── UI/
│       ├── MainHUD.tscn              # Responsive üst, alt ve yan arayüz
│       └── FloatingText.tscn         # Hasar ve altın pop-up'ı
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs            # Singleton/Autoload: Altın, dalga, durum ve olaylar
│   │   └── SaveData.cs               # Basit veri serileştirme modeli
│   ├── Combat/
│   │   ├── Hero.cs                   # Kahraman kontrolcüsü, vuruş döngüsü, öfke
│   │   ├── PentagramStats.cs         # ATK, ATK Speed, Lifesteal, Max HP, Range
│   │   ├── PetCompanion.cs           # Sinusoidal hover ve hedefli atış
│   │   ├── Enemy.cs                  # Standart düşman AI, HP ve ödül
│   │   ├── BossEnemy.cs              # Enemy'den türeyen, hasar artıran Boss AI
│   │   ├── WaveSpawner.cs            # Dalga akışı, spawn ve geri çekilme döngüsü
│   │   └── BloodProjectile.cs        # Güdümlü kan küresi mermisi
│   └── UI/
│       ├── MainHUD.cs                # Arayüz denetleyicisi ve buton dinleyicileri
│       ├── FloatingText.cs           # Tween tabanlı uçuşan hasar/altın metni
│       └── TapCombatArea.cs          # Ekrana dokunma/tıklama hasarı alanı
└── Shaders/
    ├── BloodVignette.gdshader         # Öfke modu kırmızı kenar efekti
    └── ParallaxAtmosphere.gdshader   # Sis ve atmosfer efekti
```

---

## 3. Sistem Bileşenleri & Veri Modelleri

### 3.1 GameManager (Autoload Singleton)
- **Sorumluluk:** Tüm oyun oturumu boyunca yaşayan durum yöneticisi.
- **Değişkenler:**
  - `long Gold`: Mevcut altın miktarı.
  - `int CurrentWave`: Mevcut dalga (1, 2, ...).
  - `int HighestWave`: Ulaşılan en yüksek dalga.
  - `bool IsBossWave`: Mevcut dalganın Boss dalgası olup olmadığı (`CurrentWave % 10 == 0`).
  - `bool IsInSafeFarmMode`: Oyuncu boss'a yenilip önceki dalgaya çekildiğinde `true`.
  - `float RagePercentage`: Öfke barı doluluğu (0.0f - 100.0f).
  - `bool IsRageActive`: Berserk modu aktif mi (10 sn sürer).
- **C# Event'leri:**
  - `event Action<long> OnGoldChanged;`
  - `event Action<int, bool> OnWaveChanged;` // wave, isBoss
  - `event Action<float> OnRageChanged;` // percentage
  - `event Action<bool> OnRageStateChanged;` // isActive
  - `event Action OnHeroDied;`

### 3.2 Pentagram İstatistikleri (`PentagramStats.cs`)
5 temel sütun:
1. **ATK:** Taban: 10, Seviye başı +3, Taban Maliyet: 20 altın, Katsayı: x1.15
2. **ATK Speed:** Taban: 1.0 vuruş/sn, Seviye başı +0.05 vuruş/sn (Maks: 3.5), Taban Maliyet: 30 altın, Katsayı: x1.18
3. **Lifesteal:** Taban: %1.0, Seviye başı +0.5% (Maks: %25), Taban Maliyet: 40 altın, Katsayı: x1.22
4. **Max HP:** Taban: 100, Seviye başı +25, Taban Maliyet: 25 altın, Katsayı: x1.15
5. **Range:** Taban: 180px, Seviye başı +10px (Maks: 350px), Taban Maliyet: 20 altın, Katsayı: x1.14

---

## 4. Savaş Döngüsü ve Mekanikler

### 4.1 Kahraman (`Hero.cs`)
- Ekranın sol bölgesinde konuşlanır (`Vector2(320, 680)`).
- **Hedef Bulma:** Menzili içindeki en yakın düşmanı (`WaveSpawner.GetEnemies()`) tespit eder.
- **Saldırı:** Saldırı hızına bağlı zamanlayıcı dolduğunda öne doğru kısa tween savurması yapar, düşmana `ATK` hasarı verir (Öfke modundaysa 2x Kritik).
- **Can Çalma (Lifesteal):** Verilen hasarın yüzdesi kadar kendi canını doldurur (`CurrentHP = Mathf.Min(MaxHP, CurrentHP + heal)`).
- **Öfke Kazanımı:** Her vuruşta öfke barına +%2 ekler.
- **Hasar Alma & Ölüm:** Düşmanlar vurduğunda canı düşer. Can 0 olduğunda `GameManager.Instance.NotifyHeroDied()` çağrılır.

### 4.2 Süzülen Ruhlar / Petler (`PetCompanion.cs`)
- Biri kahramanın sol-üstünde (`offset = (-80, -90)`), diğeri sol-altında (`offset = (-80, 50)`).
- `Position = Hero.Position + offset + Vector2(0, Mathf.Sin(time * 3f + phase) * 12f)`.
- Her 1.5 saniyede bir en öndeki düşmana doğru `BloodProjectile` fırlatır.
- Hasar: `Hero.ATK * 0.4f`.

### 4.3 Tıklama Hasarı (Tap Damage - `TapCombatArea.cs`)
- Savaş alanına her tıklama/dokunma yapıldığında:
  - Tıklanan noktada `FloatingText` veya kan sıçraması parçacığı oluşur.
  - En öndeki düşmana `Hero.ATK * 0.75f` doğrudan hasar verilir.
  - Öfke barına +%1.5 eklenir.

### 4.4 Öfke (Berserk) Modu
- Öfke barı %100 olduğunda HUD'daki butona basılarak tetiklenir.
- 10 saniye sürer.
- Ekranın kenarlarında kızıl pulsasyon (`BloodVignette`) başlar.
- Saldırı hızı 2 katına çıkar, tüm hasarlar 2x Kritik olarak vurulur.
- Süre dolduğunda öfke barı 0'a döner.

---

## 5. Düşmanlar, Boss Enrage ve Dalga Döngüsü

### 5.1 Standart Düşmanlar (`Enemy.cs`)
- Sağdan sola yürür (`speed = 100-140 px/s`).
- Can: `Wave * 25 + 50`.
- Hasar: `Wave * 3 + 5`.
- Kahramanın menziline girdiğinde durur ve saniyede bir vurur.
- Öldüğünde: Altın ödülü (`Wave * 5 + 10`) verir, `FloatingText` ile gösterilir, sahneden silinir.

### 5.2 Boss Düşman (`BossEnemy.cs`)
- Dalga 10'da tek başına spawn olur.
- Boyutu 2x, Canı `Wave * 200 + 400`, Hasarı `Wave * 15 + 20`.
- **Enrage Mekaniği:** Boss savaşı başladıktan sonra her 5 saniyede bir hasarı %25 artar ve rengi/aurası daha koyu kırmızıya kayar.

### 5.3 Dalga Akışı & Sonsuz Farm (`WaveSpawner.cs`)
- Dalga 1-9: Belirli aralıklarla (1.2 sn) 5 düşman spawn edilir. Tümü yok edilince dalga ilerler.
- Dalga 10 (Boss): Boss yenilirse `CurrentWave = 11` olur ve oyun devam eder.
- **Yenilgi (Kahraman Ölürse):**
  - Kahraman anında tam canla diriltilir.
  - Savaş alanındaki tüm düşmanlar temizlenir.
  - `IsInSafeFarmMode = true` yapılır ve dalga bir önceki güvenli dalgaya (örn. Dalga 9) çekilir.
  - Oyuncu bu dalgada sonsuz farm yapar.
  - HUD üzerinde "Boss'a Yeniden Meydan Oku" butonu görünür; basıldığında tekrar Dalga 10 Boss'una geçilir.

---

## 6. Görsel Atmosfer & Responsive UI (1920x1080)

### 6.1 Katmanlı Gotik Parallax
1. **Arka Katman:** Koyu lacivert/siyah gökyüzü, sisli bulutlar ve büyük parlak Kızıl Ay.
2. **Orta Katman:** Gotik Malikane silüeti, kuleler, mezar taşları ve havadaki kırmızı kül parçacıkları.
3. **Ön Katman:** Kan sıçramış antik taş zemin.

### 6.2 Responsive HUD (`MainHUD.cs`)
- **Üst Panel:**
  - Sol: Bölge Başlığı ("Varnath Harabeleri - Malikane Kapısı").
  - Orta: Dalga Göstergesi (Örn: "DALGA 3/10" veya "⚠️ BOSS SAVAŞI").
  - Sağ: Altın Göstergesi ("🪙 1,250 Altın").
- **Sağ Yan Panel:**
  - Dairesel Öfke (Berserk) Butonu.
  - "Boss'a Meydan Oku" Butonu (Sadece Safe Farm modunda görünür).
- **Alt Panel:**
  - 5 adet Pentagram stat kartı (ATK, ATK Speed, Lifesteal, Max HP, Range).
  - Her kart seviye atlatma butonuna sahiptir; yeterli altın varsa vurgulanır, yetersizse pasif kalır.
- **Savaş Alanı Üstü:**
  - Boss Can Barı (Yalnızca Boss dalgasında aktif).
  - Dinamik Hasar & Altın Sayıları (`FloatingText`).

---

## 7. Hata Yönetimi ve Kenar Durumları (Edge Cases)
- **Boş Hedef / Erken Düşman Ölümü:** Kahraman veya pet ateş ederken hedef ölürse mermiler boşa düşmez; en yakın canlı düşmana yönlendirilir veya zararsızca yok edilir.
- **Büyük Sayı Formatlaması:** Altın ve hasar sayıları için `long` / `double` kullanılır; UI'da 1K, 1M, 1B şeklinde okunabilir formatlama fonksiyonu (`FormatNumber`) bulunur.
- **Ekran Boyutu Esnekliği:** Mobil ve farklı masaüstü çözünürlükleri için `stretch/mode = "canvas_items"`, `stretch/aspect = "expand"` ayarlanmıştır.

---

## 8. Doğrulama ve Test Planı
1. **Derleme Testi:** `dotnet build` komutunun 0 hata ve uyarı ile başarıyla tamamlanması.
2. **Godot Çalışma Testi:** Projenin `Godot_v4.7.2-stable_mono_win64_console.exe` ile hatasız açılıp `MainCombat.tscn` sahnesini yüklemesi.
3. **Mekanik Testleri:**
   - Kahramanın otomatik saldırması ve düşmanları kesmesi.
   - Petlerin süzülüp kan küresi fırlatması.
   - Ekrana tıklandığında düşmana tap hasarı verilmesi.
   - Altın toplanıp 5 statın da seviyesinin artırılabilmesi.
   - Öfke barı dolduğunda Berserk butonunun çalışması ve 10 sn sonra normale dönmesi.
   - Dalga 10'da Boss'un gelmesi; ölünce Dalga 11'e geçilmesi; yenilince Dalga 9'da güvenli farm yapılması.
