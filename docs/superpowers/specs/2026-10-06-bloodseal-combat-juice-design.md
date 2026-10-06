# BloodSeal: Combat Juice & VFX Tasarım Dokümanı (Spec)

- **Proje:** BloodSeal (2D Gotik Idle RPG)
- **Motor / Dil:** Godot 4.7.2 (.NET 10 / C#)
- **Platform:** 1920x1080 (16:9 Yatay), GL Compatibility Renderer
- **Tarih:** 2026-10-06
- **Durum:** Onaylandı

---

## 1. Amaç & Kapsam
Bu doküman, BloodSeal temel savaş döngüsünün vuruş hissiyatını (Combat Juice) ve karanlık gotik görsel tatminini zirveye taşımak için gerekli Kamera Sarsıntısı (Screen Shake), Kılıç Kan Savurma Kavisi (Slash Arc), Darbe Kan Sıçraması (Blood Splatter), Düşman Ölüm Patlaması (Death Explosion), Tıklama Şok Dalgası (Tap Ripple) ve Mikro Duraksama (Hit-Freeze) sistemlerini tanımlar.

---

## 2. Sistem Mimarisi

```text
blood-seal/
├── Scenes/
│   ├── MainCombat.tscn               # Camera2D ve FXManager düğümleri eklenecek
│   └── VFX/
│       ├── SlashVfx.tscn             # Kılıç savurma kan hilali
│       ├── BloodSplatter.tscn        # Darbe kan sıçraması parçacığı
│       ├── DeathExplosion.tscn       # Düşman/Boss ölüm patlaması
│       └── TapRipple.tscn            # Ekrana dokunma şok dalgası
└── Scripts/
    └── Combat/
        ├── CameraShake.cs            # Camera2D sarsıntı kontrolcüsü (Trauma decay)
        └── FXManager.cs              # Tüm efektlerin üretim ve yaşam döngüsü yöneticisi
```

---

## 3. Bileşen Detayları

### 3.1 CameraShake (`CameraShake.cs` - `Camera2D`)
- **Konum:** `Vector2(960, 540)` (Ekran merkezi).
- **Travma Mantığı:**
  - `_trauma` değeri 0.0f ile 1.0f arasındadır.
  - Her karede `_trauma = Mathf.Max(0, _trauma - DecayRate * (float)delta)` ile sönümlenir (`DecayRate = 1.6f`).
  - Maksimum sarsıntı ofseti: `MaxOffset = 22f`.
  - Formül: `Offset = new Vector2(GD.Randf() * 2 - 1, GD.Randf() * 2 - 1) * MaxOffset * (_trauma * _trauma)`.
- **API:**
  - `AddTrauma(float amount)`: Dışarıdan travma ekler (`Mathf.Clamp(_trauma + amount, 0f, 1f)`).

### 3.2 FXManager (`FXManager.cs` - Singleton)
- **API Metotları:**
  - `PlaySlash(Vector2 pos, bool isBerserk)`: Kılıç savurma hilalini üretir.
  - `PlayBloodSplatter(Vector2 pos, Vector2 direction)`: Darbe alan noktada yönlü kan sıçratır.
  - `PlayDeathExplosion(Vector2 pos, bool isBoss)`: Düşman veya Boss öldüğünde patlama parçacığı oluşturur.
  - `PlayTapRipple(Vector2 pos)`: Tıklanan noktada genişleyen kan dalgası oluşturur.
  - `TriggerHitFreeze(float duration = 0.05f)`: `Engine.TimeScale = 0.05f` yaparak anlık dondurur ve süre bitince `1.0f` değerine çeker.

### 3.3 VFX Sahneleri
1. **`SlashVfx.tscn`:** Hilal şeklinde kırmızı kavisli `Polygon2D`. 0.12 saniyede ileri doğru esneyip solarak silinir.
2. **`BloodSplatter.tscn`:** `CPUParticles2D`, 16 adet fışkıran kan damlası, yerçekimli (`gravity = Vector2(0, 350)`), 0.35s ömür.
3. **`DeathExplosion.tscn`:** `CPUParticles2D`, 28 adet radyal patlayan kırmızı-siyah parçacık + anlık genişleyen halka, 0.5s ömür.
4. **`TapRipple.tscn`:** Genişleyen kırmızı çember ve kıvılcımlar, 0.25s ömür.

---

## 4. Entegrasyon Noktaları
- **`Hero.cs`:**
  - Kılıç vuruşu yapıldığında: `FXManager.Instance.PlaySlash(target.GlobalPosition, isRage);`
  - Vuruş kritik ise: `CameraShake.Instance?.AddTrauma(0.35f);` ve `FXManager.Instance.TriggerHitFreeze(0.04f);`
  - Normal vuruş ise: `CameraShake.Instance?.AddTrauma(0.12f);`
- **`Enemy.cs` / `BossEnemy.cs`:**
  - Darbe aldığında: `FXManager.Instance.PlayBloodSplatter(GlobalPosition + Vector2.Up * 40, Vector2.Right);`
  - Can 0 olup öldüğünde: `FXManager.Instance.PlayDeathExplosion(GlobalPosition + Vector2.Up * 40, this is BossEnemy);`
- **`TapCombatArea.cs`:**
  - Ekrana dokunulduğunda: `FXManager.Instance.PlayTapRipple(tapPos);` ve `CameraShake.Instance?.AddTrauma(0.1f);`

---

## 5. Doğrulama & Test Planı
- `dotnet build`: 0 hata, 0 uyarı.
- Godot headless test: `MainCombat.tscn` yeni efekt ve kamera düğümleriyle sorunsuz başlatılır.
- Oynanış doğrulaması: Kılıç savrulunca slash kavisi çıkar, vurulan düşmandan kan fışkırır, ekrana dokununca şok dalgası oluşur, kritik vuruşta kamera sarsılır ve mikro duraksama hissedilir.
