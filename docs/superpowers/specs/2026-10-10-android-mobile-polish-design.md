# 📱 BloodSeal: Android Mobil Optimizasyonu & Dokunmatik Polish Tasarım Dokümanı

## 1. Genel Bakış
Bu doküman, **BloodSeal**'ın Android mobil cihazlarda akıcı, profesyonel, ergonomik ve haptik geri bildirimli bir oyun deneyimi sunması için gerekli mobil optimizasyon mimarisini tanımlar.
Sistem; cihaz haptik motoru (`HapticManager`), dinamik ekran çentiği / safe-area yöneticisi (`SafeAreaHandler`), çoklu dokunmatik savaş alanı (`TapCombatArea` multi-touch) ve Android export/ayar konfigürasyonlarını kapsar.

---

## 2. Mimari Bileşenler & Sorumluluklar

### A. Haptik Geri Bildirim Servisi (`HapticManager.cs`)
- **Konum:** `Scripts/Core/HapticManager.cs`
- **Tür:** Saf C# Singleton Servisi (`HapticManager.Instance`).
- **Sorumluluk:**
  - `Input.VibrateHandheld(int ms)` çağrılarını güvenle kapsüllemek.
  - Mobil olmayan platformlarda (Windows/Linux) veya haptik ayarı kapalı olduğunda sıfır hata/sessiz çalışma.
  - Vuruş hissi için 3 ön tanımlı titreşim şiddeti:
    1. `VibrateLight()` (~12-15 ms): Normal ekran tıklamaları / tap vuruşları, menü buton basımları.
    2. `VibrateMedium()` (~30-35 ms): Kritik vuruşlar, eşya geliştirme (+1), yoldaş mermi isabetleri.
    3. `VibrateHeavy()` (~65-80 ms): Boss yenilgisi, Berserk öfke modu aktivasyonu, Uyanış (Rebirth).
- **Ayar Entegrasyonu:** `IsHapticsEnabled` boolean özelliği; `SettingsModal` üzerinden değiştirilebilir ve `SaveData` ile kalıcı olarak saklanır.

### B. Mobil Güvenli Alan / Çentik Koruyucu (`SafeAreaHandler.cs`)
- **Konum:** `Scripts/UI/SafeAreaHandler.cs`
- **Tür:** `Node` veya `Control` yardımcısı.
- **Sorumluluk:**
  - `DisplayServer.GetDisplaySafeArea()` ve pencere boyutunu (`DisplayServer.WindowGetSize()`) karşılaştırarak ekran çentiği (notch), kamera deliği ve yuvarlatılmış köşeleri hesaplar.
  - `TopBar` ve `BottomPanel` gibi kenara yaslanan `MarginContainer` düğümlerinin sol ve sağ marjinlerini dinamik olarak günceller.
  - Çentikli ekranlarda butonların kamera deliği altında kalmasını engeller; düz ekranlarda (veya masaüstünde) standart marjinleri korur.

### C. Çoklu Dokunmatik (Multi-Touch) Savaş Alanı (`TapCombatArea.cs` Güncellemesi)
- **Konum:** `Scripts/Combat/TapCombatArea.cs`
- **Sorumluluk:**
  - `InputEventScreenTouch` içindeki `touchEvent.Index` değerini dinleyerek aynı anda 2 veya 3 parmakla yapılan dokunuşları ayrı ayrı işler.
  - Her dokunuş için:
    1. İlgili parmak koordinatında `TapRipple` görsel efekti üretilir.
    2. En öndeki düşmana bağımsız tap hasarı uygulanır ve öfke kazanımı eklenir.
    3. `HapticManager.Instance.VibrateLight()` tetiklenir.
  - Çoklu dokunmatik spam'ini dengeli tutmak için saniyede maks 15 dokunuş sınırlandırıcı (anti-autoclicker / macro protection) uygulanır.

### D. Ayarlar ve Kalıcılık Entegrasyonu (`SettingsModal.cs` & `SaveData.cs`)
- **`SaveData.cs`:** `public bool IsHapticsEnabled { get; set; } = true;` alanı eklenir.
- **`SaveSystem.cs`:** Kayıt ve yükleme esnasında bu alan serileştirilir.
- **`SettingsModal.cs`:** Koyu gotik bir buton eklenir: `📳 Titreşim: [AÇIK / KAPALI]`. Tıklandığında durum değişir, `HapticManager`'a bildirilir ve anlık test titreşimi verilir.

### E. Proje ve Android Export Yapılandırması (`project.godot` & `export_presets.cfg`)
- **`project.godot`:**
  - `input_devices/pointing/emulate_touch_from_mouse=true`
  - `input_devices/pointing/emulate_mouse_from_touch=true`
  - `display/window/stretch/mode="canvas_items"`
  - `display/window/stretch/aspect="expand"`
- **`export_presets.cfg`:**
  - `permissions/vibrate=true` (Android manifest `android.permission.VIBRATE` izni).
  - Ekran yönü `screen/orientation=0` (Yatay / Landscape).

---

## 3. Kodlama ve Sınıf Sınırları (< 250 Satır Kuralı)
- `HapticManager.cs`: ~60-80 satır.
- `SafeAreaHandler.cs`: ~60-80 satır.
- `TapCombatArea.cs`: 76 satırdan ~110 satıra güncellenir.
- `SettingsModal.cs`: 201 satırdan ~225 satıra güncellenir (250 satır sınırının altında güvenle kalır).
- `SaveData.cs`: 1 yeni özellik eklenir.

---

## 4. Test ve Doğrulama Stratejisi
1. **Birim Testleri (`Tests/HapticAndMobileTests.cs`):**
   - `HapticManager` açık/kapalı durumu ve çağrı güvenliği test edilir.
   - `SaveSystem` üzerinden `IsHapticsEnabled` alanının serileşip geri yüklendiği doğrulanır.
   - Çoklu dokunuş hasar ve sınır mantığı doğrulanır.
2. **`dotnet build`:** 0 uyarı, 0 hata kapısı.
3. **Godot Headless Smoke Test:** Ekran ve sahne ağacının hatasız başlaması doğrulanır.
