# 🩸 BLOODSEAL: KAN MÜHRÜNÜN MİRASI
### Oyun Tasarım Belgesi (GDD) & Godot 4 (.NET / C#) Mimari Raporu

---

## 1. Temel Bilgiler ve Sabitlenen Kararlar

| Alan | Karar | Detay |
| :--- | :--- | :--- |
| **Proje Adı** | BloodSeal | Karanlık Gotik Fantastik Evren |
| **Tür** | 2D Karanlık/Gotik Idle RPG | Yandan görünümlü, yarı otomatik, derin hikaye ve istatistik odaklı |
| **Görsel Stil** | Katmanlı 2D Derinlik (Parallax) | 16:9 Yatay ekran. Arka planda karanlık gökyüzü/kızıl ay, harabeler ve zemin katmanı |
| **Karakter Yapısı** | Tek Kahraman + 2 Süzülen Ruh/Pet | Önde devasa kılıcıyla Köken Mührü taşıyıcısı; etrafında süzülen 2 kan/gölge ruhu |
| **Oynanış** | Yarı Otomatik (Semi-Auto) | Kahraman & petler otomatik savaşır; oyuncu dokunarak **Tıklama Hasarı (Tap DMG)** verir ve **Öfke/Yetenek** butonlarını tetikler |
| **Platform** | Çapraz Platform (PC + Mobil) | Hem fareye hem dokunmatik ekrana duyarlı esnek (responsive) arayüz |
| **Motor & Dil** | Godot 4.x (.NET) / C# 8.0+ | İleride olası Unity geçişi için %100 C# ve modüler nesne yönelimli mimari |
| **Renderer** | Compatibility (OpenGL 3) | PC, tüm mobil cihazlar ve web için maksimum kararlılık ve düşük pil tüketimi |

---

## 2. Hikaye ve Atmosfer (Lore)

### Evren ve Arka Plan
Varnath dünyasının kadim dengesini sağlayan 13 mühürden 12'si, kendi sapkın inançlarını "kurtuluş" olarak pazarlayan **Kızıl Ahit (Crimson Covenant)** tarikatı tarafından ele geçirilmiştir. Geriye yalnızca oyuncunun bedenine mühürlenen **Köken Mührü** kalmıştır.

### Prologue ve Uyanış
Darius'un fedakarlığı ve emaneti olan kan tılsımıyla yıkık Malikaneye (Kan Odası'na) ulaşan isimsiz savaşçı, mührün uyanışıyla adını fısıldar: `[OYUNCU İSİMLENDİRME EKRANI]`.

### Hikaye Anlatım Mekaniği (Lore Drops)
* Oyun akışını kesen uzun diyaloglar yerine; Boss'lardan düşen kanlı yüzükler, yırtık aile portreleri ve mühürlü parşömenler envantere düşer.
* Bu parçalar, Malikane'nin kilitli odalarını ve araştırma kütüphanesini açan anahtarlar olarak kullanılır.
* Seçilen sokak geçmişine göre başlangıç haritasının aydınlatma ve parçacık renk paleti dinamik olarak değişir.

---

## 3. Karakter Gelişimi & Sistemler

### A. Aşama 1: Mutasyon Şekli (Kan Soyu Seçimi)
Oyun başlangıcında seçilen kalıcı temel pasif:
1. **Kemik Dokulu:** +%10 Maksimum Can
2. **Gölge Damarlı:** +Ekstra Saldırı Menzili
3. **Kan Pençeli:** Doğuştan Can Çalma (Lifesteal)
4. **Çelik Dokulu:** Sabit Hasar Azaltma ve Hasar Yansıtma (Thorns)
5. **Ruh Emici:** Bekleme Süresi Azaltma + Erken aşamada aktif *Kan Mızrağı* yeteneği

### B. Aşama 2: Sokak Geçmişi
* **Kafes Dövüşçüsü:** +%10 Saldırı Gücü
* **Sokak Hırsızı:** +%10 Ekstra XP & Altın
* **Eski Paralı Asker:** +%5 Saldırı Hızı
* **Yeraltı Kimyageri:** İksir ve elementel hasar artışı
* **Çete Lideri:** Savaşta destekçi çırak çağırma şansı

### C. Pentagram Gelişim Sistemi (Kan Mührü)
Altın harcanarak seviye atlatılan 5 temel istatistik:
1. **Saldırı Gücü (ATK)**
2. **Saldırı Hızı (ATK Speed)**
3. **Can Çalma (Lifesteal %)**
4. **Maksimum Can (Max HP)**
5. **Saldırı Menzili (Range)**

---

## 4. Savaş Döngüsü ve Idle Mekanikleri

```text
[İlerleme / Dalga Temizleme] ──(Kazanma)──> [Ganimet + Otomatik İlerleme]
           │                                            │
        (Ölüm)                                       (10. Dalga)
           │                                            ▼
[Önceki Aşamaya Düşüp Sonsuz Farm] <────────── [Boss Savaşı: Kademeli Öfkelenme]
```

