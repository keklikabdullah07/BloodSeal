# 🩸 Malikane Kütüphanesi & Araştırma Sistemi (Research Tree) Tasarım Şartnamesi

**Tarih:** 08 Ekim 2026  
**Durum:** Tasarım Onaylandı (Ready for Planning)  
**Hedef Sürüm:** Godot 4.7.x Mono (C#) / .NET 10.0 (`net10.0`)  
**İlgili Belgeler:** [`BLOODSEAL_GDD.md`](file:///c:/Users/Partridge/Desktop/blood-seal/BLOODSEAL_GDD.md), [`AGENTS.md`](file:///c:/Users/Partridge/Desktop/blood-seal/AGENTS.md), [`GEMINI.md`](file:///c:/Users/Partridge/Desktop/blood-seal/GEMINI.md)

---

## 1. Genel Bakış ve Amaç

BloodSeal GDD Bölüm 2, 4.4 ve 6'da tanımlandığı üzere; oyuncunun Malikane Kapısı'nı aşması (Dalga 5) ve Boss savaşlarından topladığı **Kadim Parşömenler (Lore Scrolls)** ile Malikane Kütüphanesi'nin kilidi açılır. Kütüphane Araştırma Sistemi, oyuncuya temel Pentagram istatistiklerinin ötesinde kalıcı ve stratejik meta-ilerleme sağlar.

Bu sistemin üç temel sütunu vardır:
1. **Kadim Ekonomi:** Altın kazanımını artırma, Pentagram yükseltme maliyetlerini ucuzlatma ve Boss ganimetlerini katlama.
2. **Kan Hafızası:** 6 saatlik çevrimdışı ilerleme sınırını 12 saate kadar genişletme ve çevrimdışı altın üretim verimini artırma.
3. **Savaş Ezoterizmi:** Tıklama (Tap) hasarını güçlendirme, süzülen 2 yardımcı pet'in saldırı verimini artırma ve Berserk Öfke modunun süresini uzatma.

---

## 2. Mimari Prensipler ve Kısıtlar

* **250 Satır Kuralı:** [`GameManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/GameManager.cs) 248 satır sınırında olduğundan, araştırma mantığı tamamen bağımsız [`Scripts/Core/ResearchManager.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/ResearchManager.cs) ve [`Scripts/Core/ResearchModels.cs`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Core/ResearchModels.cs) sınıflarında toplanacaktır.
* **Call Down, Signal Up:** Araştırma yükseltildiğinde UI veya diğer sistemler C# event'leri (`event Action<string, int> OnResearchUpgraded`, `event Action<int> OnLoreScrollsChanged`) üzerinden bilgilendirilecektir.
* **Engine.TimeScale Dokunulmazlığı:** Hiçbir araştırma veya arayüz efekti `Engine.TimeScale`'e müdahale etmeyecektir.
* **Gerçek Delta Hesaplama:** Çevrimdışı ve bekleme hesapları `DateTimeOffset` ve `delta` ile ilerlemeye devam edecektir.

---

## 3. Veri Modeli ve Araştırma Düğümleri (Research Nodes)

### A. Para Birimleri ve Kaynaklar
1. **Altın (`double`):** Standart dalga ve farm geliri.
2. **Kadim Parşömen (`int LoreScrolls`):**
   * Dalga 5 Malikane Kapısı (`BloodSeal` yaklaşımıyla veya kapı tamamlanmasıyla) başlangıç parşömeni (1 adet).
   * Her 10. dalga Boss'u **ilk kez** yenildiğinde (Milestone Boss Kill: Dalga 10, 20, 30...) **garanti 1 adet**.
   * Tekrarlanan farm Boss kesimlerinde **%10 şansla 1 adet**.

### B. 3 Temel Disiplin ve Araştırma Ağacı Tanımları

```text
[Malikane Kütüphanesi]
  ├── 1. Kadim Ekonomi (Ancient Economy)
  │     ├── Econ_GoldBounty (Lv 1-10): Altın Kazanımı +%5/lv (Maliyet: Altın)
  │     ├── Econ_SealEfficiency (Lv 1-5): Pentagram Maliyet İndirimi -%2/lv (Maliyet: Altın + Parşömen)
  │     └── Econ_BossTribute (Lv 1-5): Boss Zafer Altını +%20/lv (Maliyet: Altın + Parşömen)
  ├── 2. Kan Hafızası (Blood Memory)
  │     ├── Mem_OfflineCap (Lv 1-6): Çevrimdışı Zaman Sınırı +1 Saat/lv [6s -> 12s] (Maliyet: Altın + Parşömen)
  │     ├── Mem_OfflineYield (Lv 1-10): Çevrimdışı Altın Üretim Çarpanı +%10/lv (Maliyet: Altın)
  │     └── Mem_DeepSlumber (Lv 1-5): Çevrimdışı Simülasyon Hızı/Verimi +%5/lv (Maliyet: Altın + Parşömen)
  └── 3. Savaş Ezoterizmi (Combat Esotericism)
        ├── War_TapMastery (Lv 1-10): Tıklama Hasarı (Tap DMG) +%15/lv (Maliyet: Altın)
        ├── War_PetFrequency (Lv 1-5): Ruh Pet Saldırı Hızı & Hasarı +%8/lv (Maliyet: Altın + Parşömen)
        └── War_BerserkProlong (Lv 1-5): Berserk Öfke Süresi +1 sn/lv [10s -> 15s] (Maliyet: Altın + Parşömen)
```

### C. Maliyet Formülleri
* Altın Maliyeti:
  $$\text{GoldCost} = \text{BaseGold} \times 1.4^{(\text{Level})}$$
  *(Temel değerler düğümün gücüne göre 200 ila 1000 altın arasında başlar).*
* Parşömen Maliyeti:
  * Saf altın düğümleri: 0 Parşömen.
  * İleri düzey düğümler: Kademe 1-2 için 1 Parşömen, Kademe 3+ için 2 Parşömen.

---

## 4. Sistem Entegrasyonları (Gameplay & Core)

1. **`PentagramStats.cs`:**
   * `GetAtkCost()`, `GetAtkSpeedCost()` vb. maliyet hesaplamalarında `ResearchManager.Instance.GetSealDiscountMultiplier()` çarpanı uygulanır (örn. %10 indirim = 0.90x).
2. **`GameManager.cs`:**
   * `CalculateGoldReward(baseGold)`: `ResearchManager.Instance.GetGoldBountyMultiplier()` ve boss için `GetBossTributeMultiplier()` eklenir.
   * `TriggerRage()`: `_rageActiveTimer = 10.0 + ResearchManager.Instance.GetBerserkBonusDuration()`.
3. **`OfflineProgressCalculator.cs`:**
   * Çevrimdışı sınır: `BaseMaxSeconds (21600) + ResearchManager.Instance.GetOfflineCapBonusSeconds()`.
   * Çevrimdışı kazanç çarpanı: `BaseYield * ResearchManager.Instance.GetOfflineYieldMultiplier()`.
4. **`TapCombatArea.cs`:**
   * Tıklama vuruş hasarı: `Hero.PentagramStats.Atk * BaseRatio * ResearchManager.Instance.GetTapDamageMultiplier()`.
5. **`PetCompanion.cs`:**
   * Pet mermi hasarı ve atış frekansı: `ResearchManager.Instance.GetPetBonusMultiplier()`.
6. **`BossEnemy.cs`:**
   * Boss öldüğünde `ResearchManager.Instance.HandleBossDefeated(int wave)` çağrılır; gerekiyorsa parşömen düşer ve ekranda parşömen VFX/FloatingText belirir.

---

## 5. Kullanıcı Arayüzü (UI / UX)

### A. HUD Entegrasyonu (`MainHUD.cs` & `MainHUD.tscn`)
* Üst sağ kısımda (Wave ve Gate butonunun yanında) `🏛️ Kütüphane` butonu (`LibraryButton`).
* Dalga 5'e gelindiğinde veya ilk parşömen kazanıldığında buton görünür hale gelir.
* Yükseltme yapılabilecek kaynak varsa butonda görsel bildirim parıltısı veya rozet gösterilir.

### B. Kütüphane Modalı (`Scenes/UI/LibraryModal.tscn` & `LibraryModal.cs`)
* Gotik çerçeveli tam modal pencere (`Control`).
* Üst Çubuk:
  * Başlık: `🏛️ MALİKANE KÜTÜPHANESİ & KADİM ARAŞTIRMALAR`
  * Kaynak Paneli: `🪙 [Altın]` ve `📜 [Parşömen Sayısı]`
  * Kapatma Butonu (`✕`)
* Sekme Çubuğu:
  * `[🪙 Kadim Ekonomi]` | `[⏳ Kan Hafızası]` | `[⚔️ Savaş Ezoterizmi]`
* Liste Paneli (`ScrollContainer` + `VBoxContainer`):
  * Seçili sekmeye ait araştırma kartları listelenir.
  * Kart içeriği:
    * Araştırma Adı ve Seviye (`Lv. X / Max`)
    * Açıklama ve Anlık Değer (`Mevcut: +%10 -> Sonraki: +%15`)
    * Maliyet Rozetleri (`🪙 1.2K Altın`, `📜 1 Parşömen`)
    * `[Araştır]` butonu (yetersiz kaynakta devre dışı, son seviyede `[Maksimum]` etiketi).

---

## 6. Kayıt Sistemi & Kalıcılık (`SaveData.cs` & `SaveSystem.cs`)

`SaveData` sınıfına eklenecek alanlar:
```csharp
public int LoreScrolls { get; set; } = 0;
public Dictionary<string, int> ResearchLevels { get; set; } = new();
public List<int> DefeatedMilestoneBosses { get; set; } = new();
```
* `SaveSystem.CaptureSaveData` ve `ApplySaveData` bu alanları hatasız okuyup yazar.
* Eski kayıt dosyaları açıldığında boş dictionary ve sıfır parşömen ile geriye dönük uyumlu yüklenir.

---

## 7. Doğrulama Kapısı ve Test Stratejisi

1. **Birim Testleri (`Tests/ResearchSystemTests.cs`):**
   * Araştırma maliyeti ve seviye sınırları testleri.
   * Yetersiz altın veya yetersiz parşömen durumunda harcama engelleme testleri.
   * Formül çarpanlarının (Pentagram indirimi, çevrimdışı süre uzatımı, tap hasarı vb.) doğruluğu.
   * Boss kesiminde ilk zafer garantisi (Milestone) vs %10 farm düşüş mantığı testleri.
   * `SaveData` JSON serileştirme ve geriye dönük uyumluluk testi.
2. **Derleme:**
   * `dotnet build` komutu 0 uyarı ve 0 hata ile tamamlanmalıdır.
3. **Headless Test:**
   * `Godot_v4.7.2-stable_mono_win64_console.exe --headless --quit-after 60` sahne ağacı kontrollerinden sıfır hata ile geçmelidir.
