# 🩸 BloodSeal: Uyanış (Awakening / Rebirth) Sistemi Tasarım Belgesi

**Belge Kimliği:** `2026-10-08-awakening-rebirth-system-design`  
**Tarih:** 08 Ekim 2026  
**Durum:** Onaylandı (Spec)  
**İlgili GDD Bölümü:** Bölüm 4.5 ("Uyanış (Awakening / Rebirth)") & Bölüm 6 ("Mimari Şema")  
**Bağlayıcı Kurallar:** [`AGENTS.md`](file:///c:/Users/Partridge/Desktop/blood-seal/AGENTS.md), [`GEMINI.md`](file:///c:/Users/Partridge/Desktop/blood-seal/GEMINI.md)

---

## 1. Genel Bakış ve Amaç

BloodSeal'da oyuncunun ilerlemesi belirli dalgalardan sonra doğal olarak yavaşlar veya boss enrage nedeniyle tıkanma noktasına gelir. GDD Bölüm 4.5 doğrultusunda **Uyanış (Awakening / Rebirth)** mekanizması:
1. Oyuncunun ulaştığı dalga derinliğine bağlı olarak **Uyanış Puanı (Awakening Points - AP)** kazanmasını,
2. Karakterin döngüsel istatistiklerini (dalga, altın, pentagram seviyeleri) sıfırlayarak (soft reset) yeni bir döngüye başlamasını,
3. Kazanılan Uyanış Puanları ile **Kadim Mühürler Ağacı (Primordial Seals Tree)** üzerinden kalıcı, çarpan bazlı güçlü pasif yetenekler açmasını sağlar.

Bu sistem sayesinde her prestij döngüsü oyuncuyu daha güçlü kılar, dalgaları daha hızlı temizletir ve oyunun uzun vadeli tutundurma (retention) döngüsünü tamamlar.

---

## 2. Açılma Eşiği ve Prestij Kuralları

### A. Açılma Eşiği
- **Minimum Dalga:** `CurrentWave >= 20` (veya `HighestWave >= 20`).
- Dalga 20'nin altındaki oyuncular için Uyanış butonu ve ritüeli kilitli bilgi modunda sunulur (`"Kızıl Ahit Mührü Dalga 20'de Uyanır"`).
- Dalga 20'ye ulaşıldığında sistem aktif hale gelir ve bekleyen puan arayüzde gösterilir.

### B. Uyanış Puanı (AP) Hesaplama Formülü
Kademeli-polinomik formül uygulanır:
$$BasePoints = \left\lfloor 1 + \frac{\max(0, Wave - 20)}{5} + \left(\frac{\max(0, Wave - 20)}{15}\right)^{1.3} \right\rfloor$$

Eğer Kadim Mühürler Ağacındaki `Heritage_PrimordialHarvest` pasifi açıksa, nihai kazanım şu şekilde hesaplanır:
$$FinalPoints = \lfloor BasePoints \times (1.0 + PrimordialHarvestBonus) \rfloor$$

**Örnek Kazanım Tablosu (Bonus Harici):**
| Ulaşılan Dalga | Temel AP | Not |
| :---: | :---: | :--- |
| **Dalga 20** | $1$ AP | İlk açılma eşiği |
| **Dalga 25** | $2$ AP | Minyon dalgası temizlendi |
| **Dalga 30** | $3$ AP | 3. Boss kesildi |
| **Dalga 40** | $6$ AP | Hızlanma aşaması |
| **Dalga 50** | $10$ AP | Orta aşama prestij |
| **Dalga 75** | $21$ AP | Derin ilerleme |
| **Dalga 100** | $36$ AP | Usta kademe prestij |

### C. Sıfırlama Kapsamı (Soft Reset vs Kalıcı İlerleme)

#### 1. Sıfırlananlar (Soft Reset)
- `CurrentWave = 1`
- `IsInSafeFarmMode = false`
- Pentagram Stat Seviyeleri (ATK, ATK Speed, Lifesteal, Max HP, Range) $\rightarrow 1$
- Mevcut Altın $\rightarrow$ `StartingGoldBonus` (Varsayılan 100 altın + Kan Hafızası bonusu)
- Mevcut Öfke Barı $\rightarrow 0$ (`RagePercentage = 0`, `IsRageActive = false`)

#### 2. Asla Sıfırlanmayanlar (Permanent Progress)
- `AwakeningPoints` (Harcanabilir ve Toplam) & Kadim Mühür Seviyeleri
- `TotalAwakenings` sayacı
- Malikane Kütüphanesi Araştırma Seviyeleri (`ResearchLevels`) & Envanterdeki `LoreScrolls`
- Malikane Kapısı Seçimi & Kuşanılmış Rün (`ActiveRune`, `SelectedGateApproach`)
- Karakter Profili (`PlayerName`, `Bloodline`, `Origin`)
- İstatistiksel `HighestWave` ve `DefeatedMilestoneBosses`

---

## 3. Kadim Mühürler Ağacı (Primordial Seals Tree)

Kadim Mühürler Ağacı 3 temel dala ayrılır ve toplam 7 odaklanmış pasif içerir:

### 🗡️ 1. Dal: Savaş Mührü (Seal of War)
1. **`War_PrimordialMight` (Kadim Kudret):**
   - **Etki:** Genel kahraman ve dokunma hasarına kalıcı $+15\%$ çarpan (Seviye başına $+15\%$).
   - **Maks Seviye:** $10$ (Maks $+150\%$)
   - **Maliyet (AP):** Hedef seviye kadar AP ($1, 2, 3, 4, 5, 6, 7, 8, 9, 10$).
2. **`War_BloodAegis` (Kan Zırhı):**
   - **Etki:** Maksimum Can değerine kalıcı $+15\%$ çarpan (Seviye başına $+15\%$).
   - **Maks Seviye:** $10$ (Maks $+150\%$)
   - **Maliyet (AP):** Hedef seviye kadar AP ($1, 2, 3, 4, 5, 6, 7, 8, 9, 10$).
3. **`War_VampiricThirst` (Vampirik Açlık):**
   - **Etki:** Can Çalma oranına doğrudan $+0.5\%$ taban değer ekler (Lv. 5'te $+2.5\%$ kalıcı Lifesteal).
   - **Maks Seviye:** $5$
   - **Maliyet (AP):** $2, 3, 4, 5, 6$

### ⚡ 2. Dal: Hız & Akış (Seal of Momentum)
4. **`Flow_WaveLeap` (Dalga Sıçraması / Mühür Kırıcı):**
   - **Etki:** Minyon dalgası temizlendiğinde $\%5$ şansla sonraki dalgayı da doğrudan atlar ve ödülünü verir (Seviye başına $+5\%$, maks $\%30$). *Boss dalgaları atlanamaz.*
   - **Maks Seviye:** $6$
   - **Maliyet (AP):** $2, 4, 6, 8, 10, 12$
5. **`Flow_CrimsonSurge` (Kızıl Hiddet):**
   - **Etki:** Vuruşlardan ve dokunmalardan kazanılan Öfke (Rage) miktarını $+20\%$ artırır (Lv. 5'te $2\times$ hızlı Berserk).
   - **Maks Seviye:** $5$
   - **Maliyet (AP):** $1, 2, 3, 4, 5$

### 🩸 3. Dal: Kan Mirası (Seal of Blood Heritage)
6. **`Heritage_BloodRecall` (Kan Hafızası):**
   - **Etki:** Her Uyanış sonrası Dalga 1'e ilave başlangıç altını ile başlanır:
     - Lv. 1: $+500$ Altın
     - Lv. 2: $+2,500$ Altın
     - Lv. 3: $+10,000$ Altın
     - Lv. 4: $+50,000$ Altın
     - Lv. 5: $+250,000$ Altın
   - **Maks Seviye:** $5$
   - **Maliyet (AP):** $1, 2, 3, 5, 8$
7. **`Heritage_PrimordialHarvest` (Kadim Hasat):**
   - **Etki:** Uyanış yapıldığında kazanılan Uyanış Puanına kalıcı $+10\%$ çarpan (Lv. 5'te $+50\%$ ekstra AP).
   - **Maks Seviye:** $5$
   - **Maliyet (AP):** $3, 5, 7, 10, 15$

---

## 4. Mimari Tasarım ve Kod Standartları

### A. Modüler Sınıf Dağılımı (`AGENTS.md` 250 Satır Kuralı)

1. **[`Scripts/Core/AwakeningModels.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/AwakeningModels.cs):**
   - `AwakeningBranch` enum
   - `AwakeningNodeDefinition` sınıfı
   - `AwakeningDatabase` statik sınıfı (tüm 7 mühür burada tanımlanır)

2. **[`Scripts/Core/AwakeningManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/AwakeningManager.cs):**
   - Singleton & Service yapısı (`ResearchManager` ile özdeş)
   - `AwakeningPoints`, `TotalAwakenings`, `Dictionary<string, int> _sealLevels`
   - Formül işletimi (`CalculatePendingPoints(int wave)`)
   - Prestij tetikleme (`ExecuteAwakening()`)
   - Düğüm satın alma (`TryUpgradeSeal(string id)`)
   - Çarpan metotları (`GetDamageMultiplier()`, `GetMaxHpMultiplier()`, `GetBonusLifesteal()`, `GetWaveLeapChance()`, `GetRageGainMultiplier()`, `GetStartingGold()`, `GetPointsMultiplier()`)

3. **[`Scripts/Combat/PentagramStats.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PentagramStats.cs):**
   - `Atk` ve `MaxHp` hesaplamalarında `AwakeningManager` çarpanları hesaba katılır.
   - `LifestealPercent` taban değerine uyanış bonusu eklenir.
   - `ResetToDefaults()` metodu eklenir.

4. **[`Scripts/Core/GameManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs):**
   - `ResetForAwakening()` delegasyon metodu:
     - `Stats.ResetToDefaults()`
     - `CurrentWave = 1`, `IsInSafeFarmMode = false`
     - `Gold = 100.0 + AwakeningManager.Instance.GetStartingGold()`
     - `RagePercentage = 0f`, `IsRageActive = false`
     - `SaveGame()` çağrısı ve UI event yayımı

5. **[`Scripts/Combat/WaveSpawner.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/WaveSpawner.cs):**
   - Minyon dalgası temizlendiğinde `AwakeningManager.Instance.GetWaveLeapChance()` kontrolü; tetiklenirse ekstra dalga atlama (`AdvanceWave()`).

6. **[`Scripts/Combat/TapCombatArea.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/TapCombatArea.cs) & [`Hero.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Hero.cs):**
   - Öfke eklenirken `AwakeningManager.Instance.GetRageGainMultiplier()` hesaba katılır.

7. **[`Scripts/Core/SaveData.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/SaveData.cs) & [`SaveSystem.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/SaveSystem.cs):**
   - `AwakeningPoints`, `TotalAwakenings`, `AwakeningLevels` alanları eklenir ve JSON serileştirmeye dahil edilir.

8. **[`Scripts/UI/AwakeningModal.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/AwakeningModal.cs):**
   - Gotik karanlık modal (`LibraryModal` standartlarında).
   - Sol panel: Uyanış Ritüeli (Mevcut Dalga, Bekleyen AP, Sıfırlanacak/Korunacak özeti, Onaylı "Kızıl Uyanışı Başlat" butonu).
   - Sağ panel: Kadim Mühürler Ağacı (3 dal sekmeleri, pasif düğüm kartları, seviye ve maliyet butonları).

9. **[`Scripts/UI/MainHUD.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/MainHUD.cs):**
   - `AwakeningBtn` ve `AwakeningModal` entegrasyonu.
   - Dalga 20+ olduğunda butonun görünürlüğü ve bildirim durumu.

---

## 5. Doğrulama Kapısı (Verification Gates)

Aşağıdaki adımlar eksiksiz tamamlanmadan geliştirme bitmiş sayılmaz:
1. **`Tests/AwakeningSystemTests.cs`:**
   - AP formülünün doğrulanması (Dalga 20, 25, 30, 40, 50, 100).
   - Düğüm satın alma, AP harcama ve maksimum seviye sınırları.
   - Prestij sıfırlama izolasyonu (araştırma, lore ve rünlerin korunduğu).
2. **`Tests/SaveSystemTests.cs`:** Uyanış verilerinin atomik kayıt ve yükleme testi.
3. **`dotnet build`:** 0 warning, 0 error.
4. **`dotnet test`:** Tüm testlerin başarıyla geçmesi.
5. **Godot Headless Smoke Test:** `--headless --quit-after 60` testi.
