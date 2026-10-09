# 🩸 BloodSeal: Karakter & Vuruş Animasyon Zenginleştirmesi Tasarım Belgesi (Combat Motion & Juice)

**Belge Tarihi:** 2026-10-09  
**Durum:** Taslak / Onay Bekliyor  
**Yazar:** Antigravity  
**Kapsam:** 2. Adım - Kahraman, Minyonlar ve Boss'lar için prosedürel nefes alma (breathing idle), vuruş atılması (lunge & tilt), darbe ezilmesi (squash & stretch), saldırı gerilmesi (wind-up) ve ölüm savrulması (death spin-fade).

---

## 1. Genel Bakış ve Amaç

Şu anda tüm karakterler tek kare sprite'lar olarak durmaktadır ve hareketler yalnızca doğrusal `position:x` geçişlerinden ibarettir.

Bu tasarımın amacı; `game-feel` ve `bloodseal-art-style` standartlarına bağlı kalarak, `Engine.TimeScale`'e dokunmadan ve 250 satır sınırını aşmadan, oyuna akıcı, organik ve son derece tok ("punchy") mikro-animasyonlar kazandırmaktır.

---

## 2. Animasyon Katmanları & Mekanikler

### A. Kahraman (Hero Motion)
1. **Canlı Duruş (Combat Breathing):**
   - Saldırı bekleme süresindeyken hafif sinüzoidal dikey nefes alma salınımı (`scale.y: 1.0 <-> 1.025`, `position.y: +-1.5 px`).
2. **Kılıç Savurma & Öne Atılma (Attack Lunge & Tilt):**
   - Vuruş anında gövde öne eğilir (`rotation = 0.08 rad`).
   - İleri atılma (`position.x = 35 px`) ve yaylanarak geri dönüş (`Back.Out` -> `Elastic.Out`).
   - Hilal ışık arkı (`SlashVfx`) ile mükemmel senkronizasyon.

### B. Minyonlar (Enemy Motion)
1. **Darbe Tepkisi (Hit Squash & Stretch):**
   - Hasar aldığında darbe yönünde yatay genişleme ve dikey basılma (`scale = (1.15, 0.88)` -> 80 ms içinde `(1.0, 1.0)`).
   - Mikro-geri tepme (Knockback: `position.x += 10 px`).
2. **Saldırı Yaylanması (Attack Wind-up & Strike):**
   - Kahramana vuruş yapmadan önce 60 ms geriye çekilme (`position.x += 8 px`), ardından kahramana doğru sert bir ileri atılma (`position.x -= 22 px`).
3. **Ölüm Savrulması (Death Spin & Fade):**
   - Düşman canı 0 olduğunda anında yok olmak yerine 0.14 saniye boyunca arkaya savrulur, hafif döner (`rotation = -0.35 rad`), küçülür ve solar (`modulate.a -> 0`).
   - Ardından havuza (`_enemyPool.Release(this)`) iade edilir.

### C. Boss Heybeti (Boss Motion)
1. **Ağır Adım & Sarsıcı Vuruş:**
   - Minyonlara kıyasla daha ağır, devasa kütle hissi veren vuruş yaylanması (`scale.y` 1.15'e basılma).
2. **Enrage Titremesi (Enrage Rumble):**
   - Enrage adımı arttıkça gövdede hafif kızıl titreşim mikro-salınımı.

---

## 3. Bağlayıcı Kurallara Uyum
- **`Engine.TimeScale`:** ASLA değiştirilmez. Hit-freeze sadece `FXManager` yerel mikro-duraksaması ile sürdürülür.
- **Berserk Kuralı:** Berserk modunda vuruş başına sarsıntı eklenmez, sabit taban 0.15 trauma korunur.
- **Bellek ve Havuzlama:** Ölüm animasyonları `Tween` tamamlandığında `ReleaseEnemy` çağırarak `NodePool<T>` döngüsünü aksatmaz.
- **Sınıf Sınırı:** `Hero.cs` ve `Enemy.cs` 250 satır sınırının altında tutulacaktır.
