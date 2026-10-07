---
name: bloodseal-balance
description: "Mathematical progression formulas, wave scaling, Boss enrage, offline progress, and economy balance rules for BloodSeal."
---

# 🩸 BloodSeal Game Balance & Mathematical Economy Standards

Bu doküman, **BloodSeal** GDD belgesindeki temel savaş matematiği, dalga ölçeklendirmesi ve boşta ilerleme (idle) standartlarını belirler.

## 1. Yükseltme Maliyet Formülü (Pentagram Stats)

GDD'de belirlenen bağlayıcı kural gereği tüm temel istatistikler (ATK, ATK Speed, Lifesteal, Max HP, Range) için tek taban üstel maliyet formülü uygulanır:

$$Cost = BaseCost \times 1.15^{(Level - 1)}$$

- **Büyüme Katsayısı:** Tüm istatistikler için standart $1.15$'tir.
- **Konfigürasyon Yeri:** Katsayılar ve taban maliyetler kod içerisine veya `Data/BalanceConfig.json` dosyasına bağlanır. Ajanlar farklı katsayıları (örneğin 1.18, 1.22 gibi test edilmemiş deneysel yer tutucuları) kesin kural saymamalıdır.
- **ATK Speed Taban:** 1.0 vuruş/sn (Maksimum sınır: 3.5 vuruş/sn).
- **Lifesteal Taban:** %1.0 (Maksimum sınır: %25.0).

## 2. Düşman ve Dalga Büyüme Eğrisi

- **Minyon Canı (HP):** $Wave \times 25 + 50$
- **Minyon Hasarı (ATK):** $Wave \times 3 + 5$
- **Minyon Altın Ödülü:** $Wave \times 5 + 10$
- **Boss Canı (Her 10. dalgada):** $Wave \times 220 + 450$
- **Boss Hasarı:** $Wave \times 14 + 25$
- **Boss Altın Ödülü:** $Wave \times 60 + 250$

### Boss Kademeli Öfkelenme (Enrage)
- Sabit ölüm sayacı (enrage timer) yerine zamanla hasar çarpanı artar.
- Tek doğruluk kaynağı `Data/BalanceConfig.json` (`bossEnrage`) dosyasıdır.
- `isMultiplicative: false` (varsayılan toplamsal mod) olduğunda her 5 saniyede bir Boss hasarı taban hasarın $+%25$'i kadar artar:
  $$BossDamage = BaseBossDamage \times (1.0 + 0.25 \times EnrageAdimi)$$
  *(60. saniyede 12 adım ile tam $4.0\times$ çarpan oluşur).*
- `isMultiplicative: true` (çarpımsal mod) seçilirse çarpan her adımda bileşik katlanır ($(1.0 + 0.25)^{EnrageAdimi}$, 60. saniyede $\approx 14.55\times$).

## 3. Yenilgi ve Güvenli Farm Döngüsü
- Kahraman Boss dalgasında ($N$) ölürse oyun bitmez; anında tam canla bir önceki güvenli dalgaya ($N - 1$) çekilir.
- Oyuncu "Boss'a Yeniden Meydan Oku" butonuna basana kadar bu dalgada kesintisiz farm yapılır.

## 4. Çevrimdışı İlerleme (Offline Progress) & Zaman Güvenliği
- Zaman damgaları `DateTimeOffset.UtcNow.ToUnixTimeSeconds()` ile UTC üzerinden tutulur.
- **Cihaz Saati & Negatif Süre Koruması:** Cihaz saatinin geriye alınması veya aşırı ileri sarılmasına karşı süre sınırlandırılır:
  $$GecerliSure = \operatorname{Clamp}(SuAnkiZaman - KayitZamani, 0, 21600)$$
- Başlangıç tavan süresi **6 Saattir** (21.600 saniye).
- Çevrimdışı altın: $GecerliSure \times TemizlenenDalgaSaniyeBasiAltin$.

## 5. Büyük Sayı (Big Number) & Taşma Güvenliği
- Tüm para ve hasar hesaplamalarında 32-bit tamsayı taşmasını (overflow) önlemek için `long` ve `double` tipleri kullanılır.
- UI sayı formatlaması:
  - `< 1,000`: Tam sayı (`850`)
  - `>= 1,000`: `1.2K`
  - `>= 1,000,000`: `4.5M`
  - `>= 1,000,000,000`: `12.8B`
  - `>= 1,000,000,000,000`: `3.1T`
