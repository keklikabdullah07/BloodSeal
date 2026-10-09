# 🩸 BloodSeal: Aktif Geliştirme Durumu ve Sonraki Adım (Handover Memory)

## 📌 Son Tamamlanan Sistemler (Hazır & Doğrulanmış)
1. **Karakter & Vuruş Animasyon Zenginleştirmesi (Streamlined Combat Motion & Juice):**
   - **Kahraman (`Hero.cs`):** Canlı dövüş nefes alma salınımı (Combat Breathing idle), kılıç savururken öne eğilme (tilt: 0.08 rad) ve elastik geri yaylanmalı ileri atılma (lunge: 35px).
   - **Minyonlar (`Enemy.cs`):** Vuruş öncesi geriye kurulma (wind-up: 8px) ve sert hücum (strike: -20px); hasar aldığında darbe yönünde yatay esneme/dikey basılma (Squash & Stretch: 1.14x, 0.88x) ve 10px mikro-geri tepme.
   - **Ölüm Savrulması (Death Spin-Fade):** Minyonlar öldüğünde 0.13 saniyelik geriye savrulma, dönme (-0.35 rad) ve solma animasyonu ile havuza (`NodePool<Enemy>`) temiz dönüş.
   - **Boss Çöküşü (`BossEnemy.cs`):** Boss ölümünde sarsıcı devasa çöküş ve yok oluş tween'i.
   - **Bağlayıcı Kurallara %100 Uyum:** `Engine.TimeScale` değiştirilmedi, yerel hit-freeze ve Berserk kuralları korundu.
2. **Düşman Çeşitliliği & Gotik Bölge İlerlemesi (Bölge I, II ve III):**
   - Bölge I (Terk Edilmiş Harabeler), Bölge II (Kadim Kripta), Bölge III (Kan Katedrali).
   - 3 farklı minyon ve 3 farklı Boss için dinamik havuzlama (`NodePool<Enemy>`) doku güncellemesi.
   - `MainHUD` üzerinde dinamik Bölge ve Boss başlıkları.
3. **Görsel & Sprite Pipeline'ı:** Kahraman (Kan Şövalyesi), Pet Yoldaş (Kan Kargası) ve 3 katmanlı paralaks arka planı (`SkyLayer`, `RuinsLayer`, `ForegroundLayer`).
4. **Malikane Kütüphanesi & Araştırma Ağacı:** 3 disiplin, 9 araştırma düğümü, parşömen düşüşleri ve `LibraryModal`.
5. **Kadim Yadigar Mahzeni & Uyanış (Rebirth):** 10 adet Boss yadigarı, Dalga 20+ prestij eşiği ve yetenek ağacı.
6. **Ses Mimarisi:** Çift kanallı BGM crossfade, 6 kanallı döngüsel SFX havuzu ve Gotik `SettingsModal`.
7. **Nesne Havuzu & Performans:** GC sıfırlama (Yüzen metinler, mermiler, vfx, minyonlar).
8. **Birim Testleri:** 68/68 xUnit testi başarılı (`dotnet test`).
9. **Derleme & Motor:** `dotnet build` 0 warning / 0 error; Godot headless smoke test 0 hata / 0 sızıntı.

---

## 🎯 Kesinleşen Sıradaki Hedef: 3. Adım (Yeni Sohbette Uygulanacak)
### **3. Adım: Öğretici & İlk Kullanıcı Deneyimi (Onboarding / FTUE)**
- **Kullanıcının Seçtiği Yaklaşım:** **Yaklaşım 1 (Bağımsız `TutorialController` & Dinamik İşaretçi Overlay'i)**
  - `MainHUD.cs` satır sayısına dokunulmayacak (248/250 satır sınırı katı şekilde korunacak).
  - Tamamen ayrık (decoupled) `TutorialController` ve `TutorialHintCallout` bileşeni.
  - Oyun akışını durdurmayan, hafif nabız gibi atan (pulsing) gotik işaretçi ok ve ipucu balonu (non-intrusive pointers).
  - İlerleme `SaveData.TutorialStep` (0: Yok, 1: Tıkla&Vur, 2: Stat Yükselt, 3: Öfkeyi Serbest Bırak, 4: Malikane Kapısı, 5: Tamamlandı) olarak kaydedilecek.
- **Kullanıcının Onayladığı 4 Temel Adım:**
  1. **Ekrana Tıkla & Vur:** Dalga 1 başında savaş alanına işaret eden dinamik vuruş yönlendirmesi (oyuncu tıkladıkça sayar, ilk minyon ölünce söner).
  2. **Saldırı Gücünü Yükselt:** 10+ altın biriktiğinde `UpgradeAtkBtn` üzerine hafif parıldayan ok & ipucu.
  3. **Öfkeyi Serbest Bırak (Berserk):** Öfke %100'e ulaştığında `RageButton` üzerine dikkat çeken gotik alevli işaretçi.
  4. **Malikane Kapısını Ziyaret Et:** Dalga 5 temizlendiğinde beliren `GateNotificationBtn` üzerine yönlendirme.
- **Doğrulama Durumu:** 68/68 birim testi geçiyor, `dotnet build` 0 hata/0 uyarı, `developer` dalı temiz.

