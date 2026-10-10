# 🩸 BloodSeal: Gotik Ekipman & Eşya Kuşanma (Gothic Gear & Inventory) Sistemi Tasarımı

## 1. Genel Bakış ve Amaç
BloodSeal projesinin idle savaş döngüsünü derinleştirmek, ganimet heyecanını (loot drop satisfaction) artırmak ve oyuncuya stratejik build çeşitliliği sunmak amacıyla **Gotik Ekipman & Envanter Sistemi** geliştirilmektedir.

Oyuncu, her 10. dalgadaki Boss infazlarından garanti olarak ve elit düşmanlardan şans eseri ekipmanlar düşürür. Bu ekipmanlar 4 slota (Silah, Zırh, Tılsım, Yüzük) kuşanılabilir, 5 farklı Gotik nadirlik derecesine (Yaygın, Nadir, Epik, Efsanevi, Kadim Kan) sahip olabilir ve Demirhanede altın harcanarak +10 seviyeye kadar bilenebilir. İstenmeyen eşyalar ise parçalanarak doğrudan altına dönüştürülür.

---

## 2. Ekipman Yapısı, Slotlar ve Nadirlik Kademeleri

### 2.1 Ekipman Slotları
1. **Silah (Weapon):** Birincil Stat = Saldırı Gücü (ATK). İkincil Statlar = Kritik Şans, Kritik Hasar, Delme.
2. **Zırh (Armor):** Birincil Stat = Maksimum Can (Max HP). İkincil Statlar = Hasar Azaltma (Zırh), Can Yenileme.
3. **Tılsım (Amulet):** Birincil Stat = Can Çalma (Lifesteal). İkincil Statlar = Öfke (Rage) Kazanımı, Berserk Süresi.
4. **Yüzük (Ring):** Birincil Stat = Saldırı Hızı (Atk Speed). İkincil Statlar = Altın Çarpanı, Kritik Şans.

### 2.2 Nadirlik Kademeleri (Rarity Tiers)
| Nadirlik | Renk Kodu | Stat Çarpanı | İkincil Stat Adedi | Özel Efekt |
|---|---|---|---|---|
| **Yaygın (Common)** | `#B0B0B0` (Gri/Beyaz) | 1.0x | 0 | Yok |
| **Nadir (Rare)** | `#3A86FF` (Gece Mavisi) | 1.35x | 1 | Yok |
| **Epik (Epic)** | `#9D4EDD` (Kripta Moru) | 1.80x | 2 | Yok |
| **Efsanevi (Legendary)** | `#FFB703` (Kadim Altın) | 2.50x | 2 | +Özel Gotik Pasif |
| **Kadim Kan (Ancient Blood)** | `#D00000` (Yakut Kırmızı) | 3.50x | 3 | +Kadim Kan Bağı Aurası |

---

## 3. 16 Gotik Eşya Kütüphanesi (`EquipmentDatabase.cs`)

### ⚔️ Silahlar (Weapon)
1. `weap_blood_rapier` — **Kızıl Meç:** Hızlı düello kılıcı (+15 ATK, +%5 Atk Hızı).
2. `weap_executioner_blade` — **İnfazcı Palası:** Ağır çift el pala (+28 ATK, +%15 Kritik Hasar).
3. `weap_crypt_scythe` — **Kripta Tırpanı:** Ruh biçen kavisli tırpan (+20 ATK, +%2 Can Çalma).
4. `weap_vampire_fang` — **Vampir Hançeri:** Sivri diş hançeri (+18 ATK, +%8 Atk Hızı, +%4 Kritik).

### 🛡️ Zırhlar (Armor)
1. `arm_night_cloak` — **Gece Muhafızı Pelerini:** Koyu yün pelerin (+80 Max HP, +%3 Hasar İndirimi).
2. `arm_bone_carapace` — **Kemik Zırhı:** İskelet göğüs kafesi zırhı (+140 Max HP, +%5 Hasar İndirimi).
3. `arm_inquisitor_tunic` — **Engizisyon Cübbesi:** Kutsal kumaş cübbe (+110 Max HP, +%8 Öfke Kazanımı).
4. `arm_blood_regalia` — **Kadim Kan Zırhı:** Pıhtılaşmış kandan dövülmüş zırh (+220 Max HP, +%8 Hasar İndirimi).

### 📿 Tılsımlar (Amulet)
1. `amu_covenant_pendant` — **Ahit Madalyonu:** Bakır ters pentagram (+%2.5 Can Çalma).
2. `amu_ashen_rosary` — **Kül Tespihi:** Zangoç tespihi (+%4 Kritik Şans, +%1.5 Can Çalma).
3. `amu_heart_locket` — **Taşlaşmış Kalp:** Kömürleşmiş madalyon (+60 Max HP, +%2.0 Can Çalma).
4. `amu_blood_tear` — **Vampir Gözyaşı:** Yakut damla kolye (+%15 Kritik Hasar, +%3.0 Can Çalma).

