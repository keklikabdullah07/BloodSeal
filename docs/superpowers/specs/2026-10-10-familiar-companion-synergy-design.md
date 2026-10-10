# 🩸 BloodSeal: Yoldaş (Familiar) Sinerji, Seviye & Yetenek Sistemi Tasarımı

## 1. Genel Bakış ve Amaç
BloodSeal, karanlık Gotik temalı bir idle RPG'dir. Bu sistem ile kahramanın yanında süzülen yardımcı pet (`PetCompanion`), tekil ve statik bir yardımcıdan zengin bir **Kadim Kan Bağı Yoldaşları (Familiars)** koleksiyonuna dönüştürülmektedir.

Oyuncu, her birinin kendine has saldırı tarzı, mermi dinamiği, görsel aurası ve taktiksel aurası olan 4 farklı Gotik yoldaş edinebilir. Yoldaşlar altın ve kan parşömenleri ile güçlendirilir, dalga ilerlemesine göre kilitleri açılır ve savaş alanında anlık olarak kuşanılabilir.

---

## 2. Gotik Yoldaş Kadrosu & Nitelikler

### 2.1 Yoldaş Tanımları
Sistemde 4 temel Gotik yoldaş yer alır:

1. **Kan Kargası (Blood Raven - Başlangıç Yoldaşı):**
   - **ID:** `blood_raven`
   - **Açılış Şartı:** Dalga 1 (Başlangıçtan açık).
   - **Görsel & Parçacık:** `Assets/Sprites/Pet/pet_blood_raven.png`, Kızıl Kırmızı Parçacık Aurası (`#E51940`).
   - **Saldırı Stili:** Hızlı kan küresi atışları (0.40x Hero ATK, 1.20s aralık).
   - **Pasif Aura (Kan Sürgünü):** Her seviye için +%10 yoldaş hasarı; her 10. seviyede (Tier) Kahraman saldırı hızı kalıcı +%3 artar.

2. **Gölge Yarasası (Shadow Bat):**
   - **ID:** `shadow_bat`
   - **Açılış Şartı:** Dalga 15.
   - **Görsel & Parçacık:** Gece Mavisi / Mor Alev Aurası (`#7A25C7`).
   - **Saldırı Stili:** İkili gölge küresi (0.35x Hero ATK, 1.40s aralık).
   - **Pasif Aura (Karanlık Bakış):** Kahramana $+(\%5 + Level \times \%0.5)$ Kritik Şans ve $+(\%15 + Level \times \%1.0)$ Kritik Hasar sağlar.

3. **Kan Tazısı (Crimson Hound):**
   - **ID:** `crimson_hound`
   - **Açılış Şartı:** Dalga 25 veya 1. Uyanış (Rebirth).
   - **Görsel & Parçacık:** Yakut / Derin Kan Kırmızısı Aurası (`#990F02`).
   - **Saldırı Stili:** Yırtıcı kan dalgası (0.55x Hero ATK, 1.60s aralık).
   - **Pasif Aura (Açgözlü Av):** Düşmanlardan ve bosslardan düşen Altın miktarını $+(\%15 + Level \times \%1.0)$ artırır; boss'lara karşı zırh delme sağlar.

4. **Gece Heykeli (Stone Gargoyle):**
   - **ID:** `stone_gargoyle`
   - **Açılış Şartı:** Dalga 40.
   - **Görsel & Parçacık:** Taş Grisi ve Soluk Kehribar Aurası (`#A89F91` / `#D4AF37`).
   - **Saldırı Stili:** Ağır taş şoku / ezici yer sarsıntısı (0.80x Hero ATK, 2.20s aralık).
   - **Pasif Aura (Taş Siper):** Kahramanın aldığı hasarı $\%10 + Level \times \%0.5$ (maksimum $\%35$) oranında sönümler; Berserk dolumunu hızlandırır.

---

## 3. Matematiksel Model ve Ekonomi Standartları

### 3.1 Seviye Yükseltme Maliyeti (Altın)
Idle ekonomi standartlarına uygun olarak üstel artış formülü:
$$Cost_{Gold} = BaseCost \times 1.15^{(Level - 1)}$$
- Taban Maliyetler:
  - `blood_raven`: 100 Altın
  - `shadow_bat`: 500 Altın
  - `crimson_hound`: 2,500 Altın
  - `stone_gargoyle`: 10,000 Altın

### 3.2 Tier Kilitleri ve Kan Parşömeni Maliyeti
Her 10. seviyede (10, 20, 30...) seviye atlamak için Altın'a ek olarak **Kan Parşömeni (Blood Parchment)** gereklidir:
$$Cost_{Parchment} = Tier \times 5$$
*(Örn: Seviye 10 için 5 parşömen, Seviye 20 için 10 parşömen, Seviye 30 için 15 parşömen).*

