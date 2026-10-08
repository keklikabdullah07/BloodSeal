# 🩸 BloodSeal: Ses Mimarisi ve Gotik Ses Deneyimi Tasarım Belgesi

**Belge Kimliği:** `2026-10-08-audio-system-design`  
**Tarih:** 08 Ekim 2026  
**Durum:** Onaylandı (Spec)  
**İlgili Beceriler & Kurallar:** [`godot-audio`](file:///c:/Users/Partridge/Desktop/blood-seal/.agents/skills/godot-audio/SKILL.md), [`AGENTS.md`](file:///c:/Users/Partridge/Desktop/blood-seal/AGENTS.md), [`GEMINI.md`](file:///c:/Users/Partridge/Desktop/blood-seal/GEMINI.md)

---

## 1. Genel Bakış ve Amaç

BloodSeal'ın karanlık gotik fantezi atmosferini güçlendirmek, savaş vuruş hissini (game feel) zirveye taşımak ve oyuncuya ses düzeylerini tam kontrol etme imkanı sunmak amacıyla kapsamlı bir ses mimarisi inşa edilmektedir.

Bu sistem:
1. **Dinamik Gotik Müzik (BGM):** Normal minyon dalgaları ile her 10. dalgadaki Boss savaşları arasında pürüzsüz çift kanallı (crossfade) müzik geçişi sağlar.
2. **Çok Kanallı SFX Havuzu (Multi-Channel):** Hızlı vuruşlarda seslerin birbirini kesmesini önleyen 6 kanallı döngüsel ses oynatıcı havuzu sunar.
3. **Kapsamlı Ses Efekti Envanteri:** Berserk patlaması, Uyanış ritüeli, Antik Eser kazanımı, Boss zaferi, Kahraman yenilgisi ve tüm UI tıklamalarını seslendirir.
4. **Gotik Ayarlar Paneli (`SettingsModal`):** Master, BGM ve SFX ses seviyelerini logaritmik (dB) kontrol eden ve diske kaydeden arayüz sunar.

---

## 2. Mimari ve Bileşen Tasarımı

### A. Ses Yolu Hiyerarşisi (`default_bus_layout.tres`)
```text
Master Bus (0 dB)
├── BGM Bus (-4 dB, Send: Master)
├── SFX Bus (0 dB, Send: Master)
└── UI Bus (0 dB, Send: Master)
```

### B. Modüler Sınıf Dağılımı (`AGENTS.md` 250 Satır Kuralı)

1. **[`Scripts/Core/AudioManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/AudioManager.cs) (Autoload Singleton, ~150-180 satır):**
   - **BGM Kanalları:** İki adet `AudioStreamPlayer` (`_bgmTrackA`, `_bgmTrackB`). `PlayBGM(BgmTrackType track, float duration = 1.2f)` metodu ile çift yönlü tween crossfade.
   - **SFX Havuzu:** 6 adet döngüsel `AudioStreamPlayer` kanalı (`_sfxPool`). Ses tetiklendiğinde sıradaki müsait veya en eski kanalı seçerek ses yutulmasını önler.
   - **Öncelikli Kanallar:** Kritik vuruş ve Boss kükremesi için özel oynatıcılar.
   - **Desibel Yönetimi:** Logaritmik desibel dönüşümü (`Mathf.LinearToDb`, 0.0 değeri güvenli -80 dB'e kenetlenir).
   - **Ayarların Uygulanması:** `ApplySettings(float master, float bgm, float sfx, bool muted)`.

2. **[`Scripts/Core/AudioData.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/AudioData.cs) (~40-60 satır):**
   - `BgmTrackType` enum (`GothicAmbient`, `BossCombat`).
   - `AudioCueType` enum (`Slash`, `Hit`, `CritHit`, `Tap`, `RageBurst`, `BossEnrage`, `Coin`, `BossVictory`, `HeroDeath`, `RelicUnlock`, `AwakeningRitual`, `ButtonClick`, `ModalOpen`, `ModalClose`).
   - Ses dosya yolları ve taban ses desibel/pitch yapılandırmaları.

3. **[`Scripts/UI/SettingsModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/SettingsModal.cs) (~120-150 satır):**
   - Gotik karanlık modal tasarımı (`LibraryModal` standartlarında).
   - Master, BGM ve SFX `HSlider` bileşenleri (%0 - %100).
   - Mute kutucukları.
   - Sürgü hareketi bittiğinde anlık test sesi (`ButtonClick`) çalma.
   - `SaveSystem.SaveGame()` entegrasyonu.

4. **[`Scripts/Core/SaveData.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/SaveData.cs) & [`SaveSystem.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/SaveSystem.cs):**
   - `MasterVolume` (float, varsayılan 1.0f)
   - `BgmVolume` (float, varsayılan 0.8f)
   - `SfxVolume` (float, varsayılan 1.0f)
   - `IsMuted` (bool, varsayılan false)
   - JSON serileştirmeye dahil edilmesi ve açılışta `AudioManager`'a otomatik uygulanması.

5. **[`Scripts/UI/MainHUD.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/MainHUD.cs):**
   - `SettingsBtn` (⚙️) butonu ve `SettingsModal` açılış bağlantısı.

---

## 3. Dinamik Müzik ve Ses Envanteri

### A. BGM Parçaları
* **`GothicAmbient` (`res://Audio/BGM/gothic_ambient.wav`):** Minyon dalgalarında (Dalga 1-9, 11-19...) kesintisiz döngüde çalan derin Gotik ambiyans.
* **`BossCombat` (`res://Audio/BGM/boss_combat.wav`):** Her 10. dalga Boss savaşında çalan agresif, tempolu savaş teması.

### B. SFX Envanteri
* **Savaş:**
  - `Slash`: Kılıç savurması (pitch: 0.92 - 1.08).
  - `Hit`: Standart darbe sesi.
  - `CritHit`: Derin baslı yankılı kritik darbe.
  - `Tap`: Ekrana dokunma hasarı çıtırtısı.
  - `RageBurst`: Öfke patlaması tetiklendiğinde derin çığlık ve kan alevi.
  - `BossEnrage`: Boss her 5 saniyede güçlendiğinde çalan tehditkar kükreme.
* **İlerleme & Ödül:**
  - `Coin`: Altın kazanımı.
  - `BossVictory`: Boss kesildiğinde çalan zafer gongu.
  - `HeroDeath`: Kahraman yenildiğinde çalan çöküş darbesi.
  - `RelicUnlock`: Milestone Boss'tan antik eser düştüğünde mistik yankılı çan.
  - `AwakeningRitual`: Kızıl Uyanış tetiklendiğinde derin kan ahdi gongu.
* **Arayüz (UI):**
  - `ButtonClick`: Menü butonları ve yükseltme tıklamaları.
  - `ModalOpen` / `ModalClose`: Gotik pencerelerin açılıp kapanması.

---

## 4. Oyun İçi Entegrasyon Noktaları

| Tetikleyen Sınıf | Tetiklenen Ses | Koşul / Olay |
| :--- | :--- | :--- |
| [`Hero.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Hero.cs) | `Slash` | Kahraman kılıç savurduğunda |
| [`Hero.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Hero.cs) | `HeroDeath` | Kahramanın canı 0'a ulaştığında |
| [`Enemy.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Enemy.cs) | `Hit` / `CritHit` | Düşman hasar aldığında |
| [`BossEnemy.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/BossEnemy.cs) | `BossEnrage` | Her 5 saniyede bir enrage adımında |
| [`BossEnemy.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/BossEnemy.cs) | `BossVictory` | Boss öldüğünde |
| [`TapCombatArea.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/TapCombatArea.cs) | `Tap` | Ekrana tıklandığında |
| [`GameManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs) | `RageBurst` | `TriggerRage()` tetiklendiğinde |
| [`GameManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs) | `BGM Crossfade` | `SetWave()` çağrısında (Boss vs Minyon) |
| [`AwakeningManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/AwakeningManager.cs) | `AwakeningRitual` | `ExecuteAwakening()` onaylandığında |
| [`RelicManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/RelicManager.cs) | `RelicUnlock` | Yeni bir eser düştüğünde |
| [`MainHUD.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/MainHUD.cs) | `ButtonClick` / `ModalOpen` | Buton ve modal etkileşimlerinde |

---

## 5. Doğrulama Kapısı ve Testler

1. **[`Tests/AudioSystemTests.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Tests/AudioSystemTests.cs):**
   - Lineer ses seviyesinden desibele dönüşümün matematiksel doğruluğu ($LinearToDb(1.0) = 0$, $LinearToDb(0.0) = -80$ dB clamp).
   - `SaveData` üzerinde `MasterVolume`, `BgmVolume`, `SfxVolume` ve `IsMuted` alanlarının atomik serileştirme ve geri yükleme testi.
   - `AudioManager` ayar uygulama testi.
2. **Derleme & Statik Kontrol:** `dotnet build` 0 uyarı ve 0 hata ile tamamlanmalıdır.
3. **Birim Testleri:** `dotnet test` tüm testleri başarıyla geçmelidir.
4. **Godot Headless Smoke Test:** Motor 60 saniyelik headless testte çökme veya eksik kaynak hatası vermemelidir.
