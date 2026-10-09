# 🩸 BloodSeal: Düşman Çeşitliliği & Gotik Bölge İlerlemesi Tasarım Belgesi (Enemy Variety & Zone Progression)

**Belge Tarihi:** 2026-10-09  
**Durum:** Taslak / Onay Bekliyor  
**Yazar:** Antigravity  
**Kapsam:** Dalga 1-30+ arasında 3 farklı Gotik Bölge (Zone), her bölgeye özgü Minyon ve Boss varyasyonları ve HUD Bölge göstergesi.

---

## 1. Genel Bakış ve Amaç

Şu anda oyunda Dalga 1'den Dalga 100'e kadar aynı tarikatçı minyon ve aynı Boss sprite'ı doğmaktadır. Bu durum oyunun meta-ilerlemesinde görsel monotonluğa sebep olmaktadır.

Bu sistemin amacı; dalgaları 3 farklı Gotik Bölgeye ayırmak, her bölgede özgün görsel temaya sahip minyon ve boss'ların doğmasını sağlamak ve mevcut `NodePool<Enemy>` nesne havuzlama mimarisini sıfır çöp (zero GC) ile korumaktır.

---

## 2. Bölge (Zone) Dağılımı ve Düşman Tipleri

### A. Bölge I: Terk Edilmiş Harabeler (Dalga 1 - 10)
- **Tema:** Yıkık gotik mezarlık, sis ve Kızıl Ay.
- **Minyon:** `enemy_cultist.png` — *Kızıl Ahit Tarikatçısı* (Kemik maskeli, yırtık cübbeli hançerli minyon).
- **Boss (Dalga 10):** `boss_abomination.png` — *Heybetli Kan Lordu* (Dev boynuzlu iblis titan).

### B. Bölge II: Kadim Kripta & Yeraltı Mahzeni (Dalga 11 - 20)
- **Tema:** Yeraltı mezar odaları, mavi/kızıl ruh ateşi meşaleleri, paslı demirler.
- **Minyon:** `enemy_skeleton_warrior.png` — *Mezar Muhafızı* (Çatlak kemikli, paslı kalkan ve kılıç taşıyan karanlık iskelet).
- **Boss (Dalga 20):** `boss_crypt_revenant.png` — *Kripta Heyulası* (Devasa tırpan taşıyan, kaburgasında parıldayan antik ruh ateşi yanan iskelet kral).

### C. Bölge III: Kan Katedrali & Malikane İç Avlusu (Dalga 21+)
- **Tema:** Kan lekeli sivri katedral kemerleri, kırmızı vitray camlar, asılı gotik zincirler.
- **Minyon:** `enemy_gargoyle.png` — *Kızıl Muhafız Gargoyle* (Taşlaşmış kanatlı, pençeli karanlık yaratık).
- **Boss (Dalga 30, 40, ...):** `boss_vampire_patriarch.png` — *Vampir Patriği* (Sivri gotik zırhlı, kan kadehi ve rünik pelerin taşıyan heybetli kan büyücüsü lord).

---

## 3. Teknik Mimari & Dinamik Havuzlama (Zero-Breakage)

### A. `Enemy.cs` Varyasyon Desteği
- `Enemy.cs` sınıfına `SetupVariant(int wave)` mantığı eklenir.
- `WaveSpawner` düşmanı havuzdan alırken (`_enemyPool.Acquire()`), dalga numarasına göre:
  - `wave <= 10` -> `CultistTexture`
  - `wave <= 20` -> `SkeletonTexture`
  - `wave > 20` -> `GargoyleTexture`
- Sprite'ın `Texture` özelliği dinamik olarak güncellenir.
- Yeni nesne türetilmez (`GC allocation = 0`), havuzlanmış düğümler geri dönüştürülmeye devam eder.

### B. `BossEnemy.cs` Varyasyon Desteği
- `BossEnemy.cs` dalga numarasına göre Boss dokusunu günceller:
  - Dalga 10 -> `boss_abomination.png`
  - Dalga 20 -> `boss_crypt_revenant.png`
  - Dalga 30+ -> `boss_vampire_patriarch.png`

### C. HUD Bölge Göstergesi (`MainHUD.cs`)
- `MainHUD` üzerinde mevcut dalga etiketi altında veya yanında Bölge adı gösterilir:
  - `BÖLGE I: TERK EDİLMİŞ HARABELER`
  - `BÖLGE II: KADİM KRİPTA`
  - `BÖLGE III: KAN KATEDRALİ`

---

## 4. Kabul Kriterleri (Acceptance Criteria)

1. **Görsel Bütünlük:** Dalga 1-10 arası Tarikatçı, 11-20 arası İskelet Muhafızı, 21+ arası Gargoyle minyonları kusursuz şekilde görünmelidir.
2. **Boss Geçişleri:** Dalga 10, 20 ve 30 Boss'ları kendi özgün sprite'ları ile sahneye inmelidir.
3. **Performans:** `NodePool<Enemy>` bellek tahsisi yapmadan sprite değiştirerek çalışmalı, `dotnet test` ve `dotnet build` 0 hata ile tamamlanmalıdır.
4. **Sınıf Sınırı:** Hiçbir dosya 250 satırı aşmayacaktır.
