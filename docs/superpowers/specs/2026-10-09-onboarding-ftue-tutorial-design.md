# 🩸 BloodSeal: Öğretici & İlk Kullanıcı Deneyimi Tasarım Belgesi (Onboarding / FTUE)

**Belge Tarihi:** 2026-10-09  
**Durum:** Onaylandı / Uygulama Planı Hazırlanıyor  
**Yazar:** Antigravity  
**Kapsam:** 3. Adım - Bağımsız `TutorialController`, `TutorialHintCallout` ve `TutorialManager` ile oyun akışını kesmeyen, gotik estetikli ilk kullanıcı deneyimi.

---

## 1. Genel Bakış ve Amaç

BloodSeal'in ilk dakikalarında yeni oyuncunun oyun döngüsünü (Game Loop) doğal ve akıcı bir şekilde öğrenmesi gerekmektedir:
1. **Dövüş:** Ekrana dokunarak düşmanlara vurma ve öfke (Rage) biriktirme.
2. **Gelişim:** Biriken altın ile temel saldırı gücünü (ATK) yükseltme.
3. **Güç Patlaması:** Dolu öfke göstergesi ile Berserk modunu tetikleme.
4. **Keşif / Hikaye:** Dalga 5'te açılan Malikane Kapısı ile ilk kadim parşömeni ve mühür seçimini yapma.

Tüm bu süreç modal pencerelerle oyunu dondurmadan, ekranda hafif nabız atan (pulsing) gotik işaretçiler ve ipucu balonları (`TutorialHintCallout`) ile oyuncuya rehberlik edecektir.

---

## 2. Mimari İlkeler & Kısıtlar

1. **250 Satır Kuralı:**
   - `MainHUD.cs` halihazırda 248 satırdır ve bu dosyaya doğrudan öğretici mantığı eklenmeyecektir (0 satır artış).
   - `GameManager.cs` 248 satırdır, tutorial durumu `TutorialManager` içine delege edilecektir.
   - Tüm yeni sınıflar (`TutorialManager`, `TutorialController`, `TutorialHintCallout`) 150 satırın altında tutulacaktır.
2. **Girdi Engellememe (Non-Intrusive Overlay):**
   - Öğretici UI elemanları (`TutorialHintCallout`) `MouseFilter = MouseFilterEnum.Ignore` olarak yapılandırılacak; oyuncunun savaş alanına veya butonlara tıklaması kesinlikle engellenmeyecektir.
3. **Zaman ve Motor Kuralları:**
   - `Engine.TimeScale` asla değiştirilmeyecektir.
   - Sayaçlar gerçek delta ile çalışmaya devam edecektir.
4. **Kayıt ve Kalıcılık:**
   - `SaveData.TutorialStep` alanı üzerinden ilerleme JSON'a kaydedilecek; oyun yeniden başlatıldığında oyuncu kaldığı öğretici adımından devam edecektir.

---

## 3. Öğretici Adımları (TutorialStep)

| Adım | Enum Değeri | Tetikleyici & Koşul | Gösterilen İpucu / Hedef | Tamamlanma Şartı |
|---|---|---|---|---|
| 1 | `TapToAttack` | Dalga 1 başlangıcı | Savaş alanına işaret eden el/ok ikonu: *"Düşmanlara vurmak ve öfke toplamak için ekrana dokun!"* | Oyuncu ekrana 3 kez dokunduğunda veya ilk minyon öldüğünde |
| 2 | `UpgradeAttack` | 10+ Altın biriktiğinde | `UpgradeAtkBtn` üzerine hafif parıldayan ok: *"Saldırı gücünü artırmak için yükselt!"* | Oyuncu ATK butonuna bastığında |
| 3 | `ActivateBerserk` | Öfke %100 olduğunda | `RageButton` üzerine alevli gotik işaretçi: *"Öfken taştı! Berserk modunu başlat!"* | Oyuncu Berserk modunu açtığında |
| 4 | `VisitManorGate` | Dalga 5 temizlendiğinde | `GateNotificationBtn` üzerine işaretçi: *"Malikane Kapısı belirdi! Mührünü seç!"* | Malikane kapısı açıldığında / incelendiğinde |
| 5 | `Completed` | Kapı ödülü alındıktan sonra | Tüm ipuçları söner; öğretici tamamlanır. | Kalıcı olarak tamamlandı olarak kaydedilir. |

---

## 4. Dosya Yapısı & Sorumluluklar

1. **`Scripts/Core/TutorialModels.cs`**:
   - `TutorialStep` enum tanımı.
2. **`Scripts/Core/TutorialManager.cs`**:
   - Saf C# singleton servisi (`Instance`).
   - Durum makinesi, olaylar (`OnTutorialStepChanged`, `OnTutorialCompleted`).
   - xUnit ile %100 test edilebilir bağımsız iş mantığı.
3. **`Scripts/UI/TutorialHintCallout.cs`**:
   - Hafif yüzen, nabız atan işaretçi ve metin balonu.
   - `Control` tabanlı, `MouseFilter = Ignore`.
4. **`Scripts/UI/TutorialController.cs`**:
   - Sahneye dinamik bağlanan `Node`/`CanvasLayer` kontrolcüsü.
   - Hedef UI butonlarının pozisyonlarını bularak `TutorialHintCallout`'u konumlandırır.
5. **`Tests/TutorialSystemTests.cs`**:
   - xUnit birim testleri (adım geçişleri, sınır durumlar, Save/Load uyumluluğu).
