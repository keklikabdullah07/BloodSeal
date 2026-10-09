# 🩸 BloodSeal: Aktif Geliştirme Durumu ve Sonraki Adım (Handover Memory)

## 📌 Son Tamamlanan Sistemler (Hazır & Doğrulanmış)
1. **Öğretici & İlk Kullanıcı Deneyimi (Onboarding / FTUE - 3. Adım):**
   - **`TutorialManager.cs`:** Saf C# singleton servisi ile adım durum makinesi (`TapToAttack` -> `UpgradeAttack` -> `ActivateBerserk` -> `VisitManorGate` -> `Completed`).
   - **`TutorialHintCallout.cs`:** Gotik çerçeveli, hafif nabız atan işaretçi ok (`▲`/`▼`) ve dinamik ipucu balonu. `MouseFilter = MouseFilterEnum.Ignore` ile oyun ve UI tıklamalarını kesinlikle engellemez.
   - **`TutorialController.cs`:** Bağımsız `CanvasLayer` (Layer 105); `MainHUD.cs` satır sayısına dokunmadan butonların ekran koordinatlarını bularak ipuçlarını konumlandırır ve adım geçişlerinde kendini günceller.
   - **Kalıcılık (`SaveData.cs` & `SaveSystem.cs`):** `TutorialStep` kaydedilir ve oyun tekrar açıldığında kaldığı adımdan devam eder.
   - **Bağlayıcı Kurallara %100 Uyum:** `MainHUD.cs` ve `GameManager.cs` 248 satırda korundu, `SaveSystem.cs` 248 satır, tüm yeni sınıflar < 140 satır. `Engine.TimeScale` değiştirilmedi.
2. **Karakter & Vuruş Animasyon Zenginleştirmesi (Combat Motion & Juice):**
   - Kahraman (Combat Breathing idle, lunge & tilt), Minyonlar (Wind-up, strike, squash & stretch, knockback, death spin-fade), Boss (Çöküş tween'i).
3. **Düşman Çeşitliliği & Gotik Bölge İlerlemesi (Bölge I, II ve III):**
   - Harabeler, Kadim Kripta, Kan Katedrali; minyon ve boss dokuları.
4. **Malikane Kütüphanesi & Araştırma Ağacı:** 3 disiplin, 9 araştırma düğümü, parşömen düşüşleri ve `LibraryModal`.
5. **Kadim Yadigar Mahzeni & Uyanış (Rebirth):** 10 adet Boss yadigarı, Dalga 20+ prestij eşiği ve yetenek ağacı.
6. **Ses Mimarisi:** Çift kanallı BGM crossfade, 6 kanallı döngüsel SFX havuzu ve Gotik `SettingsModal`.
7. **Nesne Havuzu & Performans:** GC sıfırlama (Yüzen metinler, mermiler, vfx, minyonlar).
8. **Birim Testleri:** 75/75 xUnit testi başarılı (`dotnet test`).
9. **Derleme & Motor:** `dotnet build` 0 warning / 0 error; Godot headless smoke test 0 hata / 0 sızıntı.

---

## 🎯 Sıradaki Hedef / Önerilen Adımlar
1. **Başarımlar & Günlük Görevler Sistemi (Achievements & Daily Quests):**
   - Belirli düşman öldürme, dalga tamamlama, Berserk açma, yadigar toplama hedefleri.
   - Ödül olarak altın ve kan parşömeni verme.
2. **Haptic / Titreşim & Mobil Giriş İyileştirmeleri:**
   - Dokunmatik ekranlarda geri bildirim ve çoklu dokunuş (multi-touch) hissi.
3. **Android Build & Export Hazırlığı:**
   - Android debug export APK doğrulaması ve imzalama hazırlığı.
