---
name: bloodseal-art-style
description: "Dark gothic fantasy visual direction, color palette, parallax layer composition, particle rules, and combat VFX standards for BloodSeal."
---

# 🩸 BloodSeal Visual & Art Style Guide

This skill defines the artistic identity, color harmony, particle rules, and visual feedback guidelines for **BloodSeal**.

## 1. Color Palette & Mobile Contrast Rules (Dark Gothic)

- **Blood Crimson:** `#D91A2A` / `Color(0.85, 0.1, 0.16)` — Core runes, blood slash arcs, critical alerts, moon halo.
- **Abyssal Void:** `#0A080F` / `Color(0.04, 0.03, 0.06)` — Background sky base, deep shadows.
- **Ruin Slate:** `#1A1624` / `Color(0.1, 0.08, 0.14)` — Spire silhouettes, ruined columns, castle ironwork.
- **Cobblestone Ground:** `#2A222B` / `Color(0.16, 0.13, 0.17)` — Walkway, foreground stones, graves.
- **Runic Amber Gold:** `#FFC845` / `Color(1.0, 0.78, 0.27)` — Gold currencies, rare loot, awakening seals.
- **Bone Pale:** `#E8E0D5` / `Color(0.91, 0.88, 0.83)` — Skull masks, hero skin highlights, damage text.

### Mobile Screen Readability & Rim Lighting (Contrast Rule)
Arka plan çok koyu (`#0A080F` ~ `#2A222B`) olduğundan mobil OLED/IPS ekranlarda karakterlerin kaybolmasını engellemek için:
- **Karakter Siluet Kontrastı:** Kahraman ve düşmanların dış hatlarında en az 1-2px açık renkli kenar ışığı (Rim Light / pale outline) veya açık kemik/kızıl renk vurguları kullanılır.
- **Zemin Ayrımı:** Kahramanın ve canavarların ayak hizasında hafif zemin sisi veya ambient ışık halkası bulunur.

## 2. Katmanlı Paralaks & Otomatik Kaydırma (Parallax Composition)

Kahraman solda sabit dövüşürken dünyanın aktığını hissettirmek için `ParallaxBackground` otomatik kaydırılır (`scroll_offset.x += speed * delta`):

1. **Gök Katmanı (SkyLayer - `motion_scale = (0.1, 0.1)`):** Derin mor-siyah degrade, Kızıl Ay ve hare (`scroll_speed = 8 px/sn`).
2. **Harabe Katmanı (RuinsLayer - `motion_scale = (0.4, 0.4)`):** Gotik kule siluetleri, demir parmaklıklar, kırmızı kül ve sis parçacıkları (`scroll_speed = 35 px/sn`).
3. **Ön Zemin Katmanı (ForegroundLayer - `motion_scale = (1.0, 1.0)`):** Taş parke zemin, mezar taşları ve kan lekeleri (`scroll_speed = 90 px/sn`).

## 3. Game Feel & Combat VFX Kuralları (Bağlayıcı)

- **Engine.TimeScale'e ASLA DOKUNULMAZ:** Hit-freeze kesinlikle yerel aktör görseli/tween'i üzerinde ~40 ms mikro duraksama ile yapılır. Küresel motor hızı değişmez.
- **Hit-Freeze Koşulları:** Yalnızca Boss kritiği ve Boss ölümünde çalışır. Dahili bekleme süresi (ICD) en az 0.4 saniyedir. **Berserk modunda kesinlikle kapalıdır.**
- **Camera Trauma & Sarsıntı:**
  - Trauma her zaman `Clamp(0, 1)` ile sınırlandırılır.
  - Sabit sönüm hızı: `1.5/sn`.
  - Başlangıç değerleri:
    - Normal vuruş: `+0.10` trauma
    - Berserk dışı kritik: `+0.20` trauma
    - Alınan hasar: `+0.25` trauma
    - Boss ölümü: `+0.50` trauma
  - **Berserk Modunda Sarsıntı:** Seri vuruş başına shake **eklenmez**. Berserk boyunca sabit taban `0.15` trauma uygulanır.
- **Slash Arc VFX:** Kılıç savurmasında hilal ışık efekti. Berserk modunda %60 büyütülür ve turuncu-kızıl renge geçer.
- **Bağımsız Parçacık Yaşam Döngüsü:** Kan sıçraması ve ölüm patlaması `FXManager` altına doğar; düşman `QueueFree` olsa bile parçacıklar kaybolmaz.