### 3.3 Hasar ve Çarpan Formülü
$$PetDamage = HeroAtk \times BaseMult \times (1.0 + 0.08 \times (Level - 1)) \times ResearchPetMult$$
- `HeroAtk`: Kahramanın güncel temel saldırı gücü.
- `BaseMult`: Yoldaşın taban hasar katsayısı (`0.35f` - `0.80f`).
- `ResearchPetMult`: Kütüphane araştırmasından (`War_PetFrequency`) gelen çarpan.

---

## 4. Mimari ve Bileşen Yapısı

### 4.1 Saf C# Modelleri ve Veritabanı
- **`Scripts/Core/FamiliarModels.cs`:**
  - `enum FamiliarType`
  - `class FamiliarDefinition` (ID, İsim, Açıklama, Pasif Açıklaması, İkon, Aura Rengi, Taban Çarpanlar, Dalga Kilidi).
  - `class FamiliarProgress` (ID, Seviye, Kilit Durumu).
- **`Scripts/Core/FamiliarDatabase.cs`:**
  - 4 Gotik yoldaş tanımının statik sözlüğü ve sorgulama metodları (`GetAllFamiliars()`, `GetFamiliar(id)`).
- **`Scripts/Core/FamiliarManager.cs`:**
  - Singleton servis (`Instance`).
  - Durum takibi: `ActiveFamiliarId`, `Dictionary<string, FamiliarProgress>`.
  - Operasyonlar:
    - `CanUpgrade(string id)` / `Upgrade(string id)`
    - `SetActiveFamiliar(string id)`
    - `CheckWaveUnlocks(int currentWave)`
    - `GetActivePetDamage(float heroAtk)`
    - `GetActiveAttackInterval()`
    - `GetCritChanceBonus()`
    - `GetCritDamageBonus()`
    - `GetGoldMultiplierBonus()`
    - `GetDamageReductionBonus()`
  - Olaylar: `event Action<string> OnActiveFamiliarChanged`, `event Action<string, int> OnFamiliarUpgraded`.

### 4.2 Savaş Alanı Entegrasyonu (`PetCompanion.cs`)
- `PetCompanion` sahnesi `FamiliarManager.Instance.OnActiveFamiliarChanged` olayına abone olur.
- Seçili yoldaş değiştiğinde:
  - Sprite ve parçacık rengi dinamik olarak güncellenir.
  - Atış aralığı (`_shootInterval`) ve mermi hasarı yeni aktif yoldaşa göre hesaplanır.
  - `BloodProjectile` havuzlama yapısı korunur.

### 4.3 Kullanıcı Arayüzü Mimarisi
- **`Scripts/UI/FamiliarController.cs`:**
  - `CanvasLayer` (Layer 103) üzerinde bağımsız çalışır (`MainHUD.cs` 248 satır kuralını etkilemez).
  - HUD üst sağ barında `🦇 YOLDAŞ` butonu barındırır.
- **`Scripts/UI/FamiliarModal.cs`:**
  - Gotik modal penceresi: Vitray arka plan, altın/yakut çerçeveler.
  - Aktif yoldaş özeti ve auraları.
  - 4 Yoldaş Kartı: Seviye, Tier, İstatistikler, Seviye Yükselt butonu, Kuşan/Kuşanıldı butonu, Kilit durumları.
  - Pürüzsüz Tween animasyonu ile açılış ve kapanış.

### 4.4 Kalıcılık (`SaveData.cs` & `SaveSystem.cs`)
- `SaveData.cs` içerisine `ActiveFamiliarId` ve `FamiliarProgresses` listesi/sözlüğü eklenir.
- **Uyanış (Rebirth) Kuralı:** Yoldaş seviyeleri ve kilitleri Uyanış (Rebirth) sırasında **sıfırlanmaz**, kalıcı kazanım olarak korunur.

---

## 5. Bağlayıcı Kurallar Uyumu (GEMINI.md & AGENTS.md)
1. **Sınıf Satır Sınırı (< 250 Satır):**
   - `FamiliarModels.cs` (~60 satır)
   - `FamiliarDatabase.cs` (~95 satır)
   - `FamiliarManager.cs` (~195 satır)
   - `FamiliarController.cs` (~95 satır)
   - `FamiliarModal.cs` (~240 satır)
   - `MainHUD.cs` ve `GameManager.cs` kesinlikle 248 satır altında kalacaktır.
2. **Game Feel:** `Engine.TimeScale`'e asla dokunulmaz.
3. **Sayı Hijyeni:** Para hesaplamalarında `double` kullanılır.

---

## 6. Birim Testleri & Doğrulama Kapısı
- **`Tests/FamiliarSystemTests.cs` (xUnit):**
  - Tüm yoldaşların veritabanı bütünlüğü.
  - Seviye atlama maliyetlerinin ($1.15^{Lv-1}$ ve Tier başı parşömen) test edilmesi.
  - Bakiye yetersizliği ve başarılı seviye atlama kontrolleri.
  - Aktif yoldaş değişimi ve aura çarpanlarının matematiksel doğruluğu.
  - Dalga kilidi açılma mantığı.
  - Save/Load serileştirme döngüsü.
- `dotnet build` (0 warning, 0 error).
- `dotnet test` (83+ birim testi geçmeli).