### 💍 Yüzükler (Ring)
1. `ring_darius_signet` — **Darius Mührü:** Hanedan mühür yüzüğü (+%8 Atk Hızı, +%10 Altın).
2. `ring_ruby_band` — **Yakut Kan Halkası:** Parlak yakut halka (+%5 Kritik Şans, +%8 Atk Hızı).
3. `ring_thorn_circle` — **Dikenli Yüzük:** Sivri demir yüzük (+10 ATK, +%6 Atk Hızı).
4. `ring_eternal_seal` — **Ebedi Ahit Yüzüğü:** 13 mührün minyatürü (+12 ATK, +70 HP, +%6 Atk Hızı).

---

## 4. Matematiksel Model ve Formüller

### 4.1 Boss Düşüş Dağılımı (`RollDrop(wave, isBoss)`)
- **İhtimal:** Boss (%100 Garanti), Elit/Mini-Boss (%35).
- **Dalgaya Bağlı Nadirlik Oranları:**
  - *Dalga 1 - 9:* Common %85, Rare %15
  - *Dalga 10 - 24:* Common %50, Rare %40, Epic %10
  - *Dalga 25 - 49:* Common %20, Rare %45, Epic %25, Legendary %10
  - *Dalga 50+:* Common %10, Rare %30, Epic %35, Legendary %20, AncientBlood %5

### 4.2 Demirhane Bileme Maliyeti (+0 .. +10 Seviye)
$$Cost_{Gold} = BaseCost \times 1.15^{(Level)} \times RarityMult$$
- Taban Maliyet: Common = 150, Rare = 300, Epic = 600, Legendary = 1500, AncientBlood = 4000.
- Her seviye eşyanın birincil statını +%10 artırır:
$$PrimaryValue = BaseValue \times (1.0 + 0.10 \times Level)$$

### 4.3 Parçalama / Satış Değeri (Salvage/Sell)
$$SellGold = \text{floor}\left(\frac{BaseCost \times RarityMult \times (1.0 + 0.5 \times Level)}{2}\right)$$

---

## 5. Mimari ve Bileşenler

### 5.1 Saf C# Modelleri (`EquipmentModels.cs` & `EquipmentDatabase.cs`)
- `enum EquipmentSlot`, `enum EquipmentRarity`, `class EquipmentItem`, `class EquipmentDefinition`.
- 16 tanımlı eşyanın statik sözlüğü.
- 0 Godot bağımlılığı, %100 saf C# ve xUnit ile test edilebilir.

### 5.2 Yönetici Servis (`EquipmentManager.cs`)
- Singleton servis (`Instance`).
- Durum: `Dictionary<EquipmentSlot, EquipmentItem?> EquippedItems`, `List<EquipmentItem> BagItems` (Kapasite: 24).
- Operasyonlar:
  - `Equip(string instanceId)`, `Unequip(EquipmentSlot slot)`
  - `UpgradeItem(string instanceId, GameManager gm)`
  - `DismantleItem(string instanceId, GameManager gm)`
  - `RollDrop(int wave, bool isBoss)`
  - `GetTotalBonus(StatType type)`
- Olaylar: `event Action OnEquipmentChanged`, `event Action<EquipmentItem> OnItemAcquired`.

### 5.3 Kullanıcı Arayüzü Mimarisi (`InventoryModal.cs` & `InventoryController.cs`)
- `InventoryController`: `CanvasLayer` (Layer 102). HUD üst çubuğunda `Position = Vector2(1430, 16)` konumunda `🛡️ ENVANTER` butonu.
- `InventoryModal`:
  - Sol Taraf: 4 Kuşanılmış Yuva, Aktif Donanım İstatistik Özeti.
  - Sağ Taraf: 24 Yuvalı Çanta Izgarası (`GridContainer`), Seçili Eşya Detayı, "Kuşan/Çıkar", "Bile (+1)", "Parçala" aksiyonları.
  - Tween animasyonlu pürüzsüz açılış ve kapanış.

### 5.4 Kalıcılık (`SaveData.cs` & `SaveSystem.cs`)
- `SaveData.EquippedItems` ve `SaveData.BagItems` listesi serileştirilir.
- **Uyanış (Rebirth) Kuralı:** Ekipmanlar ve envanter Uyanışta **sıfırlanmaz**, kalıcı prestij yatırımı olarak korunur.

---

## 6. Bağlayıcı Kurallar ve Testler
- **Satır Sınırı (< 250 Satır):** `EquipmentModels.cs` (~95), `EquipmentDatabase.cs` (~125), `EquipmentManager.cs` (~215), `InventoryModal.cs` (~245), `InventoryController.cs` (~85). `MainHUD.cs` ve `GameManager.cs` < 248 korunacaktır.
- **Game Feel:** `Engine.TimeScale` değiştirilmez.
- **Birim Testleri (`Tests/EquipmentSystemTests.cs`):** 16 eşya bütünlüğü, düşüş tablosu doğrulaması, kuşanma/çıkarma, demirhane maliyetleri ve stat çarpanları, save/load döngüsü.
