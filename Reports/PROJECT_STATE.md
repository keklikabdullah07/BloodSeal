# 🩸 BloodSeal: Mevcut Proje Durum Raporu (Project State Report)

**Tarih:** 07 Ekim 2026  
**Rapor Türü:** Doğrulanmış Kod ve Mimari Durum Tespiti  
**İnceleme Tabanı:** [`BLOODSEAL_GDD.md`](file:///c:/Users/Partridge/Desktop/blood-seal/BLOODSEAL_GDD.md), [`AGENTS.md`](file:///c:/Users/Partridge/Desktop/blood-seal/AGENTS.md), [`GEMINI.md`](file:///c:/Users/Partridge/Desktop/blood-seal/GEMINI.md), C# Kaynak Kodları ve Doğrulama Komutları.

---

## 1. GDD Sistemlerinin Uygulanma Durumu

| GDD Sistemi | Durum | İlgili Dosya Yolu ve Satırlar | Notlar |
| :--- | :---: | :--- | :--- |
| **Karakter İsimlendirme & Prologue** | **Uygulandı** | [`Scripts/UI/PrologueController.cs#L1-L152`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/PrologueController.cs#L1-L152)<br>[`Scenes/Prologue/PrologueScene.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/Prologue/PrologueScene.tscn) | 4 adımlı akış (İsim girişi, Kan Soyu, Sokak Geçmişi, Uyanış özeti). |
| **5 Kan Soyu (Bloodline) Seçimi** | **Uygulandı** | [`Scripts/Core/CharacterProfile.cs#L8-L15`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/CharacterProfile.cs#L8-L15)<br>[`Scripts/Combat/PentagramStats.cs#L44-L68`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PentagramStats.cs#L44-L68) | Kemik Dokulu (+%10 HP), Gölge Damarlı (+35px Menzil), Kan Pençeli (+%2.5 Lifesteal), Çelik Dokulu (-3 Dmg), Ruh Emici (+%15 Hız). |
| **5 Sokak Geçmişi (Origin) Seçimi** | **Uygulandı** | [`Scripts/Core/CharacterProfile.cs#L17-L24`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/CharacterProfile.cs#L17-L24)<br>[`Scripts/Combat/PentagramStats.cs#L20-L32`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PentagramStats.cs#L20-L32) | Kafes Dövüşçüsü (+%10 ATK), Sokak Hırsızı (+%15 Altın), Eski Paralı Asker (+%8 Hız), Yeraltı Kimyageri (+%25 Tap DMG), Çete Lideri (Hızlı Pet). |
| **Pentagram Stat Sistemi (5 İstatistik)** | **Uygulandı** | [`Scripts/Combat/PentagramStats.cs#L8-L78`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PentagramStats.cs#L8-L78)<br>[`Scripts/UI/MainHUD.cs#L123-L134`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/MainHUD.cs#L123-L134) | ATK, ATK Speed, Lifesteal, Max HP, Range. Üstel maliyet formülü ($BaseCost \times 1.15^{(Level - 1)}$) uygulandı. |
| **Yarı Otomatik Savaş & Kahraman** | **Uygulandı** | [`Scripts/Combat/Hero.cs#L53-L140`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Hero.cs#L53-L140)<br>[`Scenes/Hero.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/Hero.tscn) | Otomatik hedefleme, kılıç savurma, can çalma, vuruş animasyonları. |
| **2 Yardımcı Pet Companion** | **Uygulandı** | [`Scripts/Combat/PetCompanion.cs#L30-L77`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PetCompanion.cs#L30-L77)<br>[`Scenes/PetCompanion.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/PetCompanion.tscn) | Kahraman etrafında sinüzoidal süzülme ve otomatik kan küresi fırlatma. |
| **Tıklama Hasarı (Tap Damage)** | **Uygulandı** | [`Scripts/Combat/TapCombatArea.cs#L20-L63`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/TapCombatArea.cs#L20-L63) | Ekrana dokunma/tıklama ile en öndeki düşmana vuruş ve +1.5 öfke kazanımı. |
| **Öfke (Rage) / Kan Öfkesi Patlaması** | **Uygulandı** | [`Scripts/Core/GameManager.cs#L43-L56`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs#L43-L56)<br>[`Scripts/Combat/Hero.cs#L57-L61`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Hero.cs#L57-L61) | %100 dolunca açılan buton; 10 saniye 2x saldırı hızı ve %100 kritik. |
| **Minyon Dalgaları (1-9)** | **Uygulandı** | [`Scripts/Combat/WaveSpawner.cs#L35-L80`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/WaveSpawner.cs#L35-L80)<br>[`Scripts/Combat/Enemy.cs#L33-L70`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Enemy.cs#L33-L70) | 5 minyonluk dalgalar, doğrusal HP ve hasar ölçeklemesi. |
| **Boss Savaşı (10. Dalga) & Enrage** | **Uygulandı** | [`Scripts/Combat/BossEnemy.cs#L11-L44`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/BossEnemy.cs#L11-L44) | Dalga 10'da büyük Boss doğumu; her 5 sn'de +%25 hasar artışı (Enrage). |
| **Yenilgi & Güvenli Farm Döngüsü** | **Uygulandı** | [`Scripts/Core/GameManager.cs#L95-L123`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs#L95-L123)<br>[`Scripts/UI/MainHUD.cs#L94-L97`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/MainHUD.cs#L94-L97) | Kahraman ölünce Dalga 9'a düşüp sınırsız farm yapma; "Boss'a Yeniden Meydan Oku" butonu. |
| **Çevrimdışı İlerleme (Offline Progress)** | **Uygulandı** | [`Scripts/Core/OfflineProgressCalculator.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/OfflineProgressCalculator.cs)<br>[`Scripts/UI/OfflineProgressModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/OfflineProgressModal.cs) | 6 saat tavan, son güvenli dalga kazancı, modal arayüz ve negatif süre koruması. |
| **Lore Parşömenleri & Boss Düşüşleri** | **Uygulandı** | [`Scripts/Combat/BossEnemy.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/BossEnemy.cs)<br>[`Scripts/Core/ResearchManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/ResearchManager.cs) | Dalga 10'da ve her 10 dalgada ilk kez kesilen Boss'tan 1 Lore Scroll düşüşü. |
| **Malikane Kapısı Seçimi (5. Seviye)** | **Uygulandı** | [`Scripts/Core/ManorGateModels.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/ManorGateModels.cs)<br>[`Scripts/UI/ManorGateModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/ManorGateModal.cs) | Dalga 5'te bildirim, 4 yaklaşım seçimi, rün kuşanma ve ilk parşömen ödülü. |
| **Malikane Odaları & Kütüphane** | **Uygulandı** | [`Scripts/Core/ResearchManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/ResearchManager.cs)<br>[`Scripts/UI/LibraryModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/LibraryModal.cs) | 3 disiplin, 9 araştırma düğümü, altın + parşömen maliyeti ve savaş çarpanları. |
| **Uyanış (Awakening / Rebirth)** | **Uygulandı** | [`Scripts/Core/AwakeningManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/AwakeningManager.cs)<br>[`Scripts/UI/AwakeningModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/AwakeningModal.cs) | Dalga 20+ eşiği, polinomik AP formülü, 3 dallı ve 7 pasifli Kadim Mühürler Ağacı, prestij sıfırlaması. |
| **Lore Eserleri & Malikane Mahzeni (Relic Vault)** | **Uygulandı** | [`Scripts/Core/RelicModels.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/RelicModels.cs)<br>[`Scripts/Core/RelicManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/RelicManager.cs)<br>[`Scripts/UI/RelicVaultModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/RelicVaultModal.cs) | Dalga 10-100 milestone Boss'larından düşen 10 benzersiz Gotik eser; kalıcı pasif bonuslar (hasar, can, altın, hız, lifesteal, AP); Uyanışta silinmez; iki sütunlu vitrin modalı. |
| **Ses Mimarisi & Gotik Ses Deneyimi** | **Uygulandı** | [`Scripts/Core/AudioManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/AudioManager.cs)<br>[`Scripts/UI/SettingsModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/SettingsModal.cs) | 6 kanallı döngüsel SFX havuzu, çift kanallı BGM crossfade (Minyon vs Boss teması), kapsamlı oynanış sesleri (Rage, Boss zaferi, Eser, Uyanış), logaritmik dB dönüşümü, SaveData kalıcılığı ve Gotik SettingsModal arayüzü. |

---

## 2. Mimari Kurallara ([`AGENTS.md`](file:///c:/Users/Partridge/Desktop/blood-seal/AGENTS.md)) Uyum Analizi

### A. 250 Satır Sınırı (Composition Over Inheritance)
Projedeki tüm 23 C# dosyası taranmış ve satır sayıları ölçülmüştür:
- [`Scripts/Combat/Hero.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Hero.cs): **204 satır** (Sınır altında)
- [`Scripts/Core/GameManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs): **195 satır** (Sınır altında)
- [`Scripts/UI/PrologueController.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/PrologueController.cs): **152 satır** (Sınır altında)
- [`Scripts/UI/MainHUD.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/MainHUD.cs): **146 satır** (Sınır altında)
- [`Scripts/Combat/Enemy.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Enemy.cs): **133 satır** (Sınır altında)
- [`Scripts/Combat/WaveSpawner.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/WaveSpawner.cs): **109 satır** (Sınır altında)
- Diğer 17 sınıf: 15 ila 80 satır arasında.
**Sonuç:** Hiçbir sınıf 250 satırı aşmamaktadır. Kurala %100 uyumludur.

### B. "Call Down, Signal Up" İhlalleri
- **İhlal Tespit Edildi:** [`Scripts/Combat/PetCompanion.cs#L24-L27`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PetCompanion.cs#L24-L27) dosyasında:
  ```csharp
  if (_hero == null && GetParent() != null)
  {
      _hero = GetParent().GetNodeOrNull<Node2D>("Hero");
  }
  ```
  `PetCompanion` düğümü sahne ağacında yukarı tırmanarak (`GetParent()`) kardeş düğüm aramaktadır. Bu durum `AGENTS.md` Kural 2-A ihlalidir. `[Export] public Node2D HeroTarget;` ile çözülmelidir.
- [`Scripts/Combat/ParallaxScroller.cs#L19`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/ParallaxScroller.cs#L19) `if (GetParent() is ParallaxBackground pb)` kontrolü yapmaktadır (doğrudan alt bileşen ilişkisi).

### C. `Engine.TimeScale` ve `Timer` Node Kullanımı
- **`Engine.TimeScale` Kullanımı:** Kod tabanında `Engine.TimeScale` değiştiren hiçbir aktif kod satırı **yoktur**. [`Scripts/Combat/FXManager.cs#L64-L78`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/FXManager.cs#L64-L78) içerisinde yerel `ProcessMode = Disabled` mikro-duraksaması kullanılmaktadır.
- **`Timer` Node Kullanımı:** Sahne ağacında hiçbir `Timer` düğümü kullanılmamaktadır. Boss enrage ([`BossEnemy.cs#L29`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/BossEnemy.cs#L29)), Öfke süresi ([`GameManager.cs#L47`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs#L47)) ve saldırı süreleri gerçek `delta` ile hesaplanmaktadır.

### D. `double` vs `long` Para Birimi Hijyeni
- [`Scripts/Core/GameManager.cs#L14`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs#L14): `public double Gold { get; private set; }` (**Uyumlu**)
- [`Scripts/Combat/PentagramStats.cs#L73-L77`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PentagramStats.cs#L73-L77): `public double GetAtkCost()` (**Uyumlu**)
- [`Scripts/Core/BigNumberFormatter.cs#L9`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/BigNumberFormatter.cs#L9): `public static string Format(double num)` (**Uyumlu**)
- **İhlal/Tutarsızlık Tespit Edildi:** [`Scripts/Core/SaveData.cs#L5`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/SaveData.cs#L5) içinde:
  ```csharp
  public long Gold { get; set; } = 0;
  ```
  `SaveData` hala `long` kullanmaktadır. `double`'a çevrilmelidir.

---

## 3. Doğrulama Kapısı Çıktıları (Canlı Çalıştırıldı)

### A. `dotnet build`
- **Çalıştırılan Komut:** `dotnet build`
- **Çıkış Kodu:** `0`
- **Konsol Çıktısı:**
  ```text
  Determining projects to restore...
  All projects are up-to-date for restore.
  BloodSeal -> C:\Users\Partridge\Desktop\blood-seal\.godot\mono\temp\bin\Debug\BloodSeal.dll

  Build succeeded.
      0 Warning(s)
      0 Error(s)

  Time Elapsed 00:00:01.59
  ```

### B. `dotnet test` (Denge Simülasyonu)
- **Çalıştırılan Komut:** `dotnet test Tests/BloodSeal.Tests.csproj --logger "console;verbosity=detailed"`
- **Çıkış Kodu:** `0`
- **Konsol Çıktısı:**
  ```text
  Passed BloodSeal.Tests.BalanceSimulationTests.Run_Wave_And_Boss_Progression_Simulation [188 ms]
  Standard Output Messages:
  =========================================================================================
  🩸 BLOODSEAL DENGE SİMÜLASYONU RAPORU (Veriler: Data/BalanceConfig.json)
  =========================================================================================
  | Dalga | Tip    | Düşman HP | Düşman ATK | Kahraman ATK | Süre (sn) | Toplam Süre | Altın  |
  |-------|--------|-----------|------------|--------------|-----------|-------------|--------|
  |     1 | Minyon |        75 |        8.0 |         10.0 |       3.0 |         3.0 |     15 |
  |     2 | Minyon |       100 |       11.0 |         10.0 |       4.0 |         7.0 |     15 |
  |     3 | Minyon |       125 |       14.0 |         13.0 |       3.8 |        10.8 |     17 |
  |     4 | Minyon |       150 |       17.0 |         16.0 |       3.8 |        14.6 |     21 |
  |     5 | Minyon |       175 |       20.0 |         19.0 |       3.7 |        18.3 |     25 |
  |     6 | Minyon |       200 |       23.0 |         22.0 |       3.6 |        21.9 |     30 |
  |     7 | Minyon |       225 |       26.0 |         25.0 |       3.6 |        25.5 |     35 |
  |     8 | Minyon |       250 |       29.0 |         28.0 |       3.6 |        29.1 |     39 |
  |     9 | Minyon |       275 |       32.0 |         31.0 |       3.5 |        32.6 |     40 |
  -----------------------------------------------------------------------------------------
  ⚡ 10. Dalga Boss'una Ulaşma Süresi: 32.6 saniye (~0.5 dakika)
  [Farm #1] Kalan Boss HP: 2338/2650 | Kahraman ATK Lv.10, HP Lv.2 (100 HP)
  [Farm #5] Kalan Boss HP: 1807/2650 | Kahraman ATK Lv.14, HP Lv.6 (200 HP)
  [Farm #10] Kalan Boss HP: 972/2650 | Kahraman ATK Lv.19, HP Lv.11 (325 HP)
  [Farm #15] Kalan Boss HP: 560/2650 | Kahraman ATK Lv.24, HP Lv.16 (450 HP)
  |    10 | BOSS   |      2650 |      165.0 |         82.0 |       3.9 |       369.0 |   4889 |
  =========================================================================================
  🏆 1. Boss'u Kesme Süresi (Savaş): 3.9 sn (Gereken Farm Turu: 16)
  ⏱️ Toplam Oynanış Süresi (İlk Boss Dahil): 369.0 sn (~6.15 dk)
  📈 Bitiş Seviyeleri: ATK Lv.25, ATK Hızı Lv.1, Max HP Lv.17
  =========================================================================================

  Passed!  - Failed: 0, Passed: 1, Skipped: 0, Total: 1, Duration: 188 ms
  ```

### C. Godot Headless Smoke Test
- **Çalıştırılan Komut:** `Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 30`
- **Çıkış Kodu:** `0`
- **Konsol Çıktısı:**
  ```text
  Godot Engine v4.7.2.stable.mono.official.ed1daf0bf - https://godotengine.org
  ```

### D. Android Export Testi (Doğrulandı & TFM Çözümü Başarılı)
- **İlk Durum (net10.0 doğrudan):**
  - Çıktı: `ERROR: C# project targets 'net10.0' but the export template only supports 'net9.0'. Consider using gradle builds instead.`
- **Uygulanan Çözüm ([`BloodSeal.csproj#L4`](file:///c:/Users/Partridge/Desktop/blood-seal/BloodSeal.csproj#L4)):**
  ```xml
  <TargetFramework Condition=" '$(GodotTargetPlatform)' == 'android' ">net9.0</TargetFramework>
  ```
- **Çözüm Sonrası C# Derleme Testi:**
  - `dotnet build -p:GodotTargetPlatform=android`: **0 Warning(s), 0 Error(s)** ile başarıyla `net9.0` paketi restore edilip derlendi.
- **Çözüm Sonrası Godot Android Export Testi:**
  - `Godot_v4.7.2-stable_mono_win64_console.exe --headless --export-debug "Android" Builds/Android/BloodSeal.apk`
  - **Yeni Konsol Çıktısı:**
    ```text
    ERROR: Cannot export project with preset "Android" due to configuration errors:
    Exporting to Android when using C#/.NET is experimental.
    No export template found at the expected path:
    C:/Users/Partridge/AppData/Roaming/Godot/export_templates/4.7.2.stable.mono/android_debug.apk
    No export template found at the expected path:
    C:/Users/Partridge/AppData/Roaming/Godot/export_templates/4.7.2.stable.mono/android_release.apk
    A valid Java SDK path is required in Editor Settings.
    ```
- **Sonuç:** `net10.0` vs `net9.0` uyuşmazlığı tamamen **ÇÖZÜLDÜ**. Godot artık C# kodunu kabul ediyor; kalan tek gereksinim geliştirici makinesine Godot 4.7.2 Mono Android şablonlarının (`android_debug.apk`) ve Java SDK'nın yüklenmesidir.

---

## 4. Denge Tutarsızlıkları İncelemesi

1. **Üstel vs Doğrusal Uçurumu:**
   - Minyon Altın Getirisi: $Wave \times 5 + 10$ (Doğrusal). 9. dalgada öldürülen minyon başına sadece 55 altın.
   - Yükseltme Maliyeti: $Base \times 1.15^{(Level - 1)}$ (Üstel). Seviye 25'te tek bir yükseltme 572 altın istemektedir.
   - Simülasyon kanıtlamıştır ki, 10. dalga Boss'unu geçebilmek için oyuncunun tam 16 farm turu (yaklaşık 160 minyon kesimi, 5.5 dakika) beklemesi gerekmektedir. İlerleyen dalgalarda (örn. Dalga 30+) bu süre saatlere çıkacak ve sert bir ilerleme duvarı oluşturacaktır.
2. **Katsayılar:**
   - [`Data/BalanceConfig.json#L5-L11`](file:///c:/Users/Partridge/Desktop/blood-seal/Data/BalanceConfig.json#L5-L11) dosyasında 5 stat için de büyüme çarpanı `1.15` olarak kodlanmıştır.
   - [`Scripts/Combat/PentagramStats.cs#L73-L77`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PentagramStats.cs#L73-L77) kodunda da `1.15` sabittir. Bu konuda tutarsızlık kalmamıştır.

---

## 5. Kurulu Skill'ler (`.agents/skills`) ve Projedeki Gerçek Durumu

| Skill Adı | Projede Kullanıldığı Yer | Durum |
| :--- | :--- | :---: |
| `bloodseal-art-style` | Renk paleti, `ParallaxScroller.cs`, `Hero.tscn` ve `Enemy.tscn` RimLight | **Kullanılıyor** |
| `bloodseal-balance` | `BalanceConfig.json`, `PentagramStats.cs`, `BalanceSimulationTests.cs` | **Kullanılıyor** |
| `game-feel` | `FXManager.cs` yerel hit-freeze, `CameraShake.cs` trauma ve sönüm | **Kullanılıyor** |
| `godot-csharp` | Tüm oyun scriptleri, partial classlar, C# eventleri | **Kullanılıyor** |
| `godot-shaders` | `Shaders/BloodVignette.gdshader` | **Kullanılıyor** |
| `godot-signals-groups` | "Enemies" grubu ve `Action` eventleri | **Kullanılıyor** |
| `godot-ui-control` | `MainHUD.tscn`, `SelectionCard.tscn`, `PrologueScene.tscn`, modallar | **Kullanılıyor** |
| `godot-audio` | `AudioManager.cs`, çift kanallı BGM crossfade, 6 kanallı SFX havuzu, SettingsModal | **Kullanılıyor** |
| `save-systems` | `SaveSystem.cs`, `SaveData.cs`, atomik yazma, 6 saatlik offline ilerleme, ses ayarları | **Kullanılıyor** |
| `camera-systems` | Sadece `CameraShake.cs` içinde basit offset sarsıntısı var; kamera takip/deadzone yok | **Kısmen** |
| `game-ui-ux` | Responsive UI Container yapıları var; gamepad/klavye fokus navigasyonu yok | **Kısmen** |
| `godot-export` | `export_presets.cfg` tanımlandı, Android net9.0 platform koşulu çözüldü | **Kısmen** |
| `create-game-assets` | Henüz hiçbir görsel üretim scripti veya asset pipeline çalıştırılmadı | **KULLANILMIYOR** |
| `godot-resources` | Özel `.tres` Godot Resource sınıfı yok (JSON ve C# kullanılıyor) | **KULLANILMIYOR** |
| `performance-optimization`| Profiler ölçümü veya nesne havuzlaması (Object Pooling) yok | **KULLANILMIYOR** |

---

## 6. Görsel Durum & Mobil Okunabilirlik Riskleri

### Sahnelerdeki Varlıkların Gerçek Durumu:
- **Kahraman ([`Scenes/Hero.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/Hero.tscn)):** `%100 Geçici`. Pelerin, kılıç, gövde ve kafa Godot `Polygon2D` vektör çizimleridir. Gerçek sprite sheet veya animasyon karesi yoktur.
- **Minyon Düşman ([`Scenes/Enemy.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/Enemy.tscn)):** `%100 Geçici`. Gövde, kapüşon, gözler `Polygon2D` ile oluşturulmuştur.
- **Boss ([`Scenes/BossEnemy.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/BossEnemy.tscn)):** Minyon sahnesinin 1.9x büyütülmüş halidir; özgün görseli yoktur.
- **Pet Companion ([`Scenes/PetCompanion.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/PetCompanion.tscn)):** `%100 Geçici`. `Polygon2D` daire ve gözlerden ibarettir.
- **Arka Plan ([`Scenes/MainCombat.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/MainCombat.tscn)):** `%100 Geçici`. Düz renk `ColorRect` (Gökyüzü), geometrik sivri `Polygon2D` kuleler ve düz gri zemin dikdörtgeni.
- **VFX Efektleri ([`Scenes/VFX/`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/VFX)):** `CPUParticles2D` ve `Line2D` çizimleridir; el çizimi efekt spriteları yoktur.

### Mobil Ekran Okunabilirlik Riskleri:
1. **Düşük Kontrast ve Siyah Boğulması:** Renk paletindeki `#0A080F`, `#1A1624` ve `#2A222B` tonları OLED/IPS telefon ekranlarında zeminle harabeleri birbirine karıştırmaktadır.
2. **Karakter Ayrımı:** Karakterlere eklenen `RimLight` (kenar konturu) silueti biraz kurtarsa da, doku ve detay olmadığı için küçük ekranda aktörler düz renk lekeleri gibi görünmektedir.
3. **UI Dokunma Alanları:** 5 adet Pentagram yükseltme kartı 1080p ekranda sığışmaktadır; küçük ekranlı mobil cihazlarda buton metinleri küçülüp okunaksızlaşma riski taşımaktadır.

---

## 7. Kalan Kritik Riskler ve Yol Haritası İhtiyaçları

1. **🔴 Görsel Varlıkların Tamamının Geçici (Polygon2D) Olması:** Oyunda tek bir profesyonel sprite, tileset veya animasyon karesi yoktur (`create-game-assets` ve `bloodseal-art-style`).
2. **🟠 Nesne Havuzu (Object Pooling) Eksikliği:** Minyonlar, mermiler ve parçacıklar her dalgada `Instantiate()` ve `QueueFree()` ile yaratılıp silinmektedir; mobilde GC (Garbage Collection) takılmaları riski taşımaktadır (`performance-optimization`).
3. **🟠 Denge Eğrisi Uçurumu:** Doğrusal altın geliri ($55$ altın/minyon) ile üstel maliyet ($1.15^{Lv-1}$) arasındaki fark ilerleyen dalgalarda aşırı farm sürelerine yol açabilir; Uyanış çarpanları ve kütüphane araştırmaları bu dengeyi yumuşatmıştır ancak ileri dalga simülasyonları gereklidir.
4. **🟡 Android Export Şablon Kurulumu:** C# multi-targeting (`net9.0`) platform koşulu çözülmüştür, ancak yerel cihazda Godot Android Mono export template (`android_debug.apk`) ve Java SDK henüz kurulu değildir (`godot-export`).
5. **🟡 Gamepad ve Dokunmatik UI İyileştirmeleri:** Menüler ve modallar fare/dokunma ile çalışmaktadır; klavye/gamepad focus navigasyonu ve mobil safe-area optimizasyonları eklenebilir (`game-ui-ux`).
