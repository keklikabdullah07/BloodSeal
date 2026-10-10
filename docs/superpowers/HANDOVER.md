# 🩸 BloodSeal: Aktif Geliştirme Durumu ve Sonraki Adım (Handover Memory)

## 📌 Son Tamamlanan Sistemler (Hazır & Doğrulanmış)
1. **Öğretici & İlk Kullanıcı Deneyimi (Onboarding / FTUE - 3. Adım):**
   - **`TutorialManager.cs`:** Saf C# singleton servisi ile adım durum makinesi (`TapToAttack` -> `UpgradeAttack` -> `ActivateBerserk` -> `VisitManorGate` -> `Completed`).
   - **`TutorialHintCallout.cs`:** Gotik çerçeveli, hafif nabız atan işaretçi ok (`▲`/`▼`) ve dinamik ipucu balonu. `MouseFilter = MouseFilterEnum.Ignore` ile oyun ve UI tıklamalarını engellemez.
   - **`TutorialController.cs`:** Bağımsız `CanvasLayer` (Layer 105); `MainHUD.cs` satır sayısına dokunmadan butonların ekran koordinatlarını bularak ipuçlarını konumlandırır.
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

## 🎯 Yeni Sohbette Başlanacak Sıradaki Sistem: Başarımlar & Günlük Kan Görevleri (Quest & Achievement System)
Kullanıcı ile beyin fırtınası (brainstorming) tamamlanmış ve tüm mimari tercihler kesinleşmiştir:

### Kesinleşen Tasarım Kararları:
1. **İki Katmanlı Kapsam:**
   - **Kalıcı Gotik Başarımlar (Milestones):** Uzun vadeli hedefler (Düşman öldürme, Boss kesme, Dalga geçme, Berserk açma, Stat yükseltme).
   - **Günlük Kan Avı Görevleri (Daily Bounties):** 24 saatte bir (Unix timestamp ile) yenilenen 3 dinamik görev.
2. **Ödül Yapısı:**
   - Günlük Görevler: Bol Altın + Parşömen.
   - Kalıcı Başarımlar: Parşömen + Uyanış Puanı (Awakening Points) + Altın.
3. **Arayüz Entegrasyonu:**
   - Üst/Sağ HUD Barında Parıldayan Gotik Mühür İkonu (bildirim rozetli).
   - Tam Gotik Modal (`QuestModal.cs`): 2 sekmeli (Kalıcı Başarımlar / Günlük Av), kaydırılabilir liste, ilerleme çubuğu ve parıldayan "Talep Et" butonu.
   - `MainHUD.cs` satır sayısına dokunulmayacak (248 satırda kalacak); modal bağımsız kontrolcü / sahne referansı ile yönetilecek.
4. **Mimari:**
   - **Yaklaşım 1 (Birleşik `QuestManager.cs`):** Saf C# tek yönetici, olay tabanlı sayaçlar (`OnEnemyDied`, `OnWaveChanged`, `OnTap`, `OnRageActive`, `OnStatsUpgraded`).
   - `QuestModels.cs`, `QuestDatabase.cs`, `QuestManager.cs` (Saf C# -> xUnit ile %100 test edilebilir).
   - `SaveData.cs` ve `SaveSystem.cs` içinde görev ilerlemelerinin ve son sıfırlama zamanının (`LastDailyResetTimestamp`) saklanması.
