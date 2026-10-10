# 🩸 BloodSeal: Kadim Kan Denemeleri (Trials of Blood - Boss Rush) Tasarım Dokümanı

## 1. Genel Bakış
Bu doküman, **BloodSeal**'ın oyunculara yüksek risk ve yüksek ganimet sunan zaman karşı Boss Rush modu olan **Kadim Kan Denemeleri (Trials of Blood)** sisteminin mimari, oynanış ve arayüz standartlarını tanımlar.
Oyuncular günlük anahtarlarını harcayarak 90 saniyelik süre sınırında ardı ardına 3 kudretli Gotik Boss ile savaşır; kalan süreye göre yıldız derecesi ve **Kadim Kan Sandığı (Blood Chest)** ödülü kazanır.

---

## 2. Mimari Bileşenler & Sorumluluklar

### A. Veri Modelleri & Konfigürasyon (`DungeonModels.cs`)
- **Konum:** `Scripts/Core/DungeonModels.cs`
- **Tür:** Saf C# Modelleri (Unit test edilebilir, bağımsız).
- **Modeller:**
  - `DungeonBossDefinition`: Boss adı, can çarpanı, hasar çarpanı, görsel tipi, lore açıklaması.
  - `DungeonRewardResult`: Kazanılan altın, parşömen miktarı, düşen ekipman (`EquipmentItem`), yıldız derecesi (1, 2, 3), geçen süre.
  - `DungeonTierConfig`: Zorluk kademesi (varsayılan: Kademe I, II, III).
- **Matematik & Formüller:**
  - **Süre Sınırı:** 90.0 saniye.
  - **3 Yıldız:** Kalan süre > 30 sn (Ödül Çarpanı: $1.5\times$).
  - **2 Yıldız:** Kalan süre > 10 sn (Ödül Çarpanı: $1.25\times$).
  - **1 Yıldız:** Kalan süre $\le$ 10 sn (Ödül Çarpanı: $1.0\times$).
  - **Altın Ödülü:** $\operatorname{round}((Wave \times 400 + 1000) \times StarMultiplier)$.
  - **Parşömen Ödülü:** 1-3 adet (Yıldıza bağlı: 1★=1, 2★=2, 3★=3).

### B. Zindan Yöneticisi (`DungeonManager.cs`)
- **Konum:** `Scripts/Core/DungeonManager.cs`
- **Tür:** Saf C# Singleton Servisi (`DungeonManager.Instance`).
- **Sorumluluk:**
  - Giriş anahtarlarını (`DungeonKeys`) yönetmek (maks 5, varsayılan günlük 3).
  - Günlük hak yenilenmesini (`CheckDailyReset()`) UTC zaman damgasıyla senkronize etmek.
  - Normal dalga boss ölümlerinde nadir (%5) anahtar düşüşünü işlemek.
  - Zindan durumu: `IsTrialActive`, `CurrentBossIndex`, `RemainingTime`.
  - Olaylar (Events): `OnKeysChanged`, `OnTrialStarted`, `OnTrialBossDefeated`, `OnTrialEnded`.

### C. Zindan Savaş Döngüsü (`DungeonController.cs`)
- **Konum:** `Scripts/UI/DungeonController.cs` (CanvasLayer 104)
- **Sorumluluk:**
  - Zindan başladığında normal dalga spawn'ını (`WaveSpawner`) duraklatır/gizler.
  - 90 saniyelik geri sayımı gerçek delta (`_dungeonTimer -= delta`) ile yürütür (Hit-freeze veya TimeScale sayacı etkileyemez).
  - 3 Boss'u sırayla doğurur:
    1. *Kripta Muhafızı (Crypt Guardian)* - Dalga $\times 1.2$ HP.
    2. *Kan Başpiskoposu (Blood Archbishop)* - Dalga $\times 1.5$ HP.
    3. *Kadim Abyssal Lord (Abyssal Fiend)* - Dalga $\times 2.0$ HP.
  - Üstte dinamik HUD: Kırmızı geri sayım barı, `💀 Boss: 1/3` göstergesi.
  - Süre biterse veya kahraman ölürse: Yenilgi ekranı; ana dalga akışına geri dönüş.
  - 3. Boss ölürse: Zafer ekranını (`DungeonVictoryModal`) açar, ödülleri dağıtır.

### D. Arayüz ve Sunum (`DungeonModal.cs` & `DungeonVictoryModal.cs`)
- **Giriş Modalı (`DungeonModal.cs`):**
  - Üst bar / menüden `⚔️ KAN DENEMELERİ [3/3]` butonu ile açılır.
  - Zindan tier'ı, boss önizlemesi, mevcut anahtar sayısı ve `⚔️ DENEMEYİ BAŞLAT (1 ANAHTAR)` butonu.
- **Zafer Modalı (`DungeonVictoryModal.cs`):**
  - Yıldız derecesi (★★★), kalan süre, Kadim Kan Sandığı açılma animasyonu.
  - Düşen altın, kadim parşömen ve rastgele Gotik eşya kartı.
  - `Ganimeti Kuşan / Al` butonu.

### E. Kalıcılık Entegrasyonu (`SaveData.cs` & `SaveSystem.cs`)
- `SaveData.cs`:
  - `public int DungeonKeys { get; set; } = 3;`
  - `public long LastDungeonResetTimestamp { get; set; } = 0;`
  - `public int HighestDungeonTierCleared { get; set; } = 0;`
- `SaveSystem.cs`: Kayıt alma ve geri yükleme işlemlerinde bu alanlar serileştirilir.

---

## 3. Kodlama ve Sınıf Sınırları (< 250 Satır Kuralı)
- `DungeonModels.cs`: ~65 satır.
- `DungeonManager.cs`: ~120 satır.
- `DungeonController.cs`: ~150 satır.
- `DungeonModal.cs`: ~160 satır.
- `DungeonVictoryModal.cs`: ~130 satır.
- Tüm sınıflar bağlayıcı mimari el kitabına (`AGENTS.md`) uygun olarak kesinlikle 250 satır sınırının altında tutulacaktır.

---

## 4. Test ve Doğrulama Stratejisi
1. **Birim Testleri (`Tests/DungeonTests.cs`):**
   - Anahtar harcama, günlük yenilenme ve sınır mantığı testi.
   - Ödül formülü ve kalan süreye göre yıldız hesaplama doğrulaması.
   - Serileştirme (`SaveData` & `SaveSystem`) testi.
2. **`dotnet build`:** 0 uyarı, 0 hata kapısı.
3. **Headless Smoke Test:** Godot motorunda sahne ağacı ve modal başlatma testi (`Godot_console.exe --headless --quit-after 60`).