1. **Ölüm & Farm Döngüsü:** Karakter yenilirse oyun bitmez; otomatik olarak bir önceki güvenli dalgaya çekilir ve oyuncu güçlendirme yapana kadar kesintisiz farm yapmaya devam eder.
2. **Boss Öfkelenme (Enrage):** Zaman sayacı yerine savaş uzadıkça Boss'un hasarı kademeli olarak artar.
3. **Öfke (Rage) Patlaması:** Vurdukça dolan kırmızı öfke barı; aktive edildiğinde 10 saniye boyunca çılgın saldırı hızı ve kritik hasar sağlar.
4. **Çevrimdışı Farm (Offline Progress):** 
   * Başlangıç sınırı: **6 Saat**.
   * Malikane Kütüphanesi/Akademisi araştırmaları ile süre ve kazanım çarpanları kalıcı olarak artırılır.
5. **Uyanış (Awakening / Rebirth):** Tıkanılan seviyelerde karakter sıfırlanır, karşılığında **Uyanış Puanları** kazanılır ve sadece bu sisteme özel benzersiz yetenek ağacında harcanır.

---

## 5. Malikane Kapısı Çatışması (Kesintisiz Idle Akışı)

Oyuncu 5. seviyeye ulaştığında oyun durmaz; arka planda düşman kesilmeye devam ederken ekranda bir bildirim ikonu belirir. Oyuncu tıkladığında kapı yaklaşımını seçer:
* **Ön Kapıyı Kır:** Zırhlı boss (Ödül: Saldırı Gücü/HP Rünü)
* **Kanalizasyondan Sız:** Düşük zırhlı boss (Ödül: Hız/Menzil Rünü)
* **Kanınla Mühürle:** Büyülü boss (Ödül: Can Çalma Rünü + İlk Hikaye Parşömeni)
* **Rüşvet / Çatıdan Sızma:** Geçmişe özel boss zayıflatmasıyla başlama.

---

## 6. Godot 4 (.NET / C#) Mimari Şeması

* **`GameManager.cs` (Autoload / Singleton):**
  * Para birimleri (Altın, Ruh Taşları, Uyanış Puanları), mevcut dalga ve offline zaman hesabı (`DateTimeOffset`).
* **`Hero.cs` (`CharacterBody2D`):**
  * Pentagram statları, Lifesteal, Rage barı, otomatik hedef alma ve vuruş animasyonları.
* **`PetCompanion.cs` (`Node2D`):**
  * Kahramanın çevresinde sinusoidal süzülme ve otomatik mermi/büyü fırlatma.
* **`Enemy.cs` / `BossEnemy.cs` (`CharacterBody2D`):**
  * Canavar yapay zekası, kademeli hasar artışı (Enrage) ve ölümünde altın/parşömen bırakma.
* **`WaveSpawner.cs` (`Node2D`):**
  * Düşman dalgaları ve ölüm halinde güvenli aşamaya geri çekilme mantığı.
* **`UIManager.cs` (`CanvasLayer`):**
  * Pentagram yükseltme butonları, Öfke butonu, hasar pop-up'ları (`FloatingText.cs`) ve Malikane araştırma sekmesi.

---

## 7. Antigravity IDE Master Prompt'u

```markdown
Sen Godot 4 (.NET / C#) konusunda uzman bir oyun mimarisisin.
"BloodSeal" projemizi C:\Users\Partridge\Desktop\blood-seal dizininde, sağlanan GDD ve mimari doğrultusunda sıfırdan kuracağız.

Temel Kurallar:
1. Platform: 1920x1080 (16:9 Yatay), Compatibility Renderer, PC ve Mobil uyumlu responsive UI.
2. Sahne: 2D katmanlı Parallax (gökyüzü, harabeler, zemin), tek ana kahraman (Blood Berserker), etrafında süzülen 2 yardımcı ruh (PetCompanion) ve sağdan gelen düşman dalgaları.
3. Sistemler (C#):
   - Pentagram Stat Sistemi: ATK, ATK Speed, Lifesteal, Max HP, Range.
   - Savaş & Öfke Döngüsü: Otomatik vuruş + Ekran tıklama hasarı (Tap Damage) + Vurdukça dolan Berserk Öfke modu.
   - Düşman & Boss: Kademeli hasar artıran Boss Enrage mekaniği; kahraman ölürse bir önceki aşamada otomatik farm döngüsü.
   - Çevrimdışı İlerleme: 6 saatlik baz süre ile DateTimeOffset tabanlı altın hesaplama.
   - Arayüz: Üst panel (Dalga, Altın), Sağ/Sol aksiyon butonları (Öfke/Yetenek), Alt panel (Pentagram yükseltmeleri).

Lütfen gerekli tüm .cs sınıflarını, .csproj ve project.godot ayarlarını, sahneleri oluştur ve projenin F5 ile doğrudan oynanabilir olmasını sağla.
```
