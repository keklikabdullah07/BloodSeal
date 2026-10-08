# 🩸 BloodSeal: Lore Eserleri & Malikane Mahzeni Tasarım Belgesi

**Belge Kimliği:** `2026-10-08-lore-relics-vault-design`  
**Tarih:** 08 Ekim 2026  
**Durum:** Onaylandı (Spec)  
**İlgili GDD Bölümü:** Bölüm 2 ("Hikaye ve Atmosfer / Lore Drops") & Bölüm 6 ("Mimari Şema")  
**Bağlayıcı Kurallar:** [`AGENTS.md`](file:///c:/Users/Partridge/Desktop/blood-seal/AGENTS.md), [`GEMINI.md`](file:///c:/Users/Partridge/Desktop/blood-seal/GEMINI.md)

---

## 1. Genel Bakış ve Amaç

BloodSeal GDD Bölüm 2 gereğince:
> *"Oyun akışını kesen uzun diyaloglar yerine; Boss'lardan düşen kanlı yüzükler, yırtık aile portreleri ve mühürlü parşömenler envantere düşer. Bu parçalar, Malikane'nin kilitli odalarını ve araştırma kütüphanesini açan anahtarlar olarak kullanılır."*

Bu sistem:
1. Her 10 dalgada bir karşılaşılan Boss'lar ilk kez mağlup edildiğinde oyuncuya benzersiz bir **Lore Eseri (Relic)** kazandırır.
2. Her eser, Varnath evreninin karanlık geçmişine dair edebi bir hikaye kırıntısı (lore fragment) ve kalıcı bir pasif güçlendirme (hasar, can, altın, hız vb.) sunar.
3. Toplanan tüm eserler, Malikane arayüzündeki **Gotik Mahzen Modalı (RelicVaultModal)** içerisinde incelenebilir.
4. **Uyanış (Awakening / Rebirth)** yapıldığında toplanan eserler ve sağladıkları bonuslar asla **sıfırlanmaz** (kalıcı ilerleme bileşenidir).

---

## 2. 10 Gotik Lore Eseri ve Pasif Bonuslar

| Dalga | Eser Kimliği | Eser Adı | İkon | Kalıcı Pasif Güçlendirme | Gotik Hikaye Metni (Lore Fragment) |
| :---: | :--- | :--- | :---: | :--- | :--- |
| **D10** | `Relic_DariusRing` | **Darius'un Kanlı Mührü** | 💍 | **+%5 Kahraman Hasarı** | *"Darius son nefesinde mührü avucuma bastırdığında kanı henüz sıcaktı. 'Ahit'i durdur,' dedi, 'küllerimiz onların sunağı olmasın.'"* |
| **D20** | `Relic_TornPortrait` | **Yırtık Aile Portresi** | 🖼️ | **+%5 Maksimum Can** | *"Yüzleri jiletle kazınmış bir soylu ailesi. Altında soluk bir imza: 'Kan bağı asla çözülmez, sadece pıhtılaşır.'"* |
| **D30** | `Relic_CovenantMedallion` | **Kızıl Ahit Madalyonu** | 📿 | **+%5 Altın Kazanımı** | *"Tarikatın yüksek rahiplerinin taktığı ters pentagram madalyon. Dokunduğunda parmak uçlarında açgözlü bir sızı bırakıyor."* |
| **D40** | `Relic_BlackenedBell` | **Kararmış Zangoç Çanı** | 🔔 | **+%5 Öfke Dolum Hızı** | *"Varnath Katedrali'nin veba gecesinde çaldığı son çan. Sesi artık kulaklarda değil, doğrudan damarlarda yankılanıyor."* |
| **D50** | `Relic_InquisitorMask` | **Engizisyon Maskesi** | 🎭 | **+%10 Tıklama Hasarı** | *"Kuş gagası biçiminde dövülmüş demir maske. İç yüzeyinde kuruyan kan, takan kişinin kendi çığlıklarına ait."* |
| **D60** | `Relic_CryptKey` | **Kadim Kripta Anahtarı** | 🗝️ | **+%10 Çevrimdışı Gelir** | *"Malikane'nin unutulmuş alt mahzenlerini açan ağır pirinç anahtar. Zamanın bile unuttuğu hazinelerin bekçisi."* |
| **D70** | `Relic_BoneChalice` | **Kemik Kadeh** | 🍷 | **+%0.5 Taban Can Çalma** | *"İlk mühür taşıyıcısının kaval kemiğinden oyulmuş kadeh. İçine dökülen her damla kan, içenin susuzluğunu ebediyen dindiriyor."* |
| **D80** | `Relic_KnightSpur` | **Kan Şövalyesi Mahmuzu** | ⚔️ | **+%5 Saldırı Hızı** | *"Kızıl orduların öncülerine ait paslanmış mahmuz. Savaş alanında durmak bilmeyen bir vahşetin hatırası."* |
| **D90** | `Relic_ExtinguishedLantern` | **Sönmüş Ruh Feneri** | 🏮 | **+%10 Uyanış Puanı** | *"İçinde bir zamanlar hapsolmuş yüzlerce gölge ruhunun fısıltıları olan fener. Ölüm ve yeniden doğuş arasındaki köprü."* |
| **D100** | `Relic_FirstScroll` | **Kökenin İlk Parşömeni** | 📜 | **+%15 Tüm İstatistikler** | *"13 Mührün yaratıldığı gün yazılan ilk kutsal parşömen. Varnath'ın gerçek yaratılış sırrını fısıldıyor."* |

---

## 3. Mimari ve Kod Standartları

### A. Modüler Sınıf Dağılımı (`AGENTS.md` 250 Satır Kuralı)

1. **[`Scripts/Core/RelicModels.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/RelicModels.cs):**
   - `RelicStatType` enum (`Damage`, `MaxHp`, `Gold`, `RageGain`, `TapDamage`, `OfflineIncome`, `Lifesteal`, `AttackSpeed`, `AwakeningPoints`, `AllStats`).
   - `RelicDefinition` veri modeli.
   - `RelicDatabase` statik sınıfı (10 eserin tanımları, `AllRelics`, `GetRelic(string id)`, `GetRelicForWave(int wave)`).

2. **[`Scripts/Core/RelicManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/RelicManager.cs):**
   - Bağımsız Core servisi / Singleton (`ResearchManager` ve `AwakeningManager` deseni).
   - `HashSet<string> _collectedRelics` saklama alanı.
   - `UnlockRelic(string id)` ve `UnlockRelicForWave(int wave)`.
   - `HasRelic(string id)` ve `GetCollectedCount()`.
   - Pasif Bonus Hesaplayıcıları:
     - `GetDamageBonus()`: D10 (+%5) ve D100 (+%15) varsa toplam $+20\%$ hasar.
     - `GetHpBonus()`: D20 (+%5) ve D100 (+%15).
     - `GetGoldBonus()`: D30 (+%5) ve D100 (+%15).
     - `GetRageGainBonus()`: D40 (+%5).
     - `GetTapDamageBonus()`: D50 (+%10).
     - `GetOfflineIncomeBonus()`: D60 (+%10).
     - `GetLifestealBonus()`: D70 (+%0.5).
     - `GetAttackSpeedBonus()`: D80 (+%5) ve D100 (+%15).
     - `GetAwakeningBonus()`: D90 (+%10).
   - Event: `event Action<RelicDefinition>? OnRelicUnlocked`.

3. **[`Scripts/Combat/BossEnemy.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/BossEnemy.cs):**
   - Boss öldüğünde, eğer bu kilometre taşı ilk kez kesiliyorsa (`!ResearchManager.Instance.HasDefeatedMilestoneBoss(wave)`):
     - `RelicManager.Instance.UnlockRelicForWave(wave)` tetiklenir.
     - `FloatingTextManager.Instance?.SpawnFloatingText(GlobalPosition, "📜 Kadim Eser Açıldı!", new Color(1f, 0.85f, 0.3f))` ile geri bildirim verilir.

4. **[`Scripts/Combat/PentagramStats.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PentagramStats.cs):**
   - `Atk`: Araştırma ve Uyanış çarpanlarının yanına `*(1.0f + (RelicManager.Instance?.GetDamageBonus() ?? 0f))` eklenir.
   - `MaxHp`: `*(1.0f + (RelicManager.Instance?.GetHpBonus() ?? 0f))` eklenir.
   - `AtkSpeed`: `*(1.0f + (RelicManager.Instance?.GetAttackSpeedBonus() ?? 0f))` eklenir.
   - `LifestealPercent`: `+(RelicManager.Instance?.GetLifestealBonus() ?? 0f)` eklenir.

5. **[`Scripts/Core/GameManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs) & Çevrimdışı Gelir:**
   - `CalculateGoldReward`: `*(1.0f + (RelicManager.Instance?.GetGoldBonus() ?? 0f))` ile çarpılır.
   - `OfflineProgressCalculator`: Çevrimdışı altın hesabına eser bonusu eklenir.
   - `TapCombatArea` & `Hero`: Eser öfke ve dokunma bonusları hesaba katılır.

6. **[`Scripts/Core/SaveData.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/SaveData.cs) & [`SaveSystem.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/SaveSystem.cs):**
   - `public List<string> CollectedRelics { get; set; } = new();` alanı eklenir.
   - `CaptureSaveData` ve `ApplySaveData` içine entegre edilir.

7. **[`Scripts/UI/RelicVaultModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/RelicVaultModal.cs) & [`RelicVaultModal.tscn`](file:///c:/Users/Partridge/Desktop/blood-seal/Scenes/UI/RelicVaultModal.tscn):**
   - Gotik karanlık tasarım standardı (`LibraryModal` ve `AwakeningModal` uyumlu).
   - Sol Sütun: 10 adet eserin vitrin listesi (açılanlar altın çerçeveli, kilitliler gölgeli kilit simgeli).
   - Sağ Sütun: Seçilen eserin büyük ikonu, adı, ait olduğu dalga, gotik hikaye metni ve kalıcı aktif bonus kartı.
   - Üst Bar: Toplanan Eser Sayacı (örn: *"🏛️ Mahzen Koleksiyonu: 3 / 10 Eser"*).
   - Kapatma Butonu ("✕").

8. **[`Scripts/UI/MainHUD.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/MainHUD.cs):**
   - `RelicVaultBtn` eklenir. En az 1 eser açıldığında veya Dalga 10 temizlendiğinde üst çubukta görünür olur.

---

## 4. Doğrulama ve Test Kapısı (Verification Gates)

1. **`Tests/RelicSystemTests.cs`:**
   - 10 eserin eksiksiz tanımlandığının ve veritabanı bütünlüğünün testi.
   - Dalga bazlı eser açma mekanizmasının testi.
   - Hasar, can, hız, altın ve can çalma bonuslarının doğru hesaplandığının testi.
   - `SaveSystem` serileştirme ve geri yükleme testi.
2. **`dotnet build`:** 0 warning, 0 error.
3. **`dotnet test`:** Tüm testlerin başarıyla geçmesi.
4. **Godot Headless Smoke Test:** `--headless --quit-after 60` testi.
5. **250 Satır Sınırı:** Tüm C# sınıflarının 250 satır sınırının altında kalması.
